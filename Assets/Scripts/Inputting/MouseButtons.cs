#nullable enable

using System;

namespace Deenote.Inputting
{
    [Flags]
    public enum MouseButtons
    {
        None = 0,
        Left = 1,
        Right = 2,
        Middle = 4,
    }

    public static class MouseButtonExt
    {
        public static void Add(this ref MouseButtons buttons, MouseButtons button)
            => buttons |= button;

        public static void Remove(this ref MouseButtons buttons, MouseButtons button)
            => buttons &= ~button;
    }
}