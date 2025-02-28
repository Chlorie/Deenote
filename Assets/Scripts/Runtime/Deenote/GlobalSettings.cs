using Deenote.Library.Components;
using Deenote.Systems.Inputting;
using System.Collections.Generic;
using UnityEngine;

namespace Deenote
{
    public sealed class GlobalSettings : FlagNotifiable<GlobalSettings, GlobalSettings.NotificationFlag>
    {
        private bool _isVSyncOn;
        private bool _isIneffectivePropertiesVisible;
        private bool _isFpsShown;
        private Dictionary<string, ContextualKeyBindingList> _keyBindings;

        public GlobalSettings()
        {
            _keyBindings = new();

            MainSystem.SaveSystem.SavingConfigurations += configs =>
            {
                configs.Add("vsync", IsVSyncOn);
                configs.Add("ineffective_prop_visible", IsIneffectivePropertiesVisible);
                configs.Add("fps_shown", IsFpsShown);
                configs.AddDictionary("key_bindings", _keyBindings);
            };
            MainSystem.SaveSystem.LoadedConfigurations += configs =>
            {
                IsVSyncOn = configs.GetBoolean("vsync");
                IsIneffectivePropertiesVisible = configs.GetBoolean("ineffective_prop_visible");
                IsFpsShown = configs.GetBoolean("fps_shown");
                _keyBindings = configs.GetObject("key_bindings", _keyBindings);
            };
        }

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