using Deenote.UI.Controls;
using UnityEngine;

namespace Deenote.UI.Views
{
    public sealed class PlayerPropertiesPanelView : MonoBehaviour
    {
        [SerializeField] KVNumericStepperProperty _noteSpeedProperty = default!;
        [SerializeField] KVNumericSliderProperty _effectVolumeProperty = default!;
        [SerializeField] KVNumericSliderProperty _musicVolumeProperty = default!;
        [SerializeField] KVNumericSliderProperty _pianoVolumeProperty = default!;
        [SerializeField] KVBooleanProperty _showLinksProperty = default!;
        [SerializeField] KVNumericSliderProperty _suddenPlusProperty = default!;

        private void Start()
        {
            _noteSpeedProperty.InputParser = static input => float.TryParse(input, out var val) ? Mathf.RoundToInt(val * 2) : null;
            _noteSpeedProperty.DisplayTextSelector = static ival => ival % 2 == 0 ? $"{ival / 2}.0" : $"{ival / 2}.5";
            _noteSpeedProperty.OnValueChanged.AddListener(val => MainSystem.GameStage.NoteSpeed = val);
            _effectVolumeProperty.DisplayTextSelector = VolumeDisplayTextSelector;
            _effectVolumeProperty.OnValueChanged.AddListener(val => MainSystem.GameStage.EffectVolume = Mathf.RoundToInt(val));
            _musicVolumeProperty.DisplayTextSelector = VolumeDisplayTextSelector;
            _musicVolumeProperty.OnValueChanged.AddListener(val => MainSystem.GameStage.MusicVolume = Mathf.RoundToInt(val));
            _pianoVolumeProperty.DisplayTextSelector = VolumeDisplayTextSelector;
            _pianoVolumeProperty.OnValueChanged.AddListener(val => MainSystem.GameStage.PianoVolume = Mathf.RoundToInt(val));
            _showLinksProperty.CheckBox.OnValueChanged.AddListener(val => MainSystem.GameStage.IsShowLinkLines = val ?? false);
            _suddenPlusProperty.DisplayTextSelector = VolumeDisplayTextSelector;
            _suddenPlusProperty.OnValueChanged.AddListener(val => MainSystem.GameStage.SuddenPlusRange = Mathf.RoundToInt(val));

            static string VolumeDisplayTextSelector(float val) => val.ToString("F0");

            MainSystem.GameStage.RegisterPropertyChangeNotification(
                GameStage.GameStageController.NotifyProperty.NoteSpeed,
                stage => _noteSpeedProperty.Value = stage.NoteSpeed);
            MainSystem.GameStage.RegisterPropertyChangeNotification(
                GameStage.GameStageController.NotifyProperty.EffectVolume,
                stage => _effectVolumeProperty.Value = stage.EffectVolume);
            MainSystem.GameStage.RegisterPropertyChangeNotification(
                GameStage.GameStageController.NotifyProperty.MusicVolume,
                stage => _musicVolumeProperty.Value = stage.MusicVolume);
            MainSystem.GameStage.RegisterPropertyChangeNotification(
                GameStage.GameStageController.NotifyProperty.PianoVolume,
                stage => _pianoVolumeProperty.Value = stage.PianoVolume);
            MainSystem.GameStage.RegisterPropertyChangeNotification(
                GameStage.GameStageController.NotifyProperty.IsShowLinkLines,
                stage => _showLinksProperty.CheckBox.Value = stage.IsShowLinkLines);
            MainSystem.GameStage.RegisterPropertyChangeNotification(
                GameStage.GameStageController.NotifyProperty.SuddenPlus,
                stage => _suddenPlusProperty.Value = stage.SuddenPlusRange);
        }
    }
}