using Deenote.GameStage;
using Deenote.UI.ComponentModel;
using Deenote.UI.Controls;
using UnityEngine;

namespace Deenote.UI.Views
{
    public sealed class PlayerPropertiesPanelView : MonoBehaviour
    {
        [SerializeField] KVDropdownProperty _aspectRatioProperty = default!;
        [SerializeField] Button _fullScreenButton = default!;
        [SerializeField] KVNumericStepperProperty _noteSpeedProperty = default!;
        [SerializeField] KVNumericSliderProperty _effectVolumeProperty = default!;
        [SerializeField] KVNumericSliderProperty _musicVolumeProperty = default!;
        [SerializeField] KVNumericSliderProperty _pianoVolumeProperty = default!;
        [SerializeField] KVBooleanProperty _showLinksProperty = default!;
        [SerializeField] KVNumericSliderProperty _suddenPlusProperty = default!;

        private void Awake()
        {
            _noteSpeedProperty.InputParser = static input => float.TryParse(input, out var val) ? Mathf.RoundToInt(val * 2) : null;
            _noteSpeedProperty.DisplayTextSelector = static ival => ival % 2 == 0 ? $"{ival / 2}.0" : $"{ival / 2}.5";
            _effectVolumeProperty.DisplayTextSelector = VolumeDisplayTextSelector;
            _musicVolumeProperty.DisplayTextSelector = VolumeDisplayTextSelector;
            _pianoVolumeProperty.DisplayTextSelector = VolumeDisplayTextSelector;
            _suddenPlusProperty.DisplayTextSelector = VolumeDisplayTextSelector;

            static string VolumeDisplayTextSelector(float val) => val.ToString("F0");
        }

        private void Start()
        {
            _aspectRatioProperty.Dropdown.ResetOptions(ViewAspectRatioOptions.DropdownOptions.AsSpan());
            _aspectRatioProperty.Dropdown.OnValueChanged.AddListener(
                val => MainSystem.GameStage.PerspectiveView.AspectRatio = ViewAspectRatioOptions.GetAspectRatio(val));
            MainSystem.GameStage.PerspectiveView.RegisterPropertyChangeNotificationAndInvoke(
                PerspectiveViewController.NotifyProperty.AspectRatio,
                view => _aspectRatioProperty.Dropdown.SetValueWithoutNotify(ViewAspectRatioOptions.FindIndex(view.AspectRatio)));

            _fullScreenButton.OnClick.AddListener(() => MainSystem.GameStage.PerspectiveView.SetFullScreenState(true));

            _noteSpeedProperty.OnValueChanged.AddListener(val => MainSystem.GameStage.NoteSpeed = val);
            _effectVolumeProperty.OnValueChanged.AddListener(val => MainSystem.GameStage.EffectVolume = Mathf.RoundToInt(val));
            _musicVolumeProperty.OnValueChanged.AddListener(val => MainSystem.GameStage.MusicVolume = Mathf.RoundToInt(val));
            _pianoVolumeProperty.OnValueChanged.AddListener(val => MainSystem.GameStage.PianoVolume = Mathf.RoundToInt(val));
            _showLinksProperty.CheckBox.OnValueChanged.AddListener(val => MainSystem.GameStage.IsShowLinkLines = val ?? false);
            _suddenPlusProperty.OnValueChanged.AddListener(val => MainSystem.GameStage.SuddenPlusRange = Mathf.RoundToInt(val));

            MainSystem.GameStage.RegisterPropertyChangeNotificationAndInvoke(
                GameStage.GameStageController.NotifyProperty.NoteSpeed,
                stage => _noteSpeedProperty.Value = stage.NoteSpeed);
            MainSystem.GameStage.RegisterPropertyChangeNotificationAndInvoke(
                GameStage.GameStageController.NotifyProperty.EffectVolume,
                stage => _effectVolumeProperty.Value = stage.EffectVolume);
            MainSystem.GameStage.RegisterPropertyChangeNotificationAndInvoke(
                GameStage.GameStageController.NotifyProperty.MusicVolume,
                stage => _musicVolumeProperty.Value = stage.MusicVolume);
            MainSystem.GameStage.RegisterPropertyChangeNotificationAndInvoke(
                GameStage.GameStageController.NotifyProperty.PianoVolume,
                stage => _pianoVolumeProperty.Value = stage.PianoVolume);
            MainSystem.GameStage.RegisterPropertyChangeNotificationAndInvoke(
                GameStage.GameStageController.NotifyProperty.IsShowLinkLines,
                stage => _showLinksProperty.CheckBox.Value = stage.IsShowLinkLines);
            MainSystem.GameStage.RegisterPropertyChangeNotificationAndInvoke(
                GameStage.GameStageController.NotifyProperty.SuddenPlus,
                stage => _suddenPlusProperty.Value = stage.SuddenPlusRange);
        }
    }
}