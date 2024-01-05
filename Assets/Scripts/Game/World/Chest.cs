
using FishNet.Object;
using FoodForTheGods.Game.Components;
using FoodForTheGods.Player;
using UnityEngine;

namespace FoodForTheGods.Game.World
{
	public class Chest : NetworkBehaviour, IInteractionComponent
	{
		public InteractionComponent InteractionComponent { get; } = new();

		public override void OnStartNetwork()
		{
			base.OnStartNetwork();

			InteractionComponent.AddInteraction(
				"Open",
				CanInteractOpen,
				InteractOpen
			);

		}

		private bool InteractOpen(PlayerController player, bool asServer)
		{
			Debug.Log($"InteractOpen({player}, {asServer})");
			return true;
		}

		private bool CanInteractOpen(PlayerController player, bool asServer)
		{
			return true;
		}
	}
}