using Deenote.Edit;
using Deenote.UI.Windows;
using Deenote.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.GameStage
{
    public sealed partial class PerspectiveViewController : SingletonBehavior<PerspectiveViewController>
    {
        [Header("Notify")]
        [SerializeField] EditorController _editor;
        [SerializeField] GameStageController _stage;
        [SerializeField] PerspectiveViewWindow _perspectiveViewWindow;

        [Header("")]
        [SerializeField] RawImage _cameraViewRawImage;
        [Header("Full Screen")]
        [SerializeField] Transform _windowScreenParentTransform;
        [SerializeField] Transform _fullScreenParentTransform;

        [field: Header("Aspect Ratio Adjust")]
        [field: SerializeField] public Camera ViewCamera { get; private set; } = null!;

        [field: SerializeField] public Camera BackgroundCamera { get; private set; } = null!;

        public bool IsFullScreen { get; private set; }

        private ViewAspectRatio __aspectRatio;

        public ViewAspectRatio AspectRatio
        {
            get => __aspectRatio;
            set {
                if (__aspectRatio == value)
                    return;
                __aspectRatio = value;
                _perspectiveViewWindow.NotifyAspectRatioChanged(__aspectRatio);
            }
        }

        public void SetFullScreenState(bool full)
        {
            if (IsFullScreen == full)
                return;

            IsFullScreen = full;
            if (IsFullScreen) {
                _fullScreenParentTransform.gameObject.SetActive(true);
                transform.SetParent(_fullScreenParentTransform, false);
                MainSystem.ResolutionAdjuster.SetAspectRatio(AspectRatio.GetRatio(), true);
            }
            else {
                _fullScreenParentTransform.gameObject.SetActive(false);
                transform.SetParent(_windowScreenParentTransform, false);
                MainSystem.ResolutionAdjuster.RecoverResolution();
            }
        }

        protected override void Awake()
        {
            base.Awake();
            RegisterKeyBindings();
        }

        private void Update()
        {
            if (_stage.IsActive) {
                UpdateNoteIndicatorPosition(Input.mousePosition);
            }
        }

        public float SuddenPlusRangeToVisibleRangePercent(int suddenPlusRange)
        {
            var cameraPos = ViewCamera.transform.position;
            float h = cameraPos.y;
            float z = cameraPos.z;
            float panelLength = _stage.Args.NotePanelLength * _stage.Args.NoteTimeToZMultiplier;

            float ratio = suddenPlusRange / 100f;
            float theta = ratio * (Mathf.Atan((panelLength + z) / h) - Mathf.Atan(z / h));
            float tanTheta = Mathf.Tan(theta);
            float visibleLength = h * (panelLength + z - h * tanTheta) / (h + (panelLength + z) * tanTheta) - z;
            return visibleLength / panelLength;
        }
    }

    public enum ViewAspectRatio
    {
        SixteenNine,
        FourThree
    }

    public static class ViewAspectRatioExt
    {
        public static readonly string[] ViewAspectDropdownOptions = { "16:9", "4:3" };

        public static int ToDropdownIndex(this ViewAspectRatio aspect) => (int)aspect;

        public static ViewAspectRatio FromDropdownIndex(int index) => (ViewAspectRatio)index;

        public static float GetRatio(this ViewAspectRatio viewAspectRatio) => viewAspectRatio switch {
            ViewAspectRatio.SixteenNine => 16f / 9f,
            ViewAspectRatio.FourThree => 4f / 3f,
            _ => 16f / 9f,
        };
    }
}