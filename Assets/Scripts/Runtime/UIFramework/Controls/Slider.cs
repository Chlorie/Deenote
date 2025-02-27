#nullable enable

using Deenote.Library;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Deenote.UIFramework.Controls
{
    public sealed class Slider : UIPressableControlBase,
        IPointerMoveHandler, 
        IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] internal RectTransform _raycastAreaRectTransform = default!;
        [SerializeField] UnityEngine.UI.Image _backgroundImage = default!;
        [SerializeField] UnityEngine.UI.Image _fillImage = default!;
        [SerializeField] SliderHandle _handle = default!;

        [SerializeField, Range(0f, 1f)]
        private float _value_bf;

        /// <summary>
        /// Between 0 and 1
        /// </summary>
        public float Value
        {
            get => _value_bf;
            set => SetValue(value, true);
        }

        internal bool _isDraggingSliderTrackerBar;
        private bool _isDragging;

        public event Action<float>? ValueChanged;

        public void SetValueWithoutNotify(float value) => SetValue(value, false);

        private void SetValue(float value, bool notify)
        {
            value = Mathf.Clamp01(value);
            if (Utils.SetField(ref _value_bf, value)) {
                TranslateValueVisual();
                if (notify)
                    ValueChanged?.Invoke(value);
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

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_raycastAreaRectTransform, eventData.position, eventData.pressEventCamera, out var localPoint)) {
                var vpx = localPoint.x / _raycastAreaRectTransform.rect.width;
                Value = vpx; // Auto clamp in Value::set
            }
        }

        protected override void OnPointerDownImpl(PointerEventData eventData)
        {
            base.OnPointerDownImpl(eventData);
            _isDraggingSliderTrackerBar = true;
            ((IPointerMoveHandler)this).OnPointerMove(eventData);
        }

        protected override void OnPointerUpImpl(PointerEventData eventData)
        {
            base.OnPointerUpImpl(eventData);
            _isDraggingSliderTrackerBar = false;
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (!_isDragging)
                return;
            if (!IsLeftButtonOnInteractableControl(eventData))
                return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_raycastAreaRectTransform, eventData.position, eventData.pressEventCamera, out var localPoint)) {
                var vpx = localPoint.x / _raycastAreaRectTransform.rect.width;
                Value = vpx; // Auto clamp in Value::set
            }
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) => _isDragging = true;
        void IEndDragHandler.OnEndDrag(PointerEventData eventData) => _isDragging = false;

        protected override void DoVisualTransition()
        {
            if (!isActiveAndEnabled)
                return;

            var colors = UISystem.ThemeArgs;

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
