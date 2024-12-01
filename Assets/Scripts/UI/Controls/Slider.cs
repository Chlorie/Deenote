#nullable enable

using Deenote.Utilities;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Deenote.UI.Controls
{
    public sealed class Slider : UIPressableControlBase, IDragHandler
    {
        [SerializeField] RectTransform _raycastAreaRectTransform = default!;
        [SerializeField] Image _backgroundImage = default!;
        [SerializeField] Image _fillImage = default!;
        [SerializeField] SliderHandle _handle = default!;

        [SerializeField, Range(0f, 1f)]
        private float _value_bf;

        public float Value
        {
            get => _value_bf;
            set {
                value = Mathf.Clamp01(value);
                if (Utils.SetField(ref _value_bf, value)) {
                    TranslateValueVisual();
                }
            }
        }

        public UnityEvent<float> OnValueChanged { get; } = new();

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (!IsLeftButtonOnInteractableControl(eventData))
                return;

            _handle._isDraggingInSliderTrackBar = true;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_raycastAreaRectTransform, eventData.position, eventData.pressEventCamera, out var localPoint)) {
                var vpx = localPoint.x / _raycastAreaRectTransform.rect.width;
                Value = vpx; // Auto clamp in Value::set
            }
        }

        protected override void OnPointerDownImpl(PointerEventData eventData)
        {
            base.OnPointerDownImpl(eventData);
            ((IDragHandler)this).OnDrag(eventData);
        }

        protected override void OnPointerUpImpl(PointerEventData eventData)
        {
            base.OnPointerUpImpl(eventData);
            _handle._isDraggingInSliderTrackBar = false;
        }

        protected override void TranslateVisual()
        {
            if (!isActiveAndEnabled)
                return;

            var colors = MainSystem.Args.UIColors;

            var state = GetPressVisualState();
            var fill = state switch {
                PressVisualState.Disabled => colors.ControlAccentDisabledColor,
                PressVisualState.Pressed => colors.ControlAccentTertiaryColor,
                PressVisualState.Hovering => colors.ControlAccentSecondaryColor,
                PressVisualState.Default or _ => colors.ControlAccentDefaultColor,
            };
            _fillImage.color = fill;
            _backgroundImage.color = state is PressVisualState.Disabled
                ? colors.ControlStrongDisabledColor
                : colors.ControlStrongDefaultColor;
        }

        private void TranslateValueVisual()
        {
            var fillTsfm = _fillImage.rectTransform;
            fillTsfm.anchorMax = fillTsfm.anchorMax with { x = Value };
            var hdlTsfm = _handle.RectTransform;
            hdlTsfm.anchorMax = hdlTsfm.anchorMax with { x = Value };
            hdlTsfm.anchorMin = hdlTsfm.anchorMin with { x = Value };
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            TranslateValueVisual();
        }
    }
}
