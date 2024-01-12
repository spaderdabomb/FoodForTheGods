
#nullable enable
using FishNet.Component.Transforming;
using FishNet.Example.ColliderRollbacks;
using FishNet.Object;
using FoodForTheGods.Input;
using GameKit.Utilities;
using Medicine;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.PlayerLoop;

namespace FoodForTheGods.Player
{
	[RequireComponent(typeof(CharacterController), typeof(NetworkTransform))]
	public class PlayerController : NetworkBehaviour, MainInput.IPlayerActions
	{
		// this is Owner only, the server and other clients will not have access to this.
		// (@todo: Input manager to allow server to control player input too?)
		private MainInput? m_MainInput = null;
		public MainInput MainInput => m_MainInput ??= new MainInput();

		private Vector2 m_MovementInput = Vector2.zero;
		private Vector2 m_LookInput = Vector2.zero;
		private Vector2 m_PlayerRotation = Vector2.zero;
		private Vector2 m_CameraRotation = Vector2.zero;

		[SerializeField] private bool m_IsMovingLaterally = false;
		[SerializeField] private bool m_IsSprinting = false;

		private bool m_JumpPressed = false;
		private bool m_SprintPressed = false;
		private bool m_CanMove = false;
		private bool m_CanSprint = false;
		private bool m_CanJump = false;
		private float m_MovingThreshold = 0.01f;
		private float m_GroundedTimer = 0;
		private float m_VerticalVelocity = 0f;

		[Header("Base Movement")]
		[SerializeField] private float m_RunAcceleration = 0.5f;
		[SerializeField] private float m_RunSpeed = 10f;
		[SerializeField] private float m_SprintAcceleration = 1f;
		[SerializeField] private float m_SprintSpeed = 20f;
		[SerializeField] private float m_Drag = 0.25f;
		[SerializeField] private float m_Gravity = 25f;
		[SerializeField] private float m_JumpSpeed = 1.5f;

		[Header("Camera")]
		[SerializeField] private float m_LookSenseH = 0.1f;
		[SerializeField] private float m_LookSenseV = 0.1f;
		[SerializeField] private float m_LookLimitV = 89f;

		[Header("Settings")]
		[SerializeField] private bool m_HoldToSprint = true;

		[Header("Environment Details")]
		[SerializeField] private LayerMask m_GroundLayers;

		[Inject]
		public Transform Transform { get; } = null!;

		[Inject]
		public CharacterController CharacterController { get; } = null!;

		[Inject]
		public PlayerState PlayerState { get; } = null!;

        [Inject]
        public PlayerAnimation PlayerAnimation { get; } = null!;

        [Inject.FromChildren]
		public Camera Camera { get; set; } = null!;

		public override void OnStartClient()
		{
			base.OnStartClient();

			if (!IsOwner)
			{
				Camera.enabled = false;

				return;
			}

			MainInput.Player.SetCallbacks(this);
			MainInput.Player.Enable();
		}

        private void Update()
		{
			if (!IsOwner)
			{
				return;
			}

            ResetMovementProperties();

            m_CanJump = CanJump();		
			m_CanMove = CanMove();		//order
			m_CanSprint = CanSprint();  //matters

            m_IsMovingLaterally = IsMovingLaterally();								 //order
            m_IsSprinting = (m_CanSprint && m_SprintPressed && m_IsMovingLaterally); //matters

            TickMovement();
			UpdateMovementState();

        }

		private void LateUpdate()
		{
			if (!IsOwner)
			{
				return;
			}

			TickLook();
		}

		private void ResetMovementProperties()
		{
            m_IsMovingLaterally = false;
        }

		private void TickMovement()
        {
            Assert.IsTrue(IsOwner);

			HandleVerticalMovement();
            HandleLateralMovement();
        }

		private void HandleVerticalMovement()
		{
			// Setup grounded timer to prevent jumping in consecutive frames
			if (CharacterController.isGrounded)
				m_GroundedTimer = 0.2f;
			
			if (m_GroundedTimer > 0)
				m_GroundedTimer -= Time.deltaTime;
			
			if (CharacterController.isGrounded && m_VerticalVelocity < 0)
				m_VerticalVelocity = 0f;
			
			m_VerticalVelocity -= m_Gravity * Time.deltaTime;
			
			if (m_JumpPressed && m_CanJump)
			{
				if (m_GroundedTimer > 0)
				{
					m_VerticalVelocity += Mathf.Sqrt(m_JumpSpeed * 3 * m_Gravity);
					
					m_GroundedTimer = 0;
					m_JumpPressed = false;
				}
			}
		}

		private void HandleLateralMovement()
		{
			// State dependent acceleration and speed
			float currentLateralAcceleration = m_IsSprinting ? m_SprintAcceleration : m_RunAcceleration;
			float clampLateralVelocityMagnitude = m_IsSprinting ? m_SprintSpeed : m_RunSpeed;
			
			// Get lateral movement from input
			Vector3 movementDirection = Transform.right * m_MovementInput.x + Transform.forward * m_MovementInput.y;
			Vector3 movementDelta = movementDirection * currentLateralAcceleration;
			Vector3 lateralVelocity = new Vector3(CharacterController.velocity.x, 0f, CharacterController.velocity.z);
			Vector3 playerVelocityNew = Vector3.zero;
			
			playerVelocityNew += lateralVelocity + movementDelta;
			playerVelocityNew -= playerVelocityNew.normalized * m_Drag;
			playerVelocityNew = Vector3.ClampMagnitude(playerVelocityNew, clampLateralVelocityMagnitude);

			playerVelocityNew.y = m_VerticalVelocity;
			
			CharacterController.Move(playerVelocityNew * Time.deltaTime);
        }

		private void UpdateMovementState()
		{
			// Control Move State
			PlayerMovementState runOrSprintState = m_IsSprinting ? PlayerMovementState.Sprinting : PlayerMovementState.Running;
			PlayerState.SetPlayerMovementState(m_IsMovingLaterally ? runOrSprintState : PlayerMovementState.Idling);

			// Control Jump State
			if (!CharacterController.isGrounded && CharacterController.velocity.y >= 0f)
			{
				PlayerState.AddPlayerMovementState(PlayerMovementState.JumpingUp);
            }
			else if (!CharacterController.isGrounded && CharacterController.velocity.y < 0f)
			{
                PlayerState.AddPlayerMovementState(PlayerMovementState.JumpingDown);

            }

			PlayerAnimation.UpdateAnimationState(m_IsMovingLaterally, m_IsSprinting, CharacterController.isGrounded);

        }

        private void TickLook()
		{
			Assert.IsTrue(IsOwner);

			m_CameraRotation.x += m_LookSenseH * m_LookInput.x;
			m_CameraRotation.y = Mathf.Clamp(m_CameraRotation.y - m_LookSenseV * m_LookInput.y, -m_LookLimitV, m_LookLimitV);

			m_PlayerRotation.x += transform.eulerAngles.x + m_LookSenseH * m_LookInput.x;

			transform.rotation = Quaternion.Euler(0f, m_PlayerRotation.x, 0f);
			Camera.transform.rotation = Quaternion.Euler(m_CameraRotation.y, m_CameraRotation.x, 0f);
		}


        #region State Checks
        private bool IsMovingLaterally()
        {
			Vector3 lateralVelocity = new Vector3(CharacterController.velocity.x, 0f, CharacterController.velocity.z);

            return (lateralVelocity.magnitude > m_MovingThreshold) ? true : false;
        }

		private bool CanMove()
		{
			return true;
		}

        private bool CanSprint()
        {
            return m_CanMove;
        }

		private bool CanJump()
		{
			return CharacterController.isGrounded;
		}

        #endregion

        #region Input
        public void OnLook(InputAction.CallbackContext context)
        {
            m_LookInput = context.ReadValue<Vector2>();
        }

        public void OnMovement(InputAction.CallbackContext context)
        {
            m_MovementInput = context.ReadValue<Vector2>();
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                m_SprintPressed = m_HoldToSprint ? true : !m_IsSprinting;
            }
            else if (context.canceled)
            {
                m_SprintPressed = m_HoldToSprint ? false : m_SprintPressed;
            }
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;

            m_JumpPressed = true;
        }
        #endregion
    }
}