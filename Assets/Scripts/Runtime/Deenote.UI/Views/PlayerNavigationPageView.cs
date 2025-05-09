#nullable enable

using CommunityToolkit.Diagnostics;
using Deenote.Core.Editing;
using Deenote.Core.GamePlay;
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
        [SerializeField] ToggleSwitch _earlyDisplaySlowNotesToggle = default!;

        private void Start()
        {
            #region View Screen

            _aspectRatioDropdown.ResetOptions(_predefinedAspectTexts);
            _aspectRatioDropdown.SelectedIndexChanged += val =>
            {
                if (val >= 0)
                    MainWindow.Views.PerspectiveViewPanelView.AspectRatio = GetAspectRatioDropdownOption(val);
            };
            MainWindow.Views.PerspectiveViewPanelView.AspectRatioChanged += val => _aspectRatioDropdown.SetValueWithoutNotify(GetAspectRatioDropdownIndex(val));
            _aspectRatioDropdown.SetValueWithoutNotify(GetAspectRatioDropdownIndex(MainWindow.Views.PerspectiveViewPanelView.AspectRatio));

            _fullScreenButton.Clicked += () => MainWindow.Views.PerspectiveViewPanelView.SetIsFullScreen(true);

            #endregion

            #region NoteSpeed Volumes Sudden+

            static void Sync01Range(TextBox input, Slider slider, float value)
            {
                input.SetValueWithoutNotify((value * 100f).ToString("F0"));
                slider.SetValueWithoutNotify(value);
            }

            _noteSpeedNumericStepper.SetInputParser(static input => float.TryParse(input, out var val) ? Mathf.RoundToInt(val * 10f) : null);
            _noteSpeedNumericStepper.SetDisplayerTextSelector(static ival => $"{ival / 10}.{ival % 10}");
            _noteSpeedNumericStepper.ValueChanged += val => MainSystem.GamePlayManager.NoteFallSpeed = val;
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

            MainSystem.GamePlayManager.RegisterNotificationAndInvoke(
                GamePlayManager.NotificationFlag.NoteSpeed,
                manager => _noteSpeedNumericStepper.Value = manager.NoteFallSpeed);
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

            #endregion

            _linksIndicatorToggle.IsCheckedChanged += val => MainSystem.GamePlayManager.IsShowLinkLines = val;
            _placementIndicatorToggle.IsCheckedChanged += val => MainSystem.StageChartEditor.Placer.IsIndicatorOn = val;
            _earlyDisplaySlowNotesToggle.IsCheckedChanged += val => MainSystem.GamePlayManager.EarlyDisplaySlowNotes = val;

            MainSystem.GamePlayManager.RegisterNotificationAndInvoke(
                GamePlayManager.NotificationFlag.IsShowLinkLines,
                manager => _linksIndicatorToggle.SetIsCheckedWithoutNotify(manager.IsShowLinkLines));
            MainSystem.StageChartEditor.Placer.RegisterNotificationAndInvoke(
                StageNotePlacer.NotificationFlag.IsIndicatorOn,
                placer => _placementIndicatorToggle.SetIsCheckedWithoutNotify(placer.IsIndicatorOn));
            MainSystem.GamePlayManager.RegisterNotification(
                GamePlayManager.NotificationFlag.EarlyDisplaySlowNotes,
                manager => _earlyDisplaySlowNotesToggle.SetIsCheckedWithoutNotify(manager.EarlyDisplaySlowNotes));
        }

        private static readonly string[] _predefinedAspectTexts = { "16:9","16:10", "4:3" };

        private static float GetAspectRatioDropdownOption(int optionIndex)
            => optionIndex switch {
                0 => 16f / 9f,
                1 => 16f / 10f,
                2 => 4f / 3f,
                _ => ThrowHelper.ThrowInvalidOperationException<float>(),
            };

        private static int GetAspectRatioDropdownIndex(float option)
            => option switch {
                16f / 9f => 0,
                16f / 10f => 1,
                4f / 3f => 2,
                _ => -1,
            };
    }
}