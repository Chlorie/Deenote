#nullable enable

using Deenote.Library;
using Deenote.Library.Components;

namespace Deenote.UI
{
    partial class MainWindow
    {
        private SettingsData _settings = default!;

        public static SettingsData Settings => _instance._settings;

        public sealed class SettingsData : FlagNotifiable<SettingsData, SettingsData.NotificationFlag>
        {
            private float _gameViewScrollSensitivity_bf;
            public float GameViewScrollSensitivity
            {
                get => _gameViewScrollSensitivity_bf;
                set {
                    if (Utils.SetField(ref _gameViewScrollSensitivity_bf, value)) {
                        NotifyFlag(NotificationFlag.GameViewScrollSensitivity);
                    }
                }
            }

            internal SettingsData()
            {
                MainSystem.SaveSystem.SavingConfigurations += configs =>
                {
                    configs.Add("ui/scroll_sensitivity", GameViewScrollSensitivity);
                };
                MainSystem.SaveSystem.LoadedConfigurations += config =>
                {
                    GameViewScrollSensitivity = config.GetSingle("ui/scroll_sensitivity", 1f);
                };
            }

            public enum NotificationFlag
            {
                GameViewScrollSensitivity,
            }
        }
    }
}