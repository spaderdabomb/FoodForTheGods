
using FoodForTheGods.Player;

namespace FishNet.Object
{
	public static class NetworkObjectExtensions
	{
		public static bool IsOwner(this NetworkObject networkObject, PlayerController player)
		{
			return networkObject.Owner.ClientId == player.NetworkObject.Owner.ClientId;
		}

		public static bool HasOwner(this NetworkObject networkObject)
		{
			return networkObject.Owner.ClientId != Managing.NetworkManager.EmptyConnection.ClientId;
		}
	}
}