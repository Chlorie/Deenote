using Deenote.Settings;
using Deenote.UI;
using Deenote.UI.ComponentModel;
using System;
using UnityEngine;

namespace Deenote
{
    partial class MainSystem
    {
        [Header("Args")]
        [SerializeField] GameStageViewArgs _gameStageViewArgs;
        [SerializeField] KnownIconsArgs _knownIconsArgs;

        partial class Args
        {
            public static GameStageViewArgs GameStageViewArgs => Instance._gameStageViewArgs;

            public static KnownIconsArgs KnownIconsArgs => Instance._knownIconsArgs;
        }

        public sealed class Settings : INotifyPropertyChange<Settings, Settings.NotifyProperty>
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
                    _propertyChangedNotifier.Invoke(this, NotifyProperty.VSync);
                }
            }

            private readonly PropertyChangeNotifier<Settings, NotifyProperty> _propertyChangedNotifier = new();
            public void RegisterPropertyChangeNotification(NotifyProperty flag, Action<Settings> action)
                => _propertyChangedNotifier.AddListener(flag, action);

            public enum NotifyProperty
            {
                VSync,
            }
        }
    }
}