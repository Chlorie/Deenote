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
                    DoVisualTransition();
                }
            }
        }

        protected bool IsLeftButtonOnInteractableControl(PointerEventData eventData)
        {
            return eventData.button is PointerEventData.InputButton.Left
                && isActiveAndEnabled
                && IsInteractable;
        }

        protected void DoVisualTransition() => DoVisualTransition(UISystem.CurrentTheme);

        protected abstract void DoVisualTransition(UIThemeArgs args);

        protected override void OnThemeChanged(UIThemeArgs args)
        {
            DoVisualTransition(args);
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            _isHovering = true;
            DoVisualTransition();
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            _isHovering = false;
            DoVisualTransition();
        }

        protected override void Awake()
        {
            base.Awake();
        }

        protected virtual void OnEnable()
        {
            DoVisualTransition();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            DoVisualTransition();
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

        protected override sealed void DoVisualTransition(UIThemeArgs args)
        {
            if (!isActiveAndEnabled)
                return;
            DoVisualTransition(args, GetPressVisualState());
        }

        protected abstract void DoVisualTransition(UIThemeArgs args, PressVisualState state);

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
            => OnPointerDown(eventData);

        protected virtual void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button is not PointerEventData.InputButton.Left)
                return;
            _isPressed = true;
            DoVisualTransition();
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
            => OnPointerUp(eventData);

        protected virtual void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button is not PointerEventData.InputButton.Left)
                return;

            _isPressed = false;
            DoVisualTransition();
        }

        protected enum PressVisualState
        {
            Disabled,
            Pressed,
            Hovering,
            Default,
        }
    }

    public abstract class UIFocusableControlBase : UIVisualTransitionControl, IFocusable, IPointerDownHandler
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

        protected override sealed void DoVisualTransition(UIThemeArgs args)
        {
            if (!isActiveAndEnabled)
                return;
            DoVisualTransition(args, GetFocusVisualState());
        }

        protected abstract void DoVisualTransition(UIThemeArgs args, FocusVisualState state);

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            UISystem.FocusManager.Focus(this);
        }

        protected void Unfocus()
        {
            UISystem.FocusManager.Focus(null);
        }

        bool IFocusable.IsFocused
        {
            get => _isFocused = true;
            set {
                _isFocused = value;
                DoVisualTransition();
            }
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
