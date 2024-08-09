using Deenote.Edit;
using Deenote.GameStage;
using Deenote.Settings;
using Deenote.UI.Windows.Components;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.Windows
{
    [RequireComponent(typeof(Window))]
    public sealed partial class PerspectiveViewWindow : MonoBehaviour
    {
        [SerializeField] Window _window;
        [SerializeField] GameStageViewArgs _args;

        public Window Window => _window;

        [Header("Notify")]
        [SerializeField] GameStageController _gameStageController;
        [SerializeField] EditorController _editorController;

        [Header("UI")]
        [SerializeField] Button _fullScreenButton;
        [SerializeField] WindowDropdown _aspectDropdown;

        private void Awake()
        {
            _fullScreenButton.onClick.AddListener(() => _gameStageController.PerspectiveView.SetFullScreenState(true));
            _aspectDropdown.Dropdown.onValueChanged.AddListener(OnAspectChanged);

            AwakeStageUI();

            _window.SetOnFirstActivating(OnFirstActivating);
            _window.SetOnIsActivatedChanged(activated => { if (activated) OnWindowActivated(); });
        }

        private void OnFirstActivating()
        {
            _aspectDropdown.ResetOptions(PerspectiveViewController.EnumExt.ViewAspectDropdownOptions);
            _aspectDropdown.Dropdown.SetValueWithoutNotify(PerspectiveViewController.EnumExt.ToDropdownIndex(_gameStageController.PerspectiveView.AspectRatio));
        }

        #region Event

        private void OnAspectChanged(int value)
        {
            var aspectRatio = PerspectiveViewController.EnumExt.FromDropdownIndex(value);
            _gameStageController.PerspectiveView.AspectRatio = aspectRatio;
        }

        #endregion

        private void OnWindowActivated()
        {
            NotifyChartChanged(
                MainSystem.ProjectManager.CurrentProject,
                MainSystem.GameStage.Chart);
        }

        #region Notify

        public void NotifyAspectRatioChanged(PerspectiveViewController.ViewAspectRatio aspectRatio)
        {
            _window.FixedAspectRatio = PerspectiveViewController.EnumExt.GetRatio(aspectRatio);
        }

        #endregion
    }
}
