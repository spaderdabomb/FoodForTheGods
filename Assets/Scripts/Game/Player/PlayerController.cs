
#nullable enable
using FishNet.Component.Transforming;
using FishNet.Example.ColliderRollbacks;
using FishNet.Object;
using FoodForTheGods.Input;
using Medicine;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace FoodForTheGods.Player
{
	[RequireComponent(typeof(CharacterController), typeof(NetworkTransform))]
	public class PlayerController : NetworkBehaviour, MainInput.IPlayerActions
	{
		// this is Owner only, the server and other clients will not have access to this.
		// (@todo: Input manager to allow server to control player input too?)
		private MainInput? m_MainInput = null;
		public MainInput MainInput => m_MainInput ??= new MainInput();

		// (owner only, not networked to server or other clients)
		private Vector2 m_MovementInput = Vector2.zero;
		private Vector2 m_LookInput = Vector2.zero;
		private Vector2 m_PlayerRotation = Vector2.zero;
		private Vector2 m_CameraRotation = Vector2.zero;
        private Vector3 m_PlayerVelocity = Vector3.zero;
        private Vector3 m_PositionLastFrame = Vector3.zero;

        private bool m_IsGrounded = false;
		private bool m_IsMoving = false;
		private float m_MovingThreshold = 0.01f;

		[Header("Base Movement")]
		[SerializeField] private float m_WalkSpeed = 0.5f;
		[SerializeField] private float m_Gravity = 9.81f;
		[SerializeField] private float m_JumpForce = 5f;
		[SerializeField] private float m_LookSenseH = 0.1f;
		[SerializeField] private float m_LookSenseV = 0.1f;
		[SerializeField] private float m_LookLimitV = 89f;

		[Header("Environment Details")]
		[SerializeField] private LayerMask m_GroundLayers = null;

		[Inject]
		public Transform Transform { get; } = null!;

		[Inject]
		public CharacterController CharacterController { get; } = null!;

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

		public void OnLook(InputAction.CallbackContext context)
		{
			m_LookInput = context.ReadValue<Vector2>();
		}

		public void OnMovement(InputAction.CallbackContext context)
		{
			m_MovementInput = context.ReadValue<Vector2>();
		}

		private void Update()
		{
			if (!IsOwner)
			{
				return;
			}

			m_IsGrounded = IsGrounded();

			TickMovement();

			m_IsMoving = IsMoving();
		}

		private void LateUpdate()
		{
			if (!IsOwner)
			{
				return;
			}

			TickLook();

			m_PositionLastFrame = transform.position;
        }

		private bool IsGrounded()
		{
            bool isGrounded = Physics.CapsuleCast(
                transform.position + Vector3.up * 0.1f, 
				transform.position + Vector3.down * (CharacterController.height - 0.1f),
                CharacterController.radius, 
				Vector3.down, 
				0.1f, 
				m_GroundLayers
            );

			return isGrounded;
        }

		private bool IsMoving()
		{
			m_PlayerVelocity = (transform.position - m_PositionLastFrame) / Time.deltaTime;

			return (m_PlayerVelocity.magnitude > m_MovingThreshold) ? true : false;
        }

		private void TickMovement()
		{
			Assert.IsTrue(IsOwner);

			Vector3 movement = Transform.right * m_MovementInput.x + Transform.forward * m_MovementInput.y;
			CharacterController.Move(movement * (m_WalkSpeed * Time.deltaTime));
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
	}
}