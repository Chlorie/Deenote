using Deenote.Edit;
using Deenote.GameStage;
using Deenote.Settings;
using Deenote.UI.Windows.Components;
using Deenote.Utilities;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.Windows
{
    [RequireComponent(typeof(Window))]
    public sealed partial class PerspectiveViewWindow : SingletonBehavior<PerspectiveViewWindow>
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

        public Vector2 ViewSize => PerspectiveViewController.Instance.ViewSize;

        protected override void Awake()
        {
            base.Awake();

            _fullScreenButton.onClick.AddListener(() => _gameStageController.PerspectiveView.SetFullScreenState(true));
            _aspectDropdown.Dropdown.onValueChanged.AddListener(OnAspectChanged);

            AwakeStageUI();

            _window.SetOnFirstActivating(OnFirstActivating);
            _window.SetOnIsActivatedChanged(activated =>
            {
                if (activated) OnWindowActivated();
            });
        }

        private void OnFirstActivating()
        {
            _aspectDropdown.ResetOptions(ViewAspectRatioExt.ViewAspectDropdownOptions.AsSpan());
            _aspectDropdown.Dropdown.SetValueWithoutNotify(PerspectiveViewController.Instance.AspectRatio
                .ToDropdownIndex());
        }

        #region Event

        private void OnAspectChanged(int value)
        {
            var aspectRatio = ViewAspectRatioExt.FromDropdownIndex(value);
            PerspectiveViewController.Instance.AspectRatio = aspectRatio;
        }

        #endregion

        private void OnWindowActivated()
        {
            NotifyChartChanged(
                MainSystem.ProjectManager.CurrentProject,
                MainSystem.GameStage.Chart);
        }

        #region Notify

        public void NotifyAspectRatioChanged(ViewAspectRatio aspectRatio)
        {
            _window.FixedAspectRatio = aspectRatio.GetRatio();
        }

        #endregion
    }
}