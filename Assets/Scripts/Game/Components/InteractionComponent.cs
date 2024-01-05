
#nullable enable
using System;
using System.Collections.Generic;
using FoodForTheGods.Player;

namespace FoodForTheGods.Game.Components
{
	public struct Interaction
	{
		public uint InteractionId;

		public string DisplayName;

		/// <summary>
		/// The event that is fired when the player interacts with this interaction component. (Only fired if EventCanInteract returns true)
		/// <para>PlayerController is the local player that is interacting with this component.</para>
		/// <para>Bool is whether or not the event is being fired as the server.</para>
		/// <returns>
		///	<para>
		///		True = Pipe the event to the server,
		///		False = Don't pipe the event to the server.
		/// </para>
		/// </returns>
		/// </summary>
		public Func<PlayerController, bool, bool> EventInteract;

		/// <summary>
		/// The event that is fired when the player checks if they can interact with this interaction component.
		/// <para>PlayerController is the local player that is interacting with this component.</para>
		/// <para>Bool is whether or not the event is being fired as the server.</para>
		/// </summary>
		public Func<PlayerController, bool, bool> EventCanInteract;

		public Interaction(string name, Func<PlayerController, bool, bool> eventInteract, Func<PlayerController, bool, bool> eventCanInteract)
		{
			DisplayName = name;
			EventInteract = eventInteract;
			EventCanInteract = eventCanInteract;

			InteractionId = 0; // set by InteractionComponent
		}

		public override string ToString()
		{
			return $"[{DisplayName}, {InteractionId}]";
		}
	}

	public interface IInteractionComponent
	{
		public InteractionComponent InteractionComponent { get; }
	}

	public sealed class InteractionComponent
	{
		private HashSet<Interaction> m_Interactions = new();
		public HashSet<Interaction> Interactions => m_Interactions;

		public UInt64 InteractionComponentId { get; } = 0;
		private static UInt64 m_NextInteractionComponentId = 0;

		private uint m_NextInteractionId = 0;

		public InteractionComponent()
		{
			InteractionComponentId = m_NextInteractionComponentId++;
		}

		private void AddInteraction(Interaction interaction)
		{
			interaction.InteractionId = m_NextInteractionId++;

			// wrap eventInteract & eventCanInteract so if we're not firing as the server
			// then we need to ensure that the client is the local player and not the host
			// as the host will act as the server and client at the same time (this avoids duplicate event calls)
			Func<PlayerController, bool, bool> originalCanInteract = interaction.EventCanInteract;
			Func<PlayerController, bool, bool> wrappedCanInteract = (player, asServer) =>
			{
				if (asServer)
				{
					return originalCanInteract.Invoke(player, true);
				}

				return player.IsOwner && originalCanInteract.Invoke(player, false);
			};

			interaction.EventCanInteract = wrappedCanInteract;

			Func<PlayerController, bool, bool> originalInteract = interaction.EventInteract;
			Func<PlayerController, bool, bool> wrappedInteract = (player, asServer) =>
			{
				if (asServer)
				{
					return originalInteract.Invoke(player, true);
				}

				return player.IsOwner && originalInteract.Invoke(player, false);
			};

			interaction.EventInteract = wrappedInteract;

			m_Interactions.Add(interaction);
		}

		// ReSharper disable once ParameterHidesMember
		public void AddInteraction(string name, Func<PlayerController, bool, bool> eventCanInteract, Func<PlayerController, bool, bool> eventInteract)
		{
			AddInteraction(new Interaction(name, eventInteract, eventCanInteract));
		}

		public override string ToString()
		{
			return $"Component Id: {InteractionComponentId}, Interactions: {m_Interactions.Count}";
		}
	}
}