
#nullable enable
using FishNet.Component.Transforming;
using FishNet.Object;
using FoodForTheGods.Input;
using Medicine;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace FoodForTheGods.Player
{
	[RequireComponent(typeof(Rigidbody), typeof(NetworkTransform))]
	public class PlayerController : NetworkBehaviour, MainInput.IPlayerActions
	{
		// this is Owner only, the server and other clients will not have access to this.
		// (@todo: Input manager to allow server to control player input too?)
		private MainInput? m_MainInput = null;
		public MainInput MainInput => m_MainInput ??= new MainInput();

		// (owner only, not networked to server or other clients)
		private Vector2 m_MovementInput = Vector2.zero;
		private Vector2 m_LookInput = Vector2.zero;

		[Inject]
		public Rigidbody Rigidbody { get; } = null!;

		[Inject.FromChildren]
		public Camera Camera { get; } = null!;

		public override void OnStartClient()
		{
			base.OnStartClient();

			if (!IsOwner)
			{
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

		private void FixedUpdate()
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

			Vector3 movement = new Vector3(m_MovementInput.x, 0f, m_MovementInput.y);

			movement = Camera.transform.TransformDirection(movement);

			Rigidbody.AddForce(movement * 10f, ForceMode.Acceleration);
		}

		private void TickLook()
		{
			Assert.IsTrue(IsOwner);

			Vector3 look = new Vector3(m_LookInput.x, 0f, m_LookInput.y);

			gameObject.transform.Rotate(look);

			Camera.transform.localRotation = Quaternion.Euler(Camera.transform.localRotation.eulerAngles.x - look.y, 0f, 0f);
		}
	}
}