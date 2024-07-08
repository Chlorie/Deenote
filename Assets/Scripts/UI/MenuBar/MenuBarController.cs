using Cysharp.Threading.Tasks;
using Deenote.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.MenuBar
{
    public sealed partial class MenuBarController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] ToggleGroup _toggleGroup;
        [Header("FileMenu")]
        [SerializeField] Button _newButton;
        [SerializeField] Button _openButton;
        [SerializeField] Button _saveButton;
        [SerializeField] Button _saveAsButton;
        [SerializeField] Button _quitButton;
        [Header("Edit")]
        [SerializeField] Button _copyButton;
        [Header("View")]
        [SerializeField] Button _perspectiveViewButton;
        [Header("Settings")]
        [SerializeField] Button _settingsButton;

        public bool IsHovering { get; set; }

        private void Awake()
        {
            _newButton.onClick.AddListener(MainSystem.ProjectManager.NewProjectAsync);
            _openButton.onClick.AddListener(MainSystem.ProjectManager.OpenProjectAsync);
            _saveButton.onClick.AddListener(MainSystem.ProjectManager.SaveProjectAsync);
            _saveAsButton.onClick.AddListener(MainSystem.ProjectManager.SaveAsAsync);
            _quitButton.onClick.AddListener(OnQuitAsync);
            //TODO export
        }

        private async UniTaskVoid OnQuitAsync()
        {
            if (await MainSystem.ConfirmQuitAsync()) {
                MainSystem.QuitApplication();
            }
        }
    }
}
