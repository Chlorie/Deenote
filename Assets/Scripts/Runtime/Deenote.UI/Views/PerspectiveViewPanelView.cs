#nullable enable

using Deenote.Core;
using Deenote.Core.GamePlay;
using Deenote.GamePlay.UI;
using Deenote.Library;
using Deenote.Library.Components;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

namespace Deenote.UI.Views
{
    public sealed partial class PerspectiveViewPanelView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] AspectRatioFitter _aspectRatioFitter = default!;
        [SerializeField] RawImage _viewRawImage = default!;
        [SerializeField] IntegralSizeAspectRatioFitter _viewImageAspectRatioFitter = default!;

        [SerializeField] Transform _windowScreenParentTransform = default!;
        [SerializeField] Transform _fullScreenParentTransform = default!;
        [SerializeField] RectTransform _contentTransform = default!;

        [SerializeField] GraphicRaycaster _raycaster = default!;

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

            MainSystem.SaveSystem.SavingConfigurations += configs =>
            {
                configs.Add("ui/perspective_aspect_ratio", AspectRatio);
            };
            MainSystem.SaveSystem.LoadedConfigurations += configs =>
            {
                AspectRatio = configs.GetSingle("ui/perspective_aspect_ratio", 4f / 3f);
            };

            MainSystem.GamePlayManager.StageLoaded += _OnStageLoaded;

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

        private void _OnStageLoaded(GamePlayManager.StageLoadedEventArgs args)
        {
            args.Stage.PerspectiveCamera.ApplyToRenderTexture(_viewRenderTexture);

            var foreground = Instantiate(args.PerspectiveViewForegroundPrefab, _contentTransform);
            if (StageForeground != null) {
                Destroy(StageForeground.gameObject);
            }
            StageForeground = foreground;

            _raycaster.enabled = true;
        }

        #region Pointer

        /// <summary>
        /// InputManager requires this to judge if mouse actions should be enabled.
        /// </summary>
        public bool IsHovering { get; private set; }
        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
            => IsHovering = true;
        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
            => IsHovering = false;
       
        #endregion
    }
}