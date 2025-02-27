#nullable enable

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Deenote.UIFramework.Controls
{
    [RequireComponent(typeof(RectTransform))]
    internal sealed class SliderHandle : UIPressableControlBase,
        IPointerMoveHandler, 
        IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] Slider _slider = default!;
        [SerializeField] UnityEngine.UI.Image _backgroundImage = default!;
        [SerializeField] UnityEngine.UI.Image _circleImage = default!;
        [SerializeField] RectTransform _rectTransform = default!;
        // 似乎有1像素的border，颜色ControlElevationBorderBrush，但是完全看不出来，算了

        public RectTransform RectTransform => _rectTransform;

        private float _dragBeginXOffsetToHandle;
        private bool _isDragging;

        // Drag event of UGUI EventSystem has a drag start threshold,
        // we dont want to change the global setting, so we use
        // PointerDown and PointerMove to manually implement drag event

        protected override void OnPointerDownImpl(PointerEventData eventData)
        {
            base.OnPointerDownImpl(eventData);

            if (!IsLeftButtonOnInteractableControl(eventData))
                return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, eventData.position, eventData.pressEventCamera, out var point)) {
                _dragBeginXOffsetToHandle = point.x;
            }
        }

        void IPointerMoveHandler.OnPointerMove(PointerEventData eventData)
        {
            if (!base._isPressed)
                return;
            if (_isDragging)
                return;
            if (!IsLeftButtonOnInteractableControl(eventData))
                return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_slider._raycastAreaRectTransform, eventData.position, eventData.pressEventCamera, out var point)) {
                var vpx = (point.x - _dragBeginXOffsetToHandle) / _slider._raycastAreaRectTransform.rect.width;
                _slider.Value = vpx;
            }
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (!_isDragging)
                return;
            if (!IsLeftButtonOnInteractableControl(eventData))
                return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_slider._raycastAreaRectTransform, eventData.position, eventData.pressEventCamera, out var point)) {
                var vpx = (point.x - _dragBeginXOffsetToHandle) / _slider._raycastAreaRectTransform.rect.width;
                _slider.Value = vpx;
            }
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) => _isDragging = true;
        void IEndDragHandler.OnEndDrag(PointerEventData eventData) => _isDragging = false;

        protected override void DoVisualTransition()
        {
            if (!isActiveAndEnabled)
                return;

            const float HoverScale = 20f / 14f;
            const float PressScale = 12f / 14f;

            var colors = UISystem.ThemeArgs;

            var state = GetPressVisualState();

            var (circle, scale) = state switch {
                PressVisualState.Disabled => (colors.ControlAccentDisabledColor, 1f),
                PressVisualState.Pressed => (colors.ControlAccentTertiaryColor, _slider._isDraggingSliderTrackerBar ? 1f : PressScale),
                PressVisualState.Hovering => (colors.ControlAccentSecondaryColor, _slider._isDraggingSliderTrackerBar ? 1f : HoverScale),
                PressVisualState.Default or _ => (colors.ControlAccentDefaultColor, 1f),
            };
            _circleImage.color = circle;
            _circleImage.rectTransform.localScale = new Vector3(scale, scale, scale);
        }

        protected override void DoStaticVisualTransition()
        {
            var colors = UISystem.ThemeArgs;
            _backgroundImage.color = colors.ControlSolidDefaultColor;
        }
    }
}
