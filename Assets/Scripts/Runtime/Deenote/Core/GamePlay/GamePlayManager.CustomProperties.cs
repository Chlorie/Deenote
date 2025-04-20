#nullable enable

using Deenote.Library;
using Deenote.Library.Mathematics;
using UnityEngine;

namespace Deenote.Core.GamePlay
{
    partial class GamePlayManager
    {
        private void RegisterCustomPropertiesConfigurations()
        {
            MainSystem.SaveSystem.SavingConfigurations += configs =>
            {
                configs.Add("stage/line-color-subbeat", CustomSubBeatLineColor?.ToRGBAString());
                configs.Add("stage/line-color-beat", CustomBeatLineColor?.ToRGBAString());
                configs.Add("stage/line-color-tempo", CustomTempoLineColor?.ToRGBAString());
            };
            MainSystem.SaveSystem.LoadedConfigurations += configs =>
            {
                if (ColorUtils.TryParse(configs.GetString("stage/line-color-subbeat"), out var sbc)) {
                    CustomSubBeatLineColor = sbc;
                }
                if (ColorUtils.TryParse(configs.GetString("stage/line-color-beat"), out var bc)) {
                    CustomBeatLineColor = bc;
                }
                if (ColorUtils.TryParse(configs.GetString("stage/line-color-tempo"), out var tc)) {
                    CustomTempoLineColor = tc;
                }
            };
        }

        private Color? _customSubBeatLineColor;
        private Color? _customBeatLineColor;
        private Color? _customTempoLineColor;
        public Color? CustomSubBeatLineColor
        {
            get => _customSubBeatLineColor;
            set {
                if (Utils.SetField(ref _customSubBeatLineColor, value)) {
                    NotifyFlag(NotificationFlag.CustomSubBeatLineColor);
                }
            }
        }
        public Color? CustomBeatLineColor
        {
            get => _customBeatLineColor;
            set {
                if (Utils.SetField(ref _customBeatLineColor, value)) {
                    NotifyFlag(NotificationFlag.CustomBeatLineColor);
                }
            }
        }
        public Color? CustomTempoLineColor
        {
            get => _customTempoLineColor;
            set {
                if (Utils.SetField(ref _customTempoLineColor, value)) {
                    NotifyFlag(NotificationFlag.CustomTempoLineColor);
                }
            }
        }
    }
}