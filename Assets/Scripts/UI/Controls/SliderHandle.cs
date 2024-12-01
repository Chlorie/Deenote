#nullable enable

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Deenote.UI.Controls
{
    [RequireComponent(typeof(RectTransform))]
    internal sealed class SliderHandle : UIPressableControlBase, IDragHandler
    {
        [SerializeField] Slider _slider = default!;
        [SerializeField] RectTransform _raycastAreaRectTransform = default!;
        [SerializeField] Image _backgroundImage = default!;
        [SerializeField] Image _circleImage = default!;
        [SerializeField] RectTransform _rectTransform = default!;
        // 似乎有1像素的border，颜色ControlElevationBorderBrush，但是完全看不出来，算了

        public RectTransform RectTransform => _rectTransform;

        internal bool _isDraggingInSliderTrackBar;
        private float _pointDownXOffset;

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (!IsLeftButtonOnInteractableControl(eventData))
                return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_raycastAreaRectTransform, eventData.position, eventData.pressEventCamera, out var localPointToRaycast)) {
                var vpx = (localPointToRaycast.x - _pointDownXOffset) / _raycastAreaRectTransform.rect.width;
                _slider.Value = vpx;
            }
        }

        protected override void OnPointerDownImpl(PointerEventData eventData)
        {
            base.OnPointerDownImpl(eventData);

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, eventData.position, eventData.enterEventCamera, out var localPoint)) {
                _pointDownXOffset = localPoint.x;
            }
        }

        protected override void TranslateVisual()
        {
            if (!isActiveAndEnabled)
                return;

            const float HoverScale = 20f / 14f;
            const float PressScale = 12f / 14f;

            var colors = MainSystem.Args.UIColors;

            var state = GetPressVisualState();

            var (circle, scale) = state switch {
                PressVisualState.Disabled => (colors.ControlAccentDisabledColor, 1f),
                PressVisualState.Pressed => (colors.ControlAccentTertiaryColor, _isDraggingInSliderTrackBar ? 1f : PressScale),
                PressVisualState.Hovering => (colors.ControlAccentSecondaryColor, _isDraggingInSliderTrackBar ? 1f : HoverScale),
                PressVisualState.Default or _ => (colors.ControlAccentDefaultColor, 1f),
            };
            _circleImage.color = circle;
            _circleImage.rectTransform.localScale = new Vector3(scale, scale, scale);
        }

        protected override void TranslateStaticVisual()
        {
            var colors = MainSystem.Args.UIColors;
            _backgroundImage.color = colors.ControlSolidDefaultColor;
        }
    }
}
