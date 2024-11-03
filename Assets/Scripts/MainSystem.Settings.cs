#nullable enable

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
                    _propertyChangeNotifier.Invoke(this, NotifyProperty.VSync);
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
                    _propertyChangeNotifier.Invoke(this, NotifyProperty.IneffectivePropertiesVisiblility);
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
                    _propertyChangeNotifier.Invoke(this, NotifyProperty.FpsShown);
                }
            }

            private PropertyChangeNotifier<Settings, NotifyProperty> _propertyChangeNotifier = new();
            public void RegisterPropertyChangeNotification(NotifyProperty flag, Action<Settings> action) 
                => _propertyChangeNotifier.AddListener(flag, action);

            public enum NotifyProperty
            {
                VSync,
                IneffectivePropertiesVisiblility,
                FpsShown,
            }
        }
    }
}