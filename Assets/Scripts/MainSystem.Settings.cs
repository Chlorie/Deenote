using UnityEngine;

namespace Deenote
{
    partial class MainSystem
    {
        [Header("Settings")]
        [SerializeField] bool __isVSyncOn;

        public static bool IsVSyncOn
        {
            get => Instance.__isVSyncOn;
            set {
                if (Instance.__isVSyncOn == value)
                    return;
                Instance.__isVSyncOn = value;
                QualitySettings.vSyncCount = Instance.__isVSyncOn ? 1 : 0;
                PreferenceWindow.NotifyIsVSyncOnChanged(Instance.__isVSyncOn);
            }
        }
    }
}