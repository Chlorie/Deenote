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
        [SerializeField] Image _backgroundImage = default!;
        [SerializeField] Image _borderImage = default!;
        [SerializeField] Image _circleImage = default!;
        [SerializeField] RectTransform _rectTransform = default!;

        public RectTransform RectTransform => _rectTransform;

        private float _dragBeginXOffsetToHandle;
        private bool _isDragging;

        // Drag event of UGUI EventSystem has a drag start threshold,
        // we dont want to change the global setting, so we use
        // PointerDown and PointerMove to manually implement drag event

        protected override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);

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

        protected override void DoVisualTransition(UIThemeColorArgs args, PressVisualState state)
        {
            const float HoverScale = 20f / 14f;
            const float PressScale = 12f / 14f;

            var (circle, bdr) = state switch {
                PressVisualState.Disabled => (args.ControlAccentDisabledColor, args.ControlStrokeDefaultColor),
                PressVisualState.Pressed => (args.ControlAccentTertiaryColor, args.ControlStrokeDefaultColor),
                PressVisualState.Hovering => (args.ControlAccentSecondaryColor, args.ControlElevationBorderColor),
                PressVisualState.Default or _ => (args.ControlAccentDefaultColor, args.ControlElevationBorderColor),
            };
            var scale = (state, _slider._isDraggingSliderTrackerBar) switch {
                (PressVisualState.Pressed, false) => PressScale,
                (PressVisualState.Hovering, false) => HoverScale,
                _ => 1f,
            };
            _circleImage.color = circle;
            _borderImage.color = bdr;
            _circleImage.rectTransform.localScale = new Vector3(scale, scale, scale);
        }

        protected override void OnThemeChanged(UIThemeColorArgs args)
        {
            _backgroundImage.color = args.ControlSolidDefaultColor;
        }
    }
}
