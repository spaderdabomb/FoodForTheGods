
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FoodForTheGods.Player;

namespace FoodForTheGods.Game.Extensions
{
	public static class NetworkObjectExtensions
	{
		/// <summary>
		/// Whether or not the given <see cref="PlayerController"/> is the local player. i.e. owned by this client.
		/// </summary>
		/// <param name="player">The <see cref="PlayerController"/> to check.</param>
		/// <returns>True if the <see cref="PlayerController"/> is the local player.</returns>
		public static bool IsLocalPlayer(this PlayerController player)
		{
			return player.NetworkObject.IsOwner;
		}

		public static bool IsLocalPlayer(this NetworkBehaviour networkBehaviour)
		{
			return networkBehaviour.Owner.IsLocalClient;
		}

		public static bool IsLocalPlayer(this NetworkObject networkObject)
		{
			return networkObject.Owner.IsLocalClient;
		}

		/// <summary>
		/// Whether or not the <see cref="PlayerController"/> is the owner of this <see cref="NetworkObject"/>.
		/// </summary>
		/// <param name="networkObject">The <see cref="NetworkObject"/> to check.</param>
		/// <param name="player">The <see cref="PlayerController"/> to check.</param>
		/// <returns>True if the <see cref="PlayerController"/> is the owner of the <see cref="NetworkObject"/>.</returns>
		public static bool IsOwner(this NetworkObject networkObject, PlayerController player)
		{
			return networkObject.Owner.ClientId == player.NetworkObject.Owner.ClientId;
		}

		/// <summary>
		/// <inheritdoc cref="IsOwner(NetworkObject,PlayerController)"/>
		/// </summary>
		public static bool IsOwner(this NetworkObject networkObject, NetworkConnection connection)
		{
			return networkObject.Owner.ClientId == connection.ClientId;
		}

		/// <summary>
		/// <inheritdoc cref="IsOwner(NetworkObject,PlayerController)"/>
		/// </summary>
		public static bool IsOwner(this NetworkBehaviour networkBehaviour, PlayerController player)
		{
			return networkBehaviour.Owner.ClientId == player.NetworkObject.Owner.ClientId;
		}

		/// <summary>
		/// <inheritdoc cref="IsOwner(NetworkObject,PlayerController)"/>
		/// </summary>
		public static bool IsOwner(this NetworkBehaviour networkBehaviour, NetworkConnection connection)
		{
			return networkBehaviour.Owner.ClientId == connection.ClientId;
		}

		/// <summary>
		/// Whether or not the <see cref="NetworkObject"/> has an owner.
		/// </summary>
		/// <param name="networkObject">The <see cref="NetworkObject"/> to check.</param>
		/// <returns>True if the <see cref="NetworkObject"/> has an owner.</returns>
		public static bool HasOwner(this NetworkObject networkObject)
		{
			return networkObject.Owner.ClientId != NetworkManager.EmptyConnection.ClientId;
		}

		/// <summary>
		/// <inheritdoc cref="HasOwner(NetworkObject)"/>
		/// </summary>
		public static bool HasOwner(this NetworkBehaviour networkBehaviour)
		{
			return networkBehaviour.Owner.ClientId != NetworkManager.EmptyConnection.ClientId;
		}
	}
}