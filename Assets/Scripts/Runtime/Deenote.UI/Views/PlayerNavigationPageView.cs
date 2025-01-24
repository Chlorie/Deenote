#nullable enable

using Deenote.Editing;
using Deenote.GamePlay;
using Deenote.Library.Components;
using Deenote.UIFramework.Controls;
using System;
using UnityEngine;

namespace Deenote.UI.Views
{
    public sealed class PlayerNavigationPageView : MonoBehaviour
    {
        [SerializeField] Dropdown _aspectRatioDropdown = default!;
        [SerializeField] Button _fullScreenButton = default!;
        [SerializeField] NumericStepper _noteSpeedNumericStepper = default!;
        [SerializeField] Slider _musicVolumeSlider = default!;
        [SerializeField] TextBox _musicVolumeInput = default!;
        [SerializeField] Slider _effectVolumeSlider = default!;
        [SerializeField] TextBox _effectVolumeInput = default!;
        [SerializeField] Slider _pianoVolumeSlider = default!;
        [SerializeField] TextBox _pianoVolumeInput = default!;
        [SerializeField] Slider _suddenPlusSlider = default!;
        [SerializeField] TextBox _suddenPlusInput = default!;
        [SerializeField] ToggleSwitch _linksIndicatorToggle = default!;
        [SerializeField] ToggleSwitch _placementIndicatorToggle = default!;

        private static readonly string[] PredefinedAspectTexts = { "16:9", "4:3" };
        private static readonly float[] PredefinedAspectValues = { 16f / 9f, 4f / 3f };

        private void Awake()
        {
            #region View Screen

            _aspectRatioDropdown.ResetOptions(PredefinedAspectTexts);
            _aspectRatioDropdown.SelectedIndexChanged += val =>
            {
                if (val >= 0)
                    MainWindow.PerspectiveViewPanelView.AspectRatio = PredefinedAspectValues[val];
            };
            MainWindow.PerspectiveViewPanelView.AspectRatioChanged += aspectRatio =>
            {
                var index = Array.IndexOf(PredefinedAspectValues, aspectRatio);
                _aspectRatioDropdown.SetValueWithoutNotify(index);
            };

            _fullScreenButton.Clicked += () => MainWindow.PerspectiveViewPanelView.SetIsFullScreen(true);

            #endregion

            #region NoteSpeed Volumes Sudden+

            static void Sync01Range(TextBox input, Slider slider, float value)
            {
                input.SetValueWithoutNotify((value * 100f).ToString("F0"));
                slider.SetValueWithoutNotify(value);
            }

            _noteSpeedNumericStepper.SetInputParser(static input => float.TryParse(input, out var val) ? Mathf.RoundToInt(val * 10f) : null);
            _noteSpeedNumericStepper.SetDisplayerTextSelector(static ival => $"{ival / 10}.{ival % 10}");
            _noteSpeedNumericStepper.ValueChanged += val => MainSystem.GamePlayManager.NoteSpeed = val;
            _musicVolumeSlider.ValueChanged += val => MainSystem.GamePlayManager.MusicVolume = val;
            _musicVolumeInput.EditSubmitted += input =>
            {
                if (int.TryParse(input, out var ival))
                    MainSystem.GamePlayManager.MusicVolume = Mathf.Clamp01(ival / 100f);
                else
                    Sync01Range(_musicVolumeInput, _musicVolumeSlider, MainSystem.GamePlayManager.MusicVolume);
            };
            _effectVolumeSlider.ValueChanged += val => MainSystem.GamePlayManager.HitSoundVolume = val;
            _effectVolumeInput.EditSubmitted += input =>
            {
                if (int.TryParse(input, out var ival))
                    MainSystem.GamePlayManager.HitSoundVolume = Mathf.Clamp01(ival / 100f);
                else
                    Sync01Range(_effectVolumeInput, _effectVolumeSlider, MainSystem.GamePlayManager.HitSoundVolume);
            };
            _pianoVolumeSlider.ValueChanged += val => MainSystem.GamePlayManager.PianoVolume = val;
            _pianoVolumeInput.EditSubmitted += input =>
            {
                if (int.TryParse(input, out var ival))
                    MainSystem.GamePlayManager.PianoVolume = Mathf.Clamp01(ival / 100f);
                else
                    Sync01Range(_pianoVolumeInput, _pianoVolumeSlider, MainSystem.GamePlayManager.PianoVolume);
            };
            _suddenPlusSlider.ValueChanged += val => MainSystem.GamePlayManager.SuddenPlus = val;
            _suddenPlusInput.EditSubmitted += input =>
            {
                if (int.TryParse(input, out var ival))
                    MainSystem.GamePlayManager.SuddenPlus = Mathf.Clamp01(ival / 100f);
                else
                    Sync01Range(_suddenPlusInput, _suddenPlusSlider, MainSystem.GamePlayManager.SuddenPlus);
            };

            #endregion

            _linksIndicatorToggle.IsCheckedChanged += val => MainSystem.GamePlayManager.IsShowLinkLines = val;
            _placementIndicatorToggle.IsCheckedChanged += val => MainSystem.StageChartEditor.Placer.IsIndicatorOn = val;

            MainSystem.GamePlayManager.RegisterNotificationAndInvoke(
                GamePlayManager.NotificationFlag.NoteSpeed,
                manager => _noteSpeedNumericStepper.Value = manager.NoteSpeed);
            MainSystem.GamePlayManager.RegisterNotificationAndInvoke(
                GamePlayManager.NotificationFlag.MusicVolume,
                manager => Sync01Range(_musicVolumeInput, _musicVolumeSlider, manager.MusicVolume));
            MainSystem.GamePlayManager.RegisterNotificationAndInvoke(
                GamePlayManager.NotificationFlag.HitSoundVolume,
                manager => Sync01Range(_effectVolumeInput, _effectVolumeSlider, manager.HitSoundVolume));
            MainSystem.GamePlayManager.RegisterNotificationAndInvoke(
                GamePlayManager.NotificationFlag.PianoVolume,
                manager => Sync01Range(_pianoVolumeInput, _pianoVolumeSlider, manager.PianoVolume));
            MainSystem.GamePlayManager.RegisterNotificationAndInvoke(
                GamePlayManager.NotificationFlag.SuddenPlus,
                manager => Sync01Range(_suddenPlusInput, _suddenPlusSlider, manager.SuddenPlus));
            MainSystem.GamePlayManager.RegisterNotificationAndInvoke(
                GamePlayManager.NotificationFlag.IsShowLinkLines,
                manager => _linksIndicatorToggle.IsChecked = manager.IsShowLinkLines);
            MainSystem.StageChartEditor.Placer.RegisterNotificationAndInvoke(
                StageNotePlacer.NotificationFlag.IsIndicatorOn,
                placer => _placementIndicatorToggle.IsChecked = placer.IsIndicatorOn);
        }
    }
}
