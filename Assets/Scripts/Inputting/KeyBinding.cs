using System;
using UnityEngine;

namespace Deenote.Inputting
{
    [Serializable]
    public record struct KeyBinding(
        KeyCode Key,
        KeyModifiers Modifiers = KeyModifiers.None,
        KeyState State = KeyState.Pressed
    );

    [Flags]
    public enum KeyModifiers
    {
        None = 0,
        Ctrl = 1,
        Alt = 2,
        CtrlAlt = Ctrl | Alt,
        Shift = 4,
        CtrlShift = Ctrl | Shift,
        AltShift = Alt | Shift,
        CtrlAltShift = Ctrl | Alt | Shift
    }

    public enum KeyState { Pressed, Released, Holding }
}