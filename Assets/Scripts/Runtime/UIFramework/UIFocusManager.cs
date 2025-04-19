#nullable enable

using Deenote.Library.Components;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Deenote.UIFramework
{
    internal sealed class UIFocusManager : PersistentSingletonBehavior<UIFocusManager>
    {
        private IFocusable? _focusing;
        private bool _wasFocusedThisFrame;

        public event Action<IFocusable?>? FocusingChanged;


        private void LateUpdate()
        {
            if (_wasFocusedThisFrame) {
                _wasFocusedThisFrame = false;
                return;
            }
            if (_focusing is null)
                return;

            var mouse = Mouse.current;
            if (mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame) {
                Focus(null);
            }
        }

        internal void Focus(IFocusable? focusable)
        {
            _wasFocusedThisFrame = true;
            if (_focusing == focusable)
                return;

            if (_focusing is not null)
                _focusing.IsFocused = false;
            _focusing = focusable;
            if (_focusing is not null)
                _focusing.IsFocused = true;

            FocusingChanged?.Invoke(_focusing);
        }
    }
}