using UnityEngine;

namespace Deenote
{
    partial class MainSystem : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] bool __isVSyncOn;

        public static bool IsVSyncOn
        {
            get => _instance. __isVSyncOn;
            set {
                if (_instance.__isVSyncOn == value)
                    return;
                _instance.__isVSyncOn = value;
                QualitySettings.vSyncCount = _instance.__isVSyncOn ? 1 : 0;
                PreferenceWindow.NotifyIsVSyncOnChanged(_instance.__isVSyncOn);
            }
        }
    }
}