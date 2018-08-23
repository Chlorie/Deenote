using System;
using System.Text;
using UnityEngine;

[Serializable]
public class Shortcut
{
    public enum State
    {
        Press,
        Hold,
        Release
    }
    public State state = State.Press;
    public bool ctrl = false;
    public bool alt = false;
    public bool shift = false;
    public KeyCode key;
    public override string ToString()
    {
        StringBuilder result = new StringBuilder();
        if (ctrl) result.Append("Ctrl+");
        if (alt) result.Append("Alt+");
        if (shift) result.Append("Shift+");
        result.Append(key);
        return result.ToString();
    }
    public bool IsActive
    {
        get
        {
            if (ctrl && !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl)) return false;
            if (!ctrl && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) return false;
            if (alt && !Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.RightAlt)) return false;
            if (!alt && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))) return false;
            if (shift && !Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift)) return false;
            if (!shift && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))) return false;
            switch (state)
            {
                case State.Press:
                    return Input.GetKeyDown(key);
                case State.Hold:
                    return Input.GetKey(key);
                case State.Release:
                    return Input.GetKeyUp(key);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
