
using FishNet.Object;
using FoodForTheGods.Game.Components;
using FoodForTheGods.Player;
using UnityEngine;

namespace FoodForTheGods.Items
{
	public class GroundItem : NetworkBehaviour, IInteractionComponent
	{
		public InteractionComponent InteractionComponent { get; } = new();

		public override void OnStartNetwork()
		{
			base.OnStartNetwork();

			InteractionComponent.AddInteraction(
				"Grab",
				CanInteractGrab,
				OnInteractGrab
			);
		}

		private bool OnInteractGrab(PlayerController player, bool asServer)
		{
			return true;
		}

		private bool CanInteractGrab(PlayerController player, bool asServer)
		{
			return true;
		}

#if UNITY_EDITOR
		protected override void OnValidate()
		{
			base.OnValidate();

			if (gameObject.layer != LayerMask.NameToLayer("Item"))
			{
				gameObject.layer = LayerMask.NameToLayer("Item");
			}
		}
#endif
	}
}