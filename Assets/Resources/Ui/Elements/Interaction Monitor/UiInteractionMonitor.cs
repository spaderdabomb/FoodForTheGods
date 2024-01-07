
using System;
using System.Collections.Generic;
using System.Linq;
using FishNet.Object;
using FoodForTheGods.Game.Components;
using FoodForTheGods.Player;
using Medicine;
using UnityEngine.UIElements;
using UnityEngine;

namespace FoodForTheGods.Ui.Elements
{
	public partial class UiInteractionMonitor : NetworkBehaviour, IUiElement
	{
		public bool ShowCursor => false;
		public bool HideUsingDisplayStyle => true;

		[field: SerializeField]
		public UIDocument UIDocument { get; set; } = null!;

		[Inject.FromParents]
		private PlayerController m_LocalPlayerController { get; } = null!;

		private Camera m_PlayerCamera => m_LocalPlayerController.Camera;

		private InteractionComponent? m_InteractionComponent = null;
		private List<Interaction> m_Interactions = new();

		private Collider? HitCollider = null;

		public void OnShowUiElement()
		{
			UpdateInteractionText();
		}

		public void OnHideUiElement()
		{
		}

		private void UpdateInteractionText()
		{
			if (!this.IsUiElementVisible())
			{
				return;
			}

			if (m_Interactions.Count == 0)
			{
				return;
			}

			interactionNameLabel.text = m_Interactions.First().DisplayName;
		}

		public void Interact()
		{
			if (!m_LocalPlayerController.IsOwner)
			{
				return;
			}

			if (m_InteractionComponent == null || m_Interactions.Count == 0)
			{
				return;
			}

			Interaction interaction = m_Interactions.First();

			bool bPipeToServer = interaction.EventInteract(m_LocalPlayerController, false);

			if (bPipeToServer)
			{
				InteractServerRpc(m_InteractionComponent.InteractionComponentId, interaction.InteractionId);
			}
		}

		[ServerRpc]
		private void InteractServerRpc(UInt64 interactionComponentNetworkId, uint interactionId)
		{
			if (m_Interactions.Count == 0)
			{
				return;
			}

			// ReSharper disable once Unity.NoNullPropagation
			if (m_InteractionComponent?.InteractionComponentId != interactionComponentNetworkId)
			{
				return;
			}

			Interaction? interaction = m_Interactions.FirstOrDefault(x => x.InteractionId == interactionId);
			interaction?.EventInteract(m_LocalPlayerController, true);
		}

		private void LateUpdate()
		{
			if (!m_LocalPlayerController.IsOwner && !IsServer)
			{
				// only the local player and server should be checking what interaction component we're looking at and if we can interact with it
				return;
			}

			// if (NetworkManager.ServerTime.Time - m_LastCheckServerTime < m_TimeBetweenChecks)
			// {
			// 	// only check every 0.5 seconds, this server time to ensure the client & server are in sync
			// 	return;
			// }

			// m_LastCheckServerTime = NetworkManager.ServerTime.Time;

			RaycastHit hit;

			if (!Physics.Raycast(m_PlayerCamera.transform.position, m_PlayerCamera.transform.forward, out hit, 2.5f))
			{
				// we haven't hit anything so clear all data and hide the UI
				HitCollider = null;
				m_InteractionComponent = null;
				m_Interactions.Clear();

				if (m_LocalPlayerController.IsOwner && this.IsUiElementVisible())
				{
					this.HideUiElement();
				}

				return;
			}

			if (HitCollider == hit.collider)
			{
				// we hit the same collider, so if there was an interaction component we can just update the interactions
				// to ensure we're not showing interactions that are no longer available (e.g. states have changed)
				if (m_InteractionComponent == null)
				{
					return;
				}

				m_Interactions = GetAvailableInteractions(m_InteractionComponent);

				if (m_Interactions.Count == 0)
				{
					if (m_LocalPlayerController.IsOwner && this.IsUiElementVisible())
					{
						this.HideUiElement();
					}

					return;
				}

				UpdateInteractionText();
				return;
			}

			HitCollider = hit.collider;

			// we hit a new collider, so we need to check if it has an interaction component
			if (GetInteractionComponent(hit) is not { } interactionComponent)
			{
				// no interaction component so clear all data and hide the UI
				m_InteractionComponent = null;
				m_Interactions.Clear();

				if (m_LocalPlayerController.IsOwner && this.IsUiElementVisible())
				{
					this.HideUiElement();
				}

				return;
			}

			m_InteractionComponent = interactionComponent;
			m_Interactions = GetAvailableInteractions(m_InteractionComponent);

			// no interactions are available so clear hide the UI
			if (m_Interactions.Count == 0)
			{
				if (m_LocalPlayerController.IsOwner && this.IsUiElementVisible())
				{
					this.HideUiElement();
				}

				return;
			}

			if (!m_LocalPlayerController.IsOwner)
			{
				return;
			}

			if (!this.IsUiElementVisible())
			{
				this.ShowUiElement();
			}
		}

		private InteractionComponent? GetInteractionComponent(RaycastHit hit)
		{
			var networkBehaviours = hit.collider.GetComponentsNonAlloc<NetworkBehaviour>();

			if (networkBehaviours.Length != 0)
			{
				foreach (NetworkBehaviour networkBehaviour in networkBehaviours)
				{
					if (networkBehaviour is IInteractionComponent interactionComponent)
					{
						return interactionComponent.InteractionComponent;
					}
				}
			}

			var monoBehaviours = hit.collider.GetComponentsNonAlloc<MonoBehaviour>();

			if (monoBehaviours.Length != 0)
			{
				foreach (MonoBehaviour monoBehaviour in monoBehaviours)
				{
					if (monoBehaviour is IInteractionComponent interactionComponent)
					{
						return interactionComponent.InteractionComponent;
					}
				}
			}

			return null;
		}

		private List<Interaction> GetAvailableInteractions(InteractionComponent component)
		{
			List<Interaction> interactions = new();

			foreach (Interaction interaction in component.Interactions)
			{
				if (!interaction.EventCanInteract(m_LocalPlayerController, IsServer))
				{
					continue;
				}

				interactions.Add(interaction);
			}

			return interactions;
		}
	}
}