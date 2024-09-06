using Deenote.Inputting;
using System.Collections.Generic;
using UnityEngine;

namespace Deenote.ApplicationManaging
{
    public class UserConfig
    {
        public string ApplicationVersion = Application.version;
        public bool FirstLaunch = true;
        public Dictionary<string, ContextualKeyBindingList> KeyBindings { get; set; } = new();
    }
}