using System;
using System.Text;
using UnityEngine;

namespace Deenote
{
    [Serializable]
    public struct Shortcut
    {
        public KeyCode Key;
        public FuncKeyCode FuncKey;
        public ActionKind Action;

        public readonly string ToDisplayString()
        {
            if (Key == KeyCode.None)
                return "";
            var sb = new StringBuilder();
            if (FuncKey.HasFlag(FuncKeyCode.Ctrl))
                sb.Append("Ctrl + ");
            if (FuncKey.HasFlag(FuncKeyCode.Shift))
                sb.Append("Shift + ");
            if (FuncKey.HasFlag(FuncKeyCode.Alt))
                sb.Append("Shift + ");
            
            return sb.Append(Key.ToString())
                .ToString();
        }

        [Flags]
        public enum FuncKeyCode
        {
            None = 0,
            Ctrl = 1 << 0,
            Shift = 1 << 1,
            Alt = 1 << 2,
        }

        public enum ActionKind
        {
            Press,
            Hold,
            Release,
        }
    }
}