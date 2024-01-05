
#nullable enable
using UnityEngine.UIElements;

namespace FoodForTheGods.Ui
{
	/// <summary>
	/// An interface for UI elements that are managed by the <see cref="UiElementManager"/>.
	/// This should be attached to any <see cref="UnityEngine.MonoBehaviour"/> or NetworkBehaviour that will have a UI element attached to it.
	/// </summary>
	public interface IUiElement
	{
		/// <summary>
		/// Whether or not the cursor should be shown when this element is shown to the user.
		/// </summary>
		public bool ShowCursor { get; }

		/// <summary>
		/// Whether or not to hide the element using the <see cref="DisplayStyle"/> property of the root visual element.
		/// Instead of disabling the <see cref="UIDocument"/> component. This is useful for elements that need to be
		/// active in order to be able to receive events, but should not be visible. Or for elements that should always be
		/// retained in the visual tree.
		/// </summary>
		public bool HideUsingDisplayStyle { get; }

		/// <summary>
		/// Make sure to add [field: SerializeField] to the property in the implementing class if you want it to be serialized in the inspector.
		/// Otherwise, you will need to manually set the UI Document at runtime.
		/// </summary>
		public UIDocument UIDocument { get; set; }

		/// <summary>
		/// Called when the <see cref="IUiElement"/> is shown.
		/// </summary>
		public void OnShowUiElement();

		/// <summary>
		/// Called when the <see cref="IUiElement"/> is hidden.
		/// </summary>
		public void OnHideUiElement();
	}
}