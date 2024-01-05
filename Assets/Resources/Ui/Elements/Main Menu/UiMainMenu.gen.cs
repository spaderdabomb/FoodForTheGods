// -----------------------
// script auto-generated
// any changes to this file will be lost on next code generation
// com.quickeye.ui-toolkit-plus ver: 3.0.3
// -----------------------
using UnityEngine.UIElements;

namespace FoodForTheGods.Ui.Elements
{
    partial class UiMainMenu
    {
        private GroupBox menuGroupBox;
        private Button playButton;
        private Button optionsButton;
        private Button quitButton;

        protected void AssignQueryResults(VisualElement root)
        {
            menuGroupBox = root.Q<GroupBox>("MenuGroupBox");
            playButton = root.Q<Button>("PlayButton");
            optionsButton = root.Q<Button>("OptionsButton");
            quitButton = root.Q<Button>("QuitButton");
        }
    }
}