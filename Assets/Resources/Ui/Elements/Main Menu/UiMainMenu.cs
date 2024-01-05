
#nullable enable
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace FoodForTheGods.Ui.Elements
{
	public partial class UiMainMenu : MonoBehaviour, IUiElement
	{
		public bool ShowCursor => true;
		public bool HideUsingDisplayStyle => false;

		[field: SerializeField]
		public UIDocument UIDocument { get; set; } = null!;

		private void OnEnable()
		{
			this.ShowUiElement();
		}

		private void OnDisable()
		{
			this.HideUiElement();
		}

		public void OnShowUiElement()
		{
			playButton.clicked += OnPlayButtonClicked;
			optionsButton.clicked += OnOptionsButtonClicked;
			quitButton.clicked += OnQuitButtonClicked;
		}

		public void OnHideUiElement()
		{
			playButton.clicked -= OnPlayButtonClicked;
			optionsButton.clicked -= OnOptionsButtonClicked;
			quitButton.clicked -= OnQuitButtonClicked;
		}

		private void OnPlayButtonClicked()
		{
			SceneManager.LoadScene("DevScene");
		}

		private void OnOptionsButtonClicked()
		{
			Debug.Log("Options button clicked");
		}

		private void OnQuitButtonClicked()
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		}
	}
}