using System;
using Deenote.Edit;
using Deenote.GameStage;
using Deenote.Settings;
using Deenote.UI.Windows.Components;
using Deenote.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
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
        [SerializeField] Image _backgroundBreathingMaskImage;
        [SerializeField] RawImage _cameraViewRawImage;
        [SerializeField] RectTransform _cameraViewTransform;
        [SerializeField] Button _fullScreenButton;
        [SerializeField] WindowDropdown _aspectDropdown;

        [Header("Prefabs")]
        [SerializeField] Sprite _easyDifficultyIconSprite;
        [SerializeField] Color _easyLevelTextColor;
        [SerializeField] Sprite _normalDifficultyIconSprite;
        [SerializeField] Color _normalLevelTextColor;
        [SerializeField] Sprite _hardDifficultyIconSprite;
        [SerializeField] Color _hardLevelTextColor;
        [SerializeField] Sprite _extraDifficultyIconSprite;
        [SerializeField] Color _extraLevelTextColor;

        public Vector2 ViewSize => _viewSize.Value;
        public event Action<Vector2>? OnViewSizeChanged;

        private FrameCachedNotifyingProperty<Vector2> _viewSize = null!;

        private float _tryPlayResetTime;

        private int _judgedNoteCount;

        protected override void Awake()
        {
            base.Awake();

            Vector2 GetViewSize()
            {
                Vector3[] corners = new Vector3[4];
                _cameraViewTransform.GetWorldCorners(corners);
                return corners[2] - corners[0];
            }

            void ViewSizeChanged(Vector2 _, Vector2 newSize) => OnViewSizeChanged?.Invoke(newSize);

            RenderTexture texture = new(1280, 720, GraphicsFormat.R8G8B8A8_UNorm, GraphicsFormat.None);
            _cameraViewRawImage.texture = texture;
            _cameraViewRawImage.enabled = true;
            GameStageController.Instance.PerspectiveCamera.targetTexture = texture;
            _viewSize = new FrameCachedNotifyingProperty<Vector2>(GetViewSize);
            _viewSize.OnValueChanged += ViewSizeChanged;
            OnViewSizeChanged += ReplaceCameraRenderTexture;
            _fullScreenButton.onClick.AddListener(() => _gameStageController.PerspectiveView.SetFullScreenState(true));
            _aspectDropdown.Dropdown.onValueChanged.AddListener(OnAspectChanged);

            AwakeStageUI();

            _window.SetOnFirstActivating(OnFirstActivating);
            _window.SetOnIsActivatedChanged(activated => { if (activated) OnWindowActivated(); });
        }

        private void ReplaceCameraRenderTexture(Vector2 newTargetSize)
        {
            int width = Mathf.RoundToInt(newTargetSize.x), height = Mathf.RoundToInt(newTargetSize.y);
            if (width <= 0 || height <= 0) return;
            var texture = GameStageController.Instance.PerspectiveCamera.targetTexture;
            if (texture.width == width && texture.height == height) return;
            texture.Release();
            texture.width = width;
            texture.height = height;
            texture.Create();
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
