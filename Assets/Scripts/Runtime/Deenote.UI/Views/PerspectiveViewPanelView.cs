#nullable enable

using Deenote.Core;
using Deenote.Core.GamePlay;
using Deenote.GamePlay.UI;
using Deenote.Library;
using Deenote.Library.Components;
using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

namespace Deenote.UI.Views
{
    public sealed partial class PerspectiveViewPanelView : MonoBehaviour
    {
        [SerializeField] AspectRatioFitter _aspectRatioFitter = default!;
        [SerializeField] RawImage _viewRawImage = default!;
        [SerializeField] IntegralSizeAspectRatioFitter _viewImageAspectRatioFitter = default!;

        [SerializeField] Transform _windowScreenParentTransform = default!;
        [SerializeField] Transform _fullScreenParentTransform = default!;
        [SerializeField] RectTransform _contentTransform = default!;

        private RenderTexture _viewRenderTexture = default!;

        public PerspectiveViewForegroundBase StageForeground { get; private set; } = default!;

        public RenderTexture ViewRendererTexture => _viewRenderTexture;

        private FrameCachedNotifyingProperty<Vector2> _viewSize_bf = default!;
        public Vector2 ViewSize => _viewSize_bf.Value;

        private bool _isFullScreen_bf;
        public bool IsFullScreen => _isFullScreen_bf;

        public float AspectRatio
        {
            get => _aspectRatioFitter.aspectRatio;
            set {
                if (_aspectRatioFitter.aspectRatio == value)
                    return;
                _aspectRatioFitter.aspectRatio = value;
                _viewImageAspectRatioFitter.AspectRatio = value;
                AspectRatioChanged?.Invoke(value);
            }
        }

        public event Action<float>? AspectRatioChanged;

        public void SetIsFullScreen(bool full)
        {
            if (Utils.SetField(ref _isFullScreen_bf, full)) {

                _contentTransform.parent = full ? _fullScreenParentTransform : _windowScreenParentTransform;
                _fullScreenParentTransform.gameObject.SetActive(full);
                _windowScreenParentTransform.gameObject.SetActive(!full);
                if (full)
                    ApplicationManager.SetAspectRatio(AspectRatio, true);
                else
                    ApplicationManager.RecoverResolution();
            }
        }

        private void Awake()
        {
            InitAspectRatioController();
            RegisterKeyBindings();

            MainSystem.GamePlayManager.RegisterNotification(
                GamePlayManager.NotificationFlag.GameStageLoaded,
                manager =>
                {
                    manager.AssertStageLoaded();
                    manager.Stage.PerspectiveCamera.ApplyToRenderTexture(_viewRenderTexture);
                    StageForeground = manager.InstantiatePerspectiveViewForeground(_contentTransform);
                    _OnStageLoaded_Input(manager);
                });

            MainSystem.GamePlayManager.MusicPlayer.TimeChanged += args =>
            {
                var mousePos = Input.mousePosition;
                if (TryConvertScreenPointToNoteCoord(mousePos, null!, out var coord)) {
                    MainSystem.StageChartEditor.Placer.UpdateMovePlace(coord, mousePos);
                }
                else {
                    MainSystem.StageChartEditor.Placer.HideIndicators();
                }
            };

            void InitAspectRatioController()
            {
                _viewRenderTexture = new RenderTexture(1280, 720, GraphicsFormat.R8G8B8A8_UNorm, GraphicsFormat.None);
                _viewRawImage.texture = _viewRenderTexture;
                _viewRawImage.enabled = true;

                var imgTsfm = _viewRawImage.rectTransform;

                _viewSize_bf = new FrameCachedNotifyingProperty<Vector2>(GetViewSize, autoUpdate: true);
                _viewSize_bf.OnValueChanged += ResizeTargetTexture;

                Vector2 GetViewSize()
                {
                    var rect = _viewRawImage.rectTransform.rect;
                    return new Vector2(rect.width, rect.height);
                }

                void ResizeTargetTexture(Vector2 old, Vector2 size)
                {
                    _viewRenderTexture.Resize(MathUtils.RoundToInt(size));
                    MainSystem.GamePlayManager.Stage?.PerspectiveCamera.ApplyToRenderTexture(_viewRenderTexture);
                }
            }
        }

    }
}