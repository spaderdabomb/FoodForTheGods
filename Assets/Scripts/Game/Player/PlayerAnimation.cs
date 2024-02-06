using Medicine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FoodForTheGods.Player
{
	public class PlayerAnimation : MonoBehaviour
	{
		[field: SerializeField] public PlayerMovementAnimationState currentMovementAnimationState { get; private set; } = PlayerMovementAnimationState.Idling;

		[Inject]
		public Animator Animator { get; } = null!;

		[Inject]
		public CharacterController CharacterController { get; } = null!;

		[Inject]
		public PlayerController PlayerController { get; } = null!;

		public void UpdateAnimationState(bool isMovingLaterally, bool isSprinting, bool isGrounded)
		{

			if (isSprinting && isGrounded)
				currentMovementAnimationState = PlayerMovementAnimationState.Sprinting;
			else if (isMovingLaterally && !isGrounded && CharacterController.velocity.y >= 0f)
				currentMovementAnimationState = PlayerMovementAnimationState.RunningJumpUp;
			else if (isMovingLaterally && !isGrounded && CharacterController.velocity.y < 0f)
				currentMovementAnimationState = PlayerMovementAnimationState.JumpingDown;
			else if (!isMovingLaterally && !isGrounded && CharacterController.velocity.y >= 0f)
				currentMovementAnimationState = PlayerMovementAnimationState.JumpingUp;
			else if (!isMovingLaterally && !isGrounded && CharacterController.velocity.y < 0f)
				currentMovementAnimationState = PlayerMovementAnimationState.JumpingDown;
			else if (isMovingLaterally && !isSprinting && isGrounded)
				currentMovementAnimationState = PlayerMovementAnimationState.Running;
			else if (!isMovingLaterally && isGrounded)
				currentMovementAnimationState = PlayerMovementAnimationState.Idling;
			else
			{
				print("catch all");
				print($"isMovingLaterally: {isMovingLaterally}");
				print($"isSprinting: {isSprinting}");
				print($"isGrounded: {isGrounded}");
			}

			UpdateAnimator();
		}

		private void UpdateAnimator()
		{
			AnimatorClipInfo[] currentClipInfo = Animator.GetCurrentAnimatorClipInfo(0);
			string currentClipName = currentClipInfo[0].clip.name;

			Animator.SetInteger("currentMoveState", (int)currentMovementAnimationState);
		}
	}

	public enum PlayerMovementAnimationState
	{
		Idling = 0,
		Running = 1,
		Sprinting = 2,
		JumpingUp = 3,
		JumpingDown = 4,
		RunningJumpUp = 5,
	}
}
