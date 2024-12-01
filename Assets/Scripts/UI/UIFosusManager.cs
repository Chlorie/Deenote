#nullable enable

using Deenote.Utilities;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deenote.UI
{
    public sealed partial class UIFocusManager : PersistentSingletonBehavior<UIFocusManager>
    {
        public event Action<MouseButton, Vector2>? OnFocusChanged;

        private void LateUpdate()
        {
            if (OnFocusChanged is not null) {
                Vector2? mousePosition = null;
                for (int i = 0; i < 3; i++) {
                    if (Input.GetMouseButtonDown(i)) {
                        OnFocusChanged.Invoke((MouseButton)i, mousePosition ??= Input.mousePosition);
                    }
                }
            }
        }
    }
}