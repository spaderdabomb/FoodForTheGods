using FishNet.Object;
using FoodForTheGods.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace FoodForTheGods.Player
{
    public class PlayerState : NetworkBehaviour
    {
        [field: SerializeField] public PlayerMovementState currentMovementState { get; private set; } = PlayerMovementState.Idling;
        public override void OnStartClient()
        {
            base.OnStartClient();

            if (!IsOwner)
            {
                return;
            }
        }

        private void Update()
        {
            if (!IsOwner)
            {
                return;
            }
        }

        public void SetPlayerMovementState(PlayerMovementState state)
        {
            currentMovementState = state;
        }

        public void AddPlayerMovementState(PlayerMovementState state)
        {
            currentMovementState |= state;
        }

        public void RemovePlayerMovementState(PlayerMovementState state)
        {
            currentMovementState &= state;
        }
    }

    [Flags]
    public enum PlayerMovementState
    {
        Idling = 0,
        Walking = 1,
        Sprinting = 2,
        JumpingUp = 4,
        JumpingDown = 8,
    }
}
