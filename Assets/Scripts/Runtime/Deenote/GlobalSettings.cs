using Deenote.Library;
using Deenote.Library.Components;
using Deenote.Localization;
using UnityEngine;

namespace Deenote
{
    public sealed class GlobalSettings : FlagNotifiable<GlobalSettings, GlobalSettings.NotificationFlag>
    {
        private bool _isVSyncOn;
        private bool _isIneffectivePropertiesVisible;
        private bool _isFpsShown;
        private float _gameViewScrollSensitivity_bf;
        private bool _checkUpdateOnStartup;

        public GlobalSettings()
        {
            MainSystem.SaveSystem.SavingConfigurations += configs =>
            {
                configs.Set("language", LocalizationSystem.CurrentLanguage.LanguageCode);
                configs.Set("vsync", IsVSyncOn);
                configs.Set("ineffective_prop_visible", IsIneffectivePropertiesVisible);
                configs.Set("fps_shown", IsFpsShown);
                configs.Set("scroll_sensitivity", GameViewScrollSensitivity);
                configs.Set("check_update", CheckUpdateOnStartup);
            };
            MainSystem.SaveSystem.LoadedConfigurations += configs =>
            {
                if (!LocalizationSystem.TrySetLanguage(configs.GetString("language", LocalizationSystem.DefaultLanguageCode))) {
                    var res = LocalizationSystem.TrySetLanguage(LocalizationSystem.DefaultLanguageCode);
                    Debug.Assert(res, "Failed to set default langeuage");
                }
                IsVSyncOn = configs.GetBoolean("vsync");
                IsIneffectivePropertiesVisible = configs.GetBoolean("ineffective_prop_visible");
                IsFpsShown = configs.GetBoolean("fps_shown");
                GameViewScrollSensitivity = configs.GetSingle("scroll_sensitivity", 1f);
                CheckUpdateOnStartup = configs.GetBoolean("check_update", true);
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

        public float GameViewScrollSensitivity
        {
            get => _gameViewScrollSensitivity_bf;
            set {
                if (Utils.SetField(ref _gameViewScrollSensitivity_bf, value)) {
                    NotifyFlag(NotificationFlag.GameViewScrollSensitivity);
                }
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

        public bool CheckUpdateOnStartup
        {
            get => _checkUpdateOnStartup;
            set {
                if (_checkUpdateOnStartup == value)
                    return;

                _checkUpdateOnStartup = value;
                NotifyFlag(NotificationFlag.CheckUpdateOnStartup);
            }
        }

        public enum NotificationFlag
        {
            VSync,
            IneffectivePropertiesVisible,
            FpsShown,
            GameViewScrollSensitivity,
            CheckUpdateOnStartup,
        }

    }
}