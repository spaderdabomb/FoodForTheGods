
using FishNet.Object;
using UnityEngine;

namespace FoodForTheGods.Items
{
	public class GroundItem : NetworkBehaviour
	{
		public override void OnStartClient()
		{
			base.OnStartClient();

			if (!IsOwner)
			{
				enabled = false;
				return;
			}
		}
		protected override void OnValidate()
		{
			base.OnValidate();

			if (gameObject.layer != LayerMask.NameToLayer("Item"))
			{
				gameObject.layer = LayerMask.NameToLayer("Item");
			}
		}
	}
}