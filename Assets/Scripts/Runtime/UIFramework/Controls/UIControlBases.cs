#nullable enable

using Deenote.Library;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Deenote.UIFramework.Controls
{
    public abstract class UIVisualTransitionControl : UIThemedControl, IInteractableControl,
        IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] bool _isInteractable_bf = true;
        [SerializeField] protected bool _isHovering;

        public virtual bool IsInteractable
        {
            get => _isInteractable_bf;
            set {
                if (Utils.SetField(ref _isInteractable_bf, value)) {
                    DoVisualTransition(UISystem.ColorArgs);
                }
            }
        }

        protected bool IsLeftButtonOnInteractableControl(PointerEventData eventData)
        {
            return eventData.button is PointerEventData.InputButton.Left
                && isActiveAndEnabled
                && IsInteractable;
        }

        protected abstract void DoVisualTransition(UIThemeColorArgs args);

        protected override void OnThemeChanged(UIThemeColorArgs args) { }

        /// <summary>
        /// Invoke when awake or Theme changed
        /// </summary>

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            _isHovering = true;
            DoVisualTransition(UISystem.ColorArgs);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            _isHovering = false;
            DoVisualTransition(UISystem.ColorArgs);
        }

        protected override void Awake()
        {
            base.Awake();
            DoVisualTransition(UISystem.ColorArgs);
        }

        protected virtual void OnEnable()
        {
            DoVisualTransition(UISystem.ColorArgs);
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            DoVisualTransition(UISystem.ColorArgs);
        }
    }

    public abstract class UIPressableControlBase : UIVisualTransitionControl,
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

        protected override sealed void DoVisualTransition(UIThemeColorArgs args)
        {
            if (!isActiveAndEnabled)
                return;
            DoVisualTransition(args, GetPressVisualState());
        }

        protected abstract void DoVisualTransition(UIThemeColorArgs args, PressVisualState state);

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
            => OnPointerDown(eventData);

        protected virtual void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button is not PointerEventData.InputButton.Left)
                return;
            _isPressed = true;
            DoVisualTransition(UISystem.ColorArgs);
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
            => OnPointerUp(eventData);

        protected virtual void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button is not PointerEventData.InputButton.Left)
                return;

            _isPressed = false;
            DoVisualTransition(UISystem.ColorArgs);
        }

        protected enum PressVisualState
        {
            Disabled,
            Pressed,
            Hovering,
            Default,
        }
    }

    public abstract class UIFocusableControlBase : UIVisualTransitionControl, IPointerDownHandler
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

        protected override sealed void DoVisualTransition(UIThemeColorArgs args)
        {
            if (!isActiveAndEnabled)
                return;
            DoVisualTransition(args, GetFocusVisualState());
        }

        protected abstract void DoVisualTransition(UIThemeColorArgs args, FocusVisualState state);

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            _isFocused = true;
            _pointerDownFrame = Time.frameCount;
            DoVisualTransition(UISystem.ColorArgs);
        }

        protected override void Awake()
        {
            base.Awake();
            UISystem.MouseFocusChanged += (_, _) =>
            {
                if (_pointerDownFrame == Time.frameCount)
                    return;
                Unfocus();
            };
        }

        protected void Unfocus()
        {
            _isFocused = false;
            DoVisualTransition(UISystem.ColorArgs);
        }

        protected enum FocusVisualState
        {
            Disabled,
            Focused,
            Hovering,
            Default,
        }
    }
}
