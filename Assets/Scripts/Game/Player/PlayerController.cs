
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

		[Header("Base Movement")]
		[SerializeField] float walkSpeed = 0.5f;
		[SerializeField] float gravity = 9.81f;
		[SerializeField] float jumpForce = 5f;
		[SerializeField] float lookSenseH = 0.1f;
		[SerializeField] float lookSenseV = 0.1f;
		[SerializeField] float lookLimitV = 89f;

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

			TickMovement();
		}

		private void LateUpdate()
		{
			if (!IsOwner)
			{
				return;
			}

			TickLook();
		}

		private void TickMovement()
		{
			Assert.IsTrue(IsOwner);

			Vector3 movement = Transform.right * m_MovementInput.x + Transform.forward * m_MovementInput.y;
			CharacterController.Move(movement * (walkSpeed * Time.deltaTime));
		}

		private void TickLook()
		{
			Assert.IsTrue(IsOwner);

			m_CameraRotation.x += lookSenseH * m_LookInput.x;
			m_CameraRotation.y = Mathf.Clamp(m_CameraRotation.y - lookSenseV * m_LookInput.y, -lookLimitV, lookLimitV);

			m_PlayerRotation.x += transform.eulerAngles.x + lookSenseH * m_LookInput.x;

			transform.rotation = Quaternion.Euler(0f, m_PlayerRotation.x, 0f);
			Camera.transform.rotation = Quaternion.Euler(m_CameraRotation.y, m_CameraRotation.x, 0f);

		}
	}
}