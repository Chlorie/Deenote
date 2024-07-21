using Cysharp.Threading.Tasks;
using Deenote.UI.Windows;
using Deenote.Utilities;
using UnityEngine;

namespace Deenote.UI.MenuBar
{
    public sealed  class MenuBarController : MonoBehaviour
    {
        [Header("File")]
        [SerializeField] MenuDropdownItemController _newDropdownItem;
        [SerializeField] MenuDropdownItemController _openDropdownItem;
        [SerializeField] MenuDropdownItemController _saveDropdownItem;
        [SerializeField] MenuDropdownItemController _saveAsDropdownItem;
        [SerializeField] MenuDropdownItemController _quitDropdownItem;
        [Header("Edit")]
        [SerializeField] MenuDropdownItemController _copyButton;
        [Header("View")]
        [SerializeField] MenuDropdownItemController _perspectiveViewDropdownItem;
        [SerializeField] MenuDropdownItemController _propertiesViewDropdownItem;
        [SerializeField] MenuDropdownItemController _editorPropertiesViewDropdownItem;
        [SerializeField] MenuDropdownItemController _pianoSoundEditViewDropdownItem;
        [SerializeField] MenuDropdownItemController _toolBarDropdownItem;
        [Header("Settings")]
        [SerializeField] MenuDropdownItemController _preferencesDropdownItem;
        [SerializeField] MenuDropdownItemController _aboutDropdownItem;
        [SerializeField] MenuDropdownItemController _updateHistoryDropdownItem;
        [SerializeField] MenuDropdownItemController _tutorialsDropdownItem;
        [SerializeField] MenuDropdownItemController _checkUpdateDropdownItem;

        public bool IsHovering { get; set; }

        private void Awake()
        {
            // File
            _newDropdownItem.Button.onClick.AddListener(MainSystem.ProjectManager.CreateNewProjectAsync);
            _openDropdownItem.Button.onClick.AddListener(MainSystem.ProjectManager.OpenProjectAsync);
            _saveDropdownItem.Button.onClick.AddListener(MainSystem.ProjectManager.SaveProjectAsync);
            _saveAsDropdownItem.Button.onClick.AddListener(MainSystem.ProjectManager.SaveAsAsync);
            _quitDropdownItem.Button.onClick.AddListener(OnQuitAsync);
            //TODO export

            // View
            _perspectiveViewDropdownItem.Button.onClick.AddListener(() => MainSystem.PerspectiveView.Window.IsActivated = true);
            _propertiesViewDropdownItem.Button.onClick.AddListener(() => MainSystem.PropertiesWindow.Window.IsActivated = true);
            _editorPropertiesViewDropdownItem.Button.onClick.AddListener(() => MainSystem.EditorProperties.Window.IsActivated = true);
            _pianoSoundEditViewDropdownItem.Button.onClick.AddListener(() => MainSystem.PianoSoundEdit.Window.IsActivated = true);
            _toolBarDropdownItem.Button.onClick.AddListener(() => MainSystem.ToolBar.IsActivated = !MainSystem.ToolBar.IsActivated);

            // Settings
            _preferencesDropdownItem.Button.onClick.AddListener(() => MainSystem.PreferenceWindow.Window.IsActivated = true);
            _aboutDropdownItem.Button.onClick.AddListener(() => MainSystem.AboutWindow.OpenWindow(AboutWindow.AboutPage.AboutDevelopers));
            _tutorialsDropdownItem.Button.onClick.AddListener(() => MainSystem.AboutWindow.OpenWindow(AboutWindow.AboutPage.Tutorials));
            _updateHistoryDropdownItem.Button.onClick.AddListener(() => MainSystem.AboutWindow.OpenWindow(AboutWindow.AboutPage.UpdateHistory));
            _checkUpdateDropdownItem.Button.onClick.AddListener(async () => await MainSystem.VersionManager.CheckForUpdateAsync(true, true));
        }

        private async UniTaskVoid OnQuitAsync()
        {
            if (await MainSystem.ConfirmQuitAsync()) {
                MainSystem.QuitApplication();
            }
        }
    }
}
