using System;
using Deenote.Edit;
using Deenote.GameStage;
using Deenote.Settings;
using Deenote.UI.Windows.Components;
using Deenote.Utilities;
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
        private RectTransform _cameraViewTransform = null!;
        private IntegralSizeAspectRatioFitter _imageAspectFitter = null!;

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

            _cameraViewTransform = _cameraViewRawImage.rectTransform;
            RenderTexture texture = new(1280, 720, GraphicsFormat.R8G8B8A8_UNorm, GraphicsFormat.None);
            _cameraViewRawImage.texture = texture;
            _cameraViewRawImage.enabled = true;
            var controller = PerspectiveViewController.Instance;
            controller.ViewCamera.targetTexture = controller.BackgroundCamera.targetTexture = texture;
            _viewSize = new FrameCachedNotifyingProperty<Vector2>(GetViewSize, autoUpdate: true);
            _viewSize.OnValueChanged += ViewSizeChanged;
            OnViewSizeChanged += ResizeCameraTexture;
            _imageAspectFitter = _cameraViewRawImage.GetComponent<IntegralSizeAspectRatioFitter>();
            _fullScreenButton.onClick.AddListener(() => _gameStageController.PerspectiveView.SetFullScreenState(true));
            _aspectDropdown.Dropdown.onValueChanged.AddListener(OnAspectChanged);

            AwakeStageUI();

            _window.SetOnFirstActivating(OnFirstActivating);
            _window.SetOnIsActivatedChanged(activated =>
            {
                if (activated) OnWindowActivated();
            });
        }

        private void ResizeCameraTexture(Vector2 size)
        {
            var viewCamera = PerspectiveViewController.Instance.ViewCamera;
            viewCamera.targetTexture.Resize(size.RoundToInt());
            float rectHeight = 9f / 16f * size.x / size.y;
            viewCamera.rect = new Rect(0, 0, 1, rectHeight);
        }

        private void OnFirstActivating()
        {
            _aspectDropdown.ResetOptions(ViewAspectRatioExt.ViewAspectDropdownOptions);
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
            _window.FixedAspectRatio = _imageAspectFitter.AspectRatio = aspectRatio.GetRatio();
        }

        #endregion
    }
}