#nullable enable

using Deenote.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Deenote.UI.Controls
{
    public abstract class UIControlBase : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] bool _isInteractable_bf = true;
        [SerializeField] protected bool _isHovering;

        public virtual bool IsInteractable
        {
            get => _isInteractable_bf;
            set {
                if (Utils.SetField(ref _isInteractable_bf, value)) {
                    TranslateVisual();
                }
            }
        }

        protected bool IsLeftButtonOnInteractableControl(PointerEventData eventData)
        {
            return eventData.button is PointerEventData.InputButton.Left
                && isActiveAndEnabled
                && IsInteractable;
        }

        protected abstract void TranslateVisual();

        /// <summary>
        /// Invoke when awake or Theme changed
        /// </summary>
        protected virtual void TranslateStaticVisual() { }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            _isHovering = true;
            TranslateVisual();
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            _isHovering = false;
            TranslateVisual();
        }

        protected virtual void Awake()
        {
            TranslateStaticVisual();
            TranslateVisual();
        }

        protected virtual void OnEnable()
        {
            TranslateVisual();
        }

        protected virtual void OnValidate()
        {
            TranslateVisual();
        }
    }

    public abstract class UIPressableControlBase : UIControlBase,
        IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] protected bool _isPressed;

        protected PressVisualState GetPressVisualState()
        {
            if (!IsInteractable)
                return PressVisualState.Disabled;
            else if (_isPressed)
                return PressVisualState.Pressed;
            else if (_isHovering)
                return PressVisualState.Hovering;
            else
                return PressVisualState.Default;
        }

        protected enum PressVisualState
        {
            Disabled,
            Pressed,
            Hovering,
            Default,
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
            => OnPointerDownImpl(eventData);

        protected virtual void OnPointerDownImpl(PointerEventData eventData)
        {
            if (eventData.button is not PointerEventData.InputButton.Left)
                return;
            _isPressed = true;
            TranslateVisual();
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
            => OnPointerUpImpl(eventData);

        protected virtual void OnPointerUpImpl(PointerEventData eventData)
        {
            if (eventData.button is not PointerEventData.InputButton.Left)
                return;

            _isPressed = false;
            TranslateVisual();
        }
    }

    public abstract class UIFocusableControlBase : UIControlBase, IPointerDownHandler
    {
        [SerializeField] protected bool _isFocused;

        private int _pointerDownFrame;

        protected FocusVisualState GetFocusVisualState()
        {
            if (!IsInteractable)
                return FocusVisualState.Disabled;
            else if (_isFocused)
                return FocusVisualState.Focused;
            else if (_isHovering)
                return FocusVisualState.Hovering;
            else
                return FocusVisualState.Default;
        }

        protected enum FocusVisualState
        {
            Disabled,
            Focused,
            Hovering,
            Default,
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            _isFocused = true;
            _pointerDownFrame = Time.frameCount;
            TranslateVisual();
        }

        protected override void Awake()
        {
            base.Awake();
            UIFocusManager.Instance.OnFocusChanged += (btn, pos) =>
            {
                if (_pointerDownFrame == Time.frameCount)
                    return;
                Unfocus();
            };
        }

        protected void Unfocus()
        {
            _isFocused = false;
            TranslateVisual();
        }
    }
}
