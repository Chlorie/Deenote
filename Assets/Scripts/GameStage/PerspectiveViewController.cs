using Deenote.ApplicationManaging;
using Deenote.Edit;
using Deenote.UI.Windows;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.GameStage
{
    public sealed partial class PerspectiveViewController : MonoBehaviour
    {
        [Header("Notify")]
        [SerializeField] EditorController _editor;
        [SerializeField] GameStageController _stage;
        [SerializeField] PerspectiveViewWindow _perspectiveViewWindow;
        [SerializeField] ResolutionAdjuster _resolutionAdjuster;

        [Header("")]
        [SerializeField] RawImage _cameraViewRawImage;
        [Header("Full Screen")]
        [SerializeField] Transform _windowScreenParentTransform;
        [SerializeField] Transform _fullScreenParentTransform;
        [Header("Aspect Ratio Adjust")]
        [SerializeField] Camera _viewCamera;
        [SerializeField] Camera _backgroundCamera;
        [SerializeField] RenderTexture _viewRenderTexture4_3;
        [SerializeField] RenderTexture _viewRenderTexture16_9;

        private bool _isFullScreen;
        private ViewAspectRatio __aspectRatio;

        public bool IsFullScreen => _isFullScreen;

        public ViewAspectRatio AspectRatio
        {
            get => __aspectRatio;
            set {
                if (__aspectRatio == value)
                    return;

                __aspectRatio = value;
                switch (__aspectRatio) {
                    case ViewAspectRatio.FourThree:
                        __aspectRatio = ViewAspectRatio.FourThree;
                        _viewCamera.targetTexture = _viewRenderTexture4_3;
                        _backgroundCamera.targetTexture = _viewRenderTexture4_3;
                        _cameraViewRawImage.texture = _viewRenderTexture4_3;
                        _viewCamera.rect = new Rect(0, 0, 1, 3f / 4f); // Keep view camera's aspect ratio
                        break;
                    case ViewAspectRatio.SixteenNine or _:
                        __aspectRatio = ViewAspectRatio.SixteenNine;
                        _viewCamera.targetTexture = _viewRenderTexture16_9;
                        _backgroundCamera.targetTexture = _viewRenderTexture16_9;
                        _cameraViewRawImage.texture = _viewRenderTexture16_9;
                        _viewCamera.rect = new Rect(0, 0, 1, 1);
                        break;
                }

                _perspectiveViewWindow.NotifyAspectRatioChanged(__aspectRatio);
            }
        }

        public void SetFullScreenState(bool full)
        {
            if (_isFullScreen == full)
                return;

            _isFullScreen = full;
            if (_isFullScreen) {
                _fullScreenParentTransform.gameObject.SetActive(true);
                transform.SetParent(_fullScreenParentTransform, false);
                MainSystem.ResolutionAdjuster.SetAspectRatio(EnumExt.GetRatio(AspectRatio), true);
            }
            else {
                _fullScreenParentTransform.gameObject.SetActive(false);
                transform.SetParent(_windowScreenParentTransform, false);
                MainSystem.ResolutionAdjuster.RecoverResolution();
            }
        }

        private void Update()
        {
            if (_stage.IsActive) {
                UpdateNoteIndicatorPosition((Vector2)Input.mousePosition);
            }
        }

        public float SuddenPlusRangeToVisibleRangePercent(int suddenPlusRange)
        {
            var cameraPos = _viewCamera.transform.position;
            float h = cameraPos.y;
            float z = cameraPos.z;
            float panelLength = _stage.Args.NotePanelLength * _stage.Args.NoteTimeToZMultiplier;

            float ratio = suddenPlusRange / 100f;
            float theta = ratio * (Mathf.Atan((panelLength + z) / h) - Mathf.Atan(z / h));
            float tanTheta = Mathf.Tan(theta);
            float visibleLength = h * (panelLength + z - h * tanTheta) / (h + (panelLength + z) * tanTheta) - z;
            return visibleLength / panelLength;
        }

        public enum ViewAspectRatio
        {
            SixteenNine,
            FourThree,
        }

        public static class EnumExt
        {
            public static readonly string[] ViewAspectDropdownOptions = new[] {
                "16:9",
                "4:3",
            };

            public static int ToDropdownIndex(ViewAspectRatio aspect) => (int)aspect;

            public static ViewAspectRatio FromDropdownIndex(int index) => (ViewAspectRatio)index;

            public static float GetRatio(ViewAspectRatio viewAspectRatio) => viewAspectRatio switch {
                ViewAspectRatio.SixteenNine => 16f / 9f,
                ViewAspectRatio.FourThree => 4f / 3f,
                _ => 16f / 9f,
            };
        }
    }
}