#nullable enable

using Deenote.Library;
using Deenote.Library.Components;

namespace Deenote.UI.Dialogs
{
    public partial class PezConvertDialog
    {
        // Since these settings are only used for this panel, just put it as an inner class.
        public sealed class Settings : FlagNotifiable<Settings, Settings.NotificationFlag>
        {
            private float _speed;
            private float _speedCoefficient;
            private float _speedExponent;
            private float _widthCoefficient;
            private float _widthExponent;
            private float _widthMultiplier;
            private float _holdDragInterval;
            private int _holdAlpha;
            private bool _needFlickClick;
            private bool _convertMp3ToOgg;
            private bool _earlyDisplaySlowNotes;
            private string _outputDir = string.Empty;

            public Settings()
            {
                MainSystem.SaveSystem.SavingConfigurations += configs =>
                {
                    configs.Set("pez/speed", Speed);
                    configs.Set("pez/speed_coefficient", SpeedCoefficient);
                    configs.Set("pez/speed_exponent", SpeedExponent);
                    configs.Set("pez/width_coefficient", WidthCoefficient);
                    configs.Set("pez/width_exponent", WidthExponent);
                    configs.Set("pez/width_multiplier", WidthMultiplier);
                    configs.Set("pez/hold_drag_interval", HoldDragInterval);
                    configs.Set("pez/hold_alpha", HoldAlpha);
                    configs.Set("pez/need_flick_click", NeedFlickClick);
                    configs.Set("pez/early_display_slow_notes", EarlyDisplaySlowNotes);
                    configs.Set("pez/output_dir", OutputDirectory);
                };
                MainSystem.SaveSystem.LoadedConfigurations += configs =>
                {
                    Speed = configs.GetSingle("pez/speed", 10f);
                    SpeedCoefficient = configs.GetSingle("pez/speed_coefficient", 1f);
                    SpeedExponent = configs.GetSingle("pez/speed_exponent", 1f);
                    WidthCoefficient = configs.GetSingle("pez/width_coefficient", 1f);
                    WidthExponent = configs.GetSingle("pez/width_exponent", 1f);
                    WidthMultiplier = configs.GetSingle("pez/width_multiplier", 1f);
                    HoldDragInterval = configs.GetSingle("pez/hold_drag_interval", 0.08f);
                    HoldAlpha = configs.GetInt32("pez/hold_alpha", 165);
                    NeedFlickClick = configs.GetBoolean("pez/need_flick_click", true);
                    EarlyDisplaySlowNotes = configs.GetBoolean("pez/early_display_slow_notes", false);
                    OutputDirectory = configs.GetString("pez/output_dir", string.Empty)!;
                };
                MainSystem.SaveSystem.LoadConfigurations();
            }

            public float Speed
            {
                get => _speed;
                set {
                    if (Utils.SetField(ref _speed, value)) 
                        NotifyFlag(NotificationFlag.Speed);
                }
            }

            public float SpeedCoefficient
            {
                get => _speedCoefficient;
                set {
                    if (Utils.SetField(ref _speedCoefficient, value)) 
                        NotifyFlag(NotificationFlag.SpeedCoefficient);
                }
            }

            public float SpeedExponent
            {
                get => _speedExponent;
                set {
                    if (Utils.SetField(ref _speedExponent, value)) 
                        NotifyFlag(NotificationFlag.SpeedExponent);
                }
            }

            public float WidthCoefficient
            {
                get => _widthCoefficient;
                set {
                    if (Utils.SetField(ref _widthCoefficient, value)) 
                        NotifyFlag(NotificationFlag.WidthCoefficient);
                }
            }

            public float WidthExponent
            {
                get => _widthExponent;
                set {
                    if (Utils.SetField(ref _widthExponent, value)) 
                        NotifyFlag(NotificationFlag.WidthExponent);
                }
            }

            public float WidthMultiplier
            {
                get => _widthMultiplier;
                set {
                    if (Utils.SetField(ref _widthMultiplier, value)) 
                        NotifyFlag(NotificationFlag.WidthMultiplier);
                }
            }

            public float HoldDragInterval
            {
                get => _holdDragInterval;
                set {
                    if (Utils.SetField(ref _holdDragInterval, value)) 
                        NotifyFlag(NotificationFlag.HoldDragInterval);
                }
            }

            public int HoldAlpha
            {
                get => _holdAlpha;
                set {
                    if (Utils.SetField(ref _holdAlpha, value)) 
                        NotifyFlag(NotificationFlag.HoldAlpha);
                }
            }

            public bool NeedFlickClick
            {
                get => _needFlickClick;
                set {
                    if (Utils.SetField(ref _needFlickClick, value)) 
                        NotifyFlag(NotificationFlag.NeedFlickClick);
                }
            }

            public bool EarlyDisplaySlowNotes
            {
                get => _earlyDisplaySlowNotes;
                set {
                    if (Utils.SetField(ref _earlyDisplaySlowNotes, value)) 
                        NotifyFlag(NotificationFlag.EarlyDisplaySlowNotes);
                }
            }

            public string OutputDirectory
            {
                get => _outputDir;
                set {
                    if (Utils.SetField(ref _outputDir, value)) 
                        NotifyFlag(NotificationFlag.OutputDirectory);
                }
            }

            public void ResetToDefault()
            {
                Speed = 10f;
                SpeedCoefficient = 1f;
                SpeedExponent = 1f;
                WidthCoefficient = 1f;
                WidthExponent = 1f;
                WidthMultiplier = 1f;
                HoldDragInterval = 0.08f;
                HoldAlpha = 165;
                NeedFlickClick = true;
                EarlyDisplaySlowNotes = false;
                OutputDirectory = string.Empty;
            }

            public enum NotificationFlag
            {
                Speed,
                SpeedCoefficient,
                SpeedExponent,
                WidthCoefficient,
                WidthExponent,
                WidthMultiplier,
                HoldDragInterval,
                HoldAlpha,
                NeedFlickClick,
                EarlyDisplaySlowNotes,
                OutputDirectory,
            }
        }
    }
}