using System;
using System.Text;
using UnityEngine;

[Serializable]
public class Shortcut
{
    public bool hold = false;
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
            bool result = true;
            if (ctrl && !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl)) result = false;
            if (!ctrl && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) result = false;
            if (alt && !Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.RightAlt)) result = false;
            if (!alt && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))) result = false;
            if (shift && !Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift)) result = false;
            if (!shift && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))) result = false;
            if ((hold && !Input.GetKey(key)) || (!hold && !Input.GetKeyDown(key))) result = false;
            return result;
        }
    }
}
