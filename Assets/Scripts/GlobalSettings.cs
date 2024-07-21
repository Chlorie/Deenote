using System;
using UnityEngine;

namespace Deenote
{
    [Serializable]
    public sealed class GlobalSettings
    {
        [SerializeField]
        private bool __isVSyncOn;
        public bool IsVSyncOn
        {
            get => __isVSyncOn;
            set {
                if (__isVSyncOn == value)
                    return;
                __isVSyncOn = value;
                QualitySettings.vSyncCount = __isVSyncOn ? 1 : 0;
                MainSystem.PreferenceWindow.NotifyIsVSyncOnChanged(__isVSyncOn);
            }
        }
    }
}