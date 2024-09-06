using Deenote.Inputting;
using System;
using UnityEngine;

namespace Deenote.Utilities
{
    public static class KeyboardUtils
    {
        public static KeyModifiers GetPressedModifierKeys()
        {
            KeyModifiers res = KeyModifiers.None;
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) res |= KeyModifiers.Ctrl;
            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) res |= KeyModifiers.Alt;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) res |= KeyModifiers.Shift;
            return res;
        }

        /// <summary>
        /// Check whether the <see cref="KeyCode"/> is of the given <see cref="KeyState"/>.
        /// </summary>
        /// <param name="key">The keycode.</param>
        /// <param name="state">The key state.</param>
        public static bool IsOfState(this KeyCode key, KeyState state)
        {
            return state switch {
                KeyState.Pressed => Input.GetKeyDown(key),
                KeyState.Released => Input.GetKeyUp(key),
                KeyState.Holding => Input.GetKey(key),
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
        }

        /// <inheritdoc cref="IsActive(KeyBinding, KeyModifiers)"/>
        public static bool IsActive(this KeyBinding binding) =>
            binding.IsActive(GetPressedModifierKeys());

        /// <summary>
        /// Check whether the <see cref="KeyBinding"/> is active.
        /// </summary>
        /// <remarks>
        /// Note that this method only checks whether the keyboard state matches the key binding.
        /// Normally when an input field is focused, all key bindings should be deactivated,
        /// which this method doesn't take into consideration.
        /// Use <see cref="KeyBindingManager.CheckKeyBinding"/> instead for the input field check.
        /// </remarks>
        /// <param name="binding">The key binding.</param>
        /// <param name="checkedModifiers">
        /// The current modifier state as given by <see cref="GetPressedModifierKeys"/>.
        /// </param>
        /// <returns>Whether the key binding is active.</returns>
        public static bool IsActive(this KeyBinding binding, KeyModifiers checkedModifiers) =>
            binding.Modifiers == checkedModifiers && binding.Key.IsOfState(binding.State);
    }
}