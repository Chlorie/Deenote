#nullable enable

using Deenote.Library;
using Deenote.Library.Components;

namespace Deenote.UI
{
    partial class MainWindow
    {
        private SettingsData _settings = new();

        public static SettingsData Settings => _instance._settings;

        public sealed class SettingsData : FlagNotifiable<SettingsData, SettingsData.NotificationFlag>
        {
            private float _gameViewScrollSensitivity_bf = 1;
            public float GameViewScrollSensitivity
            {
                get => _gameViewScrollSensitivity_bf;
                set {
                    if (Utils.SetField(ref _gameViewScrollSensitivity_bf, value)) {
                        NotifyFlag(NotificationFlag.GameViewScrollSensitivity);
                    }
                }
            }

            public enum NotificationFlag
            {
                GameViewScrollSensitivity,
            }
        }
    }
}