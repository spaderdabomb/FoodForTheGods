
#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Medicine;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

namespace FoodForTheGods.Ui
{
	[Register.Single]
	public class UiElementManager : MonoBehaviour
	{
		/// <summary>
		/// The currently visible elements.
		/// </summary>
		public readonly HashSet<IUiElement>VisibleElements = new();

		/// <summary>
		/// The elements that are temporarily hidden and aren't fully destroyed.
		/// </summary>
		public readonly Dictionary<IUiElement, StyleEnum<DisplayStyle>> TempHiddenElements = new();

		/// <summary>
		/// A temporary list of elements that are hidden when <see cref="HideAllElements"/> is called.
		/// This is different from <see cref="TempHiddenElements"/> as it is only used for the <see cref="HideAllElements"/> method.
		/// </summary>
		private Dictionary<IUiElement, StyleEnum<DisplayStyle>> m_AllTempHiddenElements = new();

		/// <summary>
		/// Whehter or not the cursor is currently visible.
		/// </summary>
		public static bool IsCursorVisible => s_CursorStack > 0;

		/// <summary>
		/// The current cursor stack, if this is 0, the cursor is hidden otherwise it is considered visible.
		/// </summary>
		private static byte s_CursorStack;

		/// <summary>
		/// Pushes the cursor onto the stack, making it visible if it wasn't already.
		/// Otherwise, it will just increment the stack count.
		/// </summary>
		public static void PushCursor()
		{
			s_CursorStack++;

			if (s_CursorStack > 1)
			{
				return;
			}

			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.Confined;
		}

		/// <summary>
		/// Pops the cursor off the stack, hiding it if the <see cref="s_CursorStack"/> is 0.
		/// Otherwise, it will just decrement the stack count.
		/// </summary>
		/// <param name="caller">The name of the caller, used for debugging.</param>
		public static void PopCursor([CallerMemberName] string? caller = null)
		{
			if (s_CursorStack <= 0)
			{
				Debug.LogError($"Tried to pop cursor when it was already hidden! (Caller: {caller})");
				return;
			}

			s_CursorStack--;

			if (s_CursorStack > 0)
			{
				return;
			}

			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
		}

		/// <summary>
		/// Shows all <see cref="IUiElement"/>s that were hidden using <see cref="HideAllElements"/>.
		/// </summary>
		public void ShowAllElements()
		{
			foreach ((IUiElement? uiElement, StyleEnum<DisplayStyle> displayStyle) in m_AllTempHiddenElements)
			{
				uiElement.UIDocument.rootVisualElement.style.display = displayStyle;

				m_AllTempHiddenElements.Remove(uiElement);
				VisibleElements.Add(uiElement);

				if (uiElement.ShowCursor)
				{
					PushCursor();
				}
			}
		}

		/// <summary>
		/// Hides all <see cref="IUiElement"/>s that are currently visible.
		/// </summary>
		public void HideAllElements()
		{
			foreach (IUiElement uiElement in VisibleElements)
			{
				m_AllTempHiddenElements[uiElement] = uiElement.UIDocument.rootVisualElement.style.display;

				uiElement.UIDocument.rootVisualElement.style.display = DisplayStyle.None;
				VisibleElements.Remove(uiElement);

				if (uiElement.ShowCursor)
				{
					PopCursor();
				}
			}
		}

		/// <summary>
		/// Called when the script instance is being loaded.
		/// </summary>
		private void Awake()
		{
			// we don't want to destory this object when a new scene is loaded
			// this is permanent throughout the game's entire life cycle
			DontDestroyOnLoad(gameObject);
		}

		/// <summary>
		/// Handles when the application focus changes and properly disables cursor focus, if the game is not in focus.
		/// </summary>
		/// <param name="hasFocus">Whether or not the game is in focus.</param>
		private void OnApplicationFocus(bool hasFocus)
		{
			// @Todo: Re-implement this when Network Manager is implemented.
			return;

			if (hasFocus)
			{
				Cursor.visible = IsCursorVisible;
				Cursor.lockState = IsCursorVisible ? CursorLockMode.Confined : CursorLockMode.Locked;
			}
			else
			{
				Cursor.visible = false;
				Cursor.lockState = CursorLockMode.None;
			}
		}

		/// <summary>
		/// As some properties are static, we need to reset them when the game has ended. As the Unity Editor
		/// does not automatically reset static variables until a recompile. So we need to manually reset them.
		/// </summary>
		private void OnDestroy()
		{
			s_CursorStack = 0;
			VisibleElements.Clear();
			TempHiddenElements.Clear();
		}
	}

	public static class UiElementManagerExtensions
	{
		[Inject.Single]
		private static UiElementManager UiElementManager { get; } = null!;

		public static void ShowUiElement(this IUiElement uiElement)
		{
#if UNITY_EDITOR || DEBUG
			if (uiElement.UIDocument == null)
			{
				Debug.LogError($"{nameof(UiElementManager)} tried to show {uiElement} when it's {nameof(UIDocument)} was null!");
				return;
			}
#endif

			if (UiElementManager.TempHiddenElements.ContainsKey(uiElement))
			{
				uiElement.UIDocument.rootVisualElement.style.display = UiElementManager.TempHiddenElements[uiElement];

				UiElementManager.TempHiddenElements.Remove(uiElement);
				UiElementManager.VisibleElements.Add(uiElement);

				if (uiElement.ShowCursor)
				{
					UiElementManager.PushCursor();
				}

				return;
			}

			if (UiElementManager.VisibleElements.Contains(uiElement))
			{
				Debug.Log($"{nameof(UiElementManager)} tried to show {uiElement} when it was already visible!");
				return;
			}

			if (uiElement.ShowCursor)
			{
				UiElementManager.PushCursor();
			}

			if (!uiElement.UIDocument.isActiveAndEnabled)
			{
				uiElement.UIDocument.enabled = true;

				MethodInfo? assignQueryResultsMethod = uiElement.GetType().GetMethod("AssignQueryResults", BindingFlags.NonPublic | BindingFlags.Instance);
				assignQueryResultsMethod?.Invoke(uiElement, new object[] {uiElement.UIDocument.rootVisualElement});
			}

			if (uiElement.HideUsingDisplayStyle)
			{
				uiElement.UIDocument.rootVisualElement.style.display = DisplayStyle.Flex;
			}

			uiElement.OnShowUiElement();
			UiElementManager.VisibleElements.Add(uiElement);
		}

		public static void HideUiElement(this IUiElement uiElement, bool bTempHide = false)
		{
			if (!UiElementManager.VisibleElements.Contains(uiElement) && !UiElementManager.TempHiddenElements.ContainsKey(uiElement))
			{
				// element is not visible, so we can't hide it
				return;
			}

			if (bTempHide)
			{
				UiElementManager.TempHiddenElements[uiElement] = uiElement.UIDocument.rootVisualElement.style.display;

				uiElement.UIDocument.rootVisualElement.style.display = DisplayStyle.None;
				UiElementManager.VisibleElements.Remove(uiElement);

				if (uiElement.ShowCursor)
				{
					UiElementManager.PopCursor();
				}

				return;
			}

			if (UiElementManager.TempHiddenElements.ContainsKey(uiElement))
			{
				// if we were tempoarily hidden but now we're fully hidden, remove from the temp hidden list
				UiElementManager.TempHiddenElements.Remove(uiElement);
			}

			if (uiElement.ShowCursor)
			{
				UiElementManager.PopCursor();
			}

			uiElement.OnHideUiElement();

			if (uiElement.UIDocument.isActiveAndEnabled && !uiElement.HideUsingDisplayStyle)
			{
				uiElement.UIDocument.enabled = false;
			}
			else if (uiElement.HideUsingDisplayStyle)
			{
				uiElement.UIDocument.rootVisualElement.style.display = DisplayStyle.None;
			}

			UiElementManager.VisibleElements.Remove(uiElement);
		}

		public static bool IsUiElementVisible(this IUiElement uiElement)
		{
			return UiElementManager.VisibleElements.Contains(uiElement);
		}
	}
}