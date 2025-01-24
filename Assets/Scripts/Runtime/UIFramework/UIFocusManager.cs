#nullable enable

using Deenote.Library.Components;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deenote.UIFramework
{
    internal sealed class UIFocusManager : PersistentSingletonBehavior<UIFocusManager>
    {
        public event Action<MouseButton, Vector2>? FocusChanged;

        private void LateUpdate()
        {
            if (FocusChanged is not null) {
                Vector2? position = null;
                for (int i = 0; i < 3; i++) {
                    if (Input.GetMouseButtonDown(i)) {
                        FocusChanged.Invoke((MouseButton)i, position ??= Input.mousePosition);
                    }
                }
            }
        }
    }
}