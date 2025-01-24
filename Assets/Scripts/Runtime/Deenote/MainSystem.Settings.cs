#nullable enable

using Deenote.Library.Components;
using UnityEngine;

namespace Deenote
{
    partial class MainSystem
    {
        public sealed class Settings : FlagNotifiable<Settings, Settings.NotificationFlag>
        {
            private bool _isVSyncOn;
            public bool IsVSyncOn
            {
                get => _isVSyncOn;
                set {
                    if (_isVSyncOn == value)
                        return;
                    _isVSyncOn = value;
                    QualitySettings.vSyncCount = _isVSyncOn ? 1 : 0;
                    NotifyFlag(NotificationFlag.VSync);
                }
            }

            private bool _isIneffectivePropertiesVisible;
            public bool IsIneffectivePropertiesVisible
            {
                get => _isIneffectivePropertiesVisible;
                set {
                    if (_isIneffectivePropertiesVisible == value)
                        return;

                    _isIneffectivePropertiesVisible = value;
                    NotifyFlag(NotificationFlag.IneffectivePropertiesVisible);
                }
            }

            private bool _isFpsShown;
            public bool IsFpsShown
            {
                get => _isFpsShown;
                set {
                    if (_isFpsShown == value)
                        return;

                    _isFpsShown = value;
                    NotifyFlag(NotificationFlag.FpsShown);
                }
            }

            public enum NotificationFlag
            {
                VSync,
                IneffectivePropertiesVisible,
                FpsShown,
            }
        }
    }
}