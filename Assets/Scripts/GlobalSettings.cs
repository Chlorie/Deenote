using System;
using UnityEngine;

namespace Deenote
{
    [Serializable]
    public sealed class GlobalSettings
    {
        [field: SerializeField]
        public bool ShowFps { get; set; }

        [field: SerializeField]
        public bool IsVSyncOn { get; set; }

        [field: SerializeField, Range(-10, 10)]
        public float MouseScrollSensitivity { get; set; }

        [field: SerializeField]
        public string Language { get; set; } // ?

        [field: SerializeField]
        public AutoSaveOption AutoSave { get; set; }

        [field: SerializeField]
        public ResolutionOption Resolution { get; set; }

        public enum AutoSaveOption
        {
            Off,
            On,
            OnAndSaveJson,
        }

        public enum ResolutionOption
        {
            _960x540,
            _1280x720,
            _1920x1080,
        }
    }
}