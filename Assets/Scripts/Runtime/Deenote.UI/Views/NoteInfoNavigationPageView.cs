#nullable enable

using CommunityToolkit.Diagnostics;
using Deenote.Localization;
using Deenote.Editing;
using Deenote.Entities;
using Deenote.Entities.Models;
using Deenote.Library.Collections;
using Deenote.Library.Components;
using Deenote.UIFramework.Controls;
using Deenote.UI.Views.Panels;
using System;
using System.Linq;
using UnityEngine;

namespace Deenote.UI.Views
{
    public sealed class NoteInfoNavigationPageView : MonoBehaviour
    {
        [SerializeField] TextBox _positionInput = default!;
        [SerializeField] TextBox _timeInput = default!;
        [SerializeField] TextBox _sizeInput = default!;
        [SerializeField] TextBox _durationInput = default!;
        [SerializeField] ToggleButtonGroup _noteKindToggleGroup = default!;
        [SerializeField] ToggleButton _clickNoteKindToggle = default!;
        [SerializeField] ToggleButton _slideNoteKindToggle = default!;
        [SerializeField] ToggleButton _swipeNoteKindToggle = default!;
        [SerializeField] TextBox _speedInput = default!;
        [SerializeField] Button _soundsButton = default!;
        [SerializeField] NoteInfoPianoSoundEditPanel _soundEditPanel = default!;
        [SerializeField] TextBox _shiftInput = default!;
        [SerializeField] TextBox _eventIdInput = default!;
        //[SerializeField] Dropdown _warningTypeDropdown = default!;
        [SerializeField] CheckBox _vibrateCheckBox = default!;

        [SerializeField] GameObject[] _ineffectivePropertyGameObjects = default!;
        private IInteractableControl[] _interactableControls = default!;

        #region Localization Keys

        private const string MultipleValuesPlaceHolderKey = "NavPanel_NotePropertyMultipleValue_PlaceHolder";

        #endregion

        private const string NoSoundButtonText = "-";


        private void Awake()
        {
            _interactableControls = new IInteractableControl[] {
                _positionInput, _timeInput, _sizeInput, _durationInput,
                _clickNoteKindToggle, _slideNoteKindToggle, _swipeNoteKindToggle,
                _speedInput, _shiftInput, _eventIdInput,
                _vibrateCheckBox,
            };

            MainSystem.GlobalSettings.RegisterNotificationAndInvoke(
                GlobalSettings.NotificationFlag.IneffectivePropertiesVisible,
                settings =>
                {
                    var visible = settings.IsIneffectivePropertiesVisible;
                    foreach (var go in _ineffectivePropertyGameObjects) {
                        go.SetActive(visible);
                    }
                });

            // Properties
            #region Position Time Size Duration

            _positionInput.EditSubmitted += text =>
            {
                if (float.TryParse(text, out var value))
                    MainSystem.StageChartEditor.EditSelectedNotesPosition(value);
                SyncFloatInput(_positionInput, value);
            };
            MainSystem.StageChartEditor.RegisterNotification(
                StageChartEditor.NotificationFlag.NotePosition,
                editor => NotifyMultiFloatValueChanged(_positionInput, editor.Selector.SelectedNotes, n => n.Position));
            _timeInput.EditSubmitted += text =>
            {
                if (float.TryParse(text, out var value))
                    MainSystem.StageChartEditor.EditSelectedNotesTime(value);
                SyncFloatInput(_timeInput, value);
            };
            MainSystem.StageChartEditor.RegisterNotification(
                StageChartEditor.NotificationFlag.NoteTime,
                editor => NotifyMultiFloatValueChanged(_timeInput, editor.Selector.SelectedNotes, n => n.Time));
            MainSystem.StageChartEditor.RegisterNotification(
                StageChartEditor.NotificationFlag.NotePositionCoord,
                editor =>
                {
                    NotifyMultiFloatValueChanged(_positionInput, editor.Selector.SelectedNotes, n => n.Position);
                    NotifyMultiFloatValueChanged(_timeInput, editor.Selector.SelectedNotes, n => n.Time);
                });

            _sizeInput.EditSubmitted += text =>
            {
                if (float.TryParse(text, out var value))
                    MainSystem.StageChartEditor.EditSelectedNotesSize(value);
                SyncFloatInput(_sizeInput, value);
            };
            MainSystem.StageChartEditor.RegisterNotification(
                StageChartEditor.NotificationFlag.NoteSize,
                editor => NotifyMultiFloatValueChanged(_sizeInput, editor.Selector.SelectedNotes, n => n.Size));

            _durationInput.EditSubmitted += text =>
            {
                if (float.TryParse(text, out var value))
                    MainSystem.StageChartEditor.EditSelectedNotesDuration(value);
                SyncFloatInput(_durationInput, value);
            };
            MainSystem.StageChartEditor.RegisterNotification(
                StageChartEditor.NotificationFlag.NoteDuration,
                editor => NotifyMultiFloatValueChanged(_durationInput, editor.Selector.SelectedNotes, n => n.Duration));

            #endregion

            #region Kind Speed Sounds

            _clickNoteKindToggle.IsCheckedChanged += check =>
            {
                if (check)
                    MainSystem.StageChartEditor.EditSelectedNotesKind(NoteModel.NoteKind.Click);
            };
            _slideNoteKindToggle.IsCheckedChanged += check =>
            {
                if (check)
                    MainSystem.StageChartEditor.EditSelectedNotesKind(NoteModel.NoteKind.Slide);
            };
            _swipeNoteKindToggle.IsCheckedChanged += check =>
            {
                if (check)
                    MainSystem.StageChartEditor.EditSelectedNotesKind(NoteModel.NoteKind.Swipe);
            };
            MainSystem.StageChartEditor.RegisterNotification(
                StageChartEditor.NotificationFlag.NoteKind,
                editor => NotifyMultiKindChanged(editor.Selector.SelectedNotes));

            _speedInput.EditSubmitted += text =>
            {
                if (float.TryParse(text, out var value))
                    MainSystem.StageChartEditor.EditSelectedNotesSpeed(value);
                SyncFloatInput(_speedInput, value);
            };
            MainSystem.StageChartEditor.RegisterNotification(
                StageChartEditor.NotificationFlag.NoteSpeed,
                editor => NotifyMultiFloatValueChanged(_speedInput, editor.Selector.SelectedNotes, n => n.Speed));

            _soundsButton.Clicked += () =>
            {
                _soundEditPanel.IsActive = !_soundEditPanel.IsActive;
                _soundsButton.Image.sprite = _soundEditPanel.IsActive
                    ? MainWindow.Args.UIIcons.NoteInfoSoundsCollapseSprite
                    : MainWindow.Args.UIIcons.NoteInfoSoundsEditSprite;
            };
            _soundsButton.Image.sprite = MainWindow.Args.UIIcons.NoteInfoSoundsEditSprite;
            _soundEditPanel.IsDirtyChanged += dirty =>
            {
                _soundsButton.Image.sprite = dirty
                    ? MainWindow.Args.UIIcons.NoteInfoSoundsAcceptSprite
                    : MainWindow.Args.UIIcons.NoteInfoSoundsCollapseSprite;
            };

            MainSystem.StageChartEditor.RegisterNotification(
                StageChartEditor.NotificationFlag.NoteSounds,
                editor => NotifyMultiSoundsChanged(editor.Selector.SelectedNotes));

            #endregion

            #region Shift Event WarningType Vibrate

            _shiftInput.EditSubmitted += text =>
            {
                if (float.TryParse(text, out var value))
                    MainSystem.StageChartEditor.EditSelectedNotesShift(value);
                SyncFloatInput(_shiftInput, value);
            };
            MainSystem.StageChartEditor.RegisterNotification(
                StageChartEditor.NotificationFlag.NoteShift,
                editor => NotifyMultiFloatValueChanged(_shiftInput, editor.Selector.SelectedNotes, n => n.Shift));
            _eventIdInput.EditSubmitted += text =>
            {
                MainSystem.StageChartEditor.EditSelectedNotesEventId(text);
                // Avoid display place holder
                _eventIdInput.SetPlaceHolderText(LocalizableText.Raw(""));
            };
            MainSystem.StageChartEditor.RegisterNotification(
                StageChartEditor.NotificationFlag.NoteEventId,
                editor => NotifyMultiEventIdChanged(editor.Selector.SelectedNotes));

            //_warningTypeDropdown.ResetOptions(WarningTypeExt.DropdownOptions);
            //_warningTypeDropdown.SelectedIndexChanged += index =>
            //{
            //    if (index >= 0)
            //        MainSystem.StageChartEditor.EditSelectedNotesWarningType(WarningTypeExt.FromIndex(index));
            //};
            //MainSystem.StageChartEditor.RegisterNotification(
            //    StageChartEditor.NotificationFlag.NoteWarningType,
            //    editor => NotifyMultiWarningTypeChanged(editor.Selector.SelectedNotes));

            _vibrateCheckBox.IsCheckedChanged += check =>
            {
                if (check is { } c) {
                    MainSystem.StageChartEditor.EditSelectedNotesVibrate(c);
                }
            };
            MainSystem.StageChartEditor.RegisterNotification(
                StageChartEditor.NotificationFlag.NoteVibrate,
                editor => NotifyMultiBoolValueChanged(_vibrateCheckBox, editor.Selector.SelectedNotes, n => n.Vibrate));

            #endregion

            MainSystem.StageChartEditor.Selector.RegisterNotificationAndInvoke(
                StageNoteSelector.NotificationFlag.SelectedNotesChanged,
                selector =>
                {
                    var notes = selector.SelectedNotes;
                    switch (notes.Length) {
                        case 0:
                            SetControlsActive(false);
                            break;
                        case 1:
                            SetControlsActive(true);
                            var note = notes[0];
                            SyncFloatInput(_positionInput, note.Position);
                            SyncFloatInput(_timeInput, note.Time);
                            SyncFloatInput(_sizeInput, note.Size);
                            SyncFloatInput(_durationInput, note.Duration);
                            switch (note.Kind) {
                                case NoteModel.NoteKind.Click:
                                    _clickNoteKindToggle.SetIsCheckedWithoutNotify(true);
                                    break;
                                case NoteModel.NoteKind.Slide:
                                    _slideNoteKindToggle.SetIsCheckedWithoutNotify(true);
                                    break;
                                case NoteModel.NoteKind.Swipe:
                                    _swipeNoteKindToggle.SetIsCheckedWithoutNotify(true);
                                    break;
                                default:
                                    break;
                            }
                            SyncFloatInput(_speedInput, note.Speed);
                            NotifyMultiSoundsChanged(notes);
                            SyncFloatInput(_shiftInput, note.Shift);
                            _eventIdInput.SetValueWithoutNotify(note.EventId);
                            //_warningTypeDropdown.SetValueWithoutNotify(note.WarningType.ToIndex());
                            _vibrateCheckBox.SetValueWithoutNotify(note.Vibrate);
                            break;
                        default:
                            SetControlsActive(true);
                            NotifyMultiFloatValueChanged(_positionInput, notes, n => n.Position);
                            NotifyMultiFloatValueChanged(_timeInput, notes, n => n.Time);
                            NotifyMultiFloatValueChanged(_sizeInput, notes, n => n.Size);
                            NotifyMultiFloatValueChanged(_durationInput, notes, n => n.Duration);
                            NotifyMultiKindChanged(notes);
                            NotifyMultiFloatValueChanged(_speedInput, notes, n => n.Speed);
                            NotifyMultiSoundsChanged(notes);
                            NotifyMultiFloatValueChanged(_shiftInput, notes, n => n.Shift);
                            NotifyMultiEventIdChanged(notes);
                            //NotifyMultiWarningTypeChanged(notes);
                            NotifyMultiBoolValueChanged(_vibrateCheckBox, notes, n => n.Vibrate);
                            break;
                    }
                });

            void SetControlsActive(bool active)
            {
                if (active) {
                    if (_interactableControls[0].IsInteractable == false) {
                        var textBoxes = ((ReadOnlySpan<IInteractableControl>)_interactableControls.AsSpan()).OfType<IInteractableControl, TextBox>();
                        foreach (var textBox in textBoxes) {
                            textBox.SetPlaceHolderText(LocalizableText.Localized(MultipleValuesPlaceHolderKey));
                        }
                        foreach (var ctrl in _interactableControls)
                            ctrl.IsInteractable = true;
                    }
                }
                else {
                    if (_interactableControls[0].IsInteractable) {
                        var textBoxes = ((ReadOnlySpan<IInteractableControl>)_interactableControls.AsSpan()).OfType<IInteractableControl, TextBox>();
                        foreach (var textBox in textBoxes) {
                            textBox.SetValueWithoutNotify("");
                            textBox.SetPlaceHolderText(LocalizableText.Raw(""));
                        }

                        _noteKindToggleGroup.ForceToggleOff();
                        _soundsButton.Text.SetRawText(NoSoundButtonText);
                        //_warningTypeDropdown.SetValueWithoutNotify(-1);
                        _vibrateCheckBox.SetValueWithoutNotify(null);

                        foreach (var ctrl in _interactableControls)
                            ctrl.IsInteractable = false;
                    }
                }
            }
        }

        private void SyncFloatInput(TextBox textBox, float value)
            => textBox.SetValueWithoutNotify(value.ToString("F3"));

        private void NotifyMultiFloatValueChanged(TextBox textBox, ReadOnlySpan<NoteModel> notes, Func<NoteModel, float> selector)
            => textBox.SetValueWithoutNotify(notes.IsSameForAll(selector, out var value) ? value.ToString("F3") : "");

        private void NotifyMultiBoolValueChanged(CheckBox checkBox, ReadOnlySpan<NoteModel> notes, Func<NoteModel, bool> selector)
        {
            if (notes.IsSameForAll(selector, out var value))
                checkBox.SetValueWithoutNotify(value);
            else
                checkBox.SetValueWithoutNotify(null);
        }

        private void NotifyMultiKindChanged(ReadOnlySpan<NoteModel> notes)
        {
            if (notes.IsSameForAll(n => n.Kind, out var kind)) {
                var toggle = kind switch {
                    NoteModel.NoteKind.Click => _clickNoteKindToggle,
                    NoteModel.NoteKind.Slide => _slideNoteKindToggle,
                    NoteModel.NoteKind.Swipe => _swipeNoteKindToggle,
                    _ => ThrowHelper.ThrowInvalidOperationException<ToggleButton>("Unknown note kind"),
                };
                toggle.SetIsCheckedWithoutNotify(true);
            }
            else {
                _noteKindToggleGroup.ForceToggleOff();
            }
        }

        private void NotifyMultiSoundsChanged(ReadOnlySpan<NoteModel> notes)
        {
            switch (notes.Length) {
                case 0:
                    _soundsButton.Text.SetRawText(NoSoundButtonText);
                    break;
                case 1: {
                    var sounds = notes[0].Sounds;
                    _soundsButton.Text.SetRawText(GetDisplayText(sounds));
                    break;
                }
                default: {
                    if (NoteInfoPianoSoundEditPanel.HasSameSounds(notes)) {
                        _soundsButton.Text.SetRawText(GetDisplayText(notes[0].Sounds));
                    }
                    else {
                        _soundsButton.Text.SetRawText(NoSoundButtonText);
                    }
                    break;
                }
            }

            static string GetDisplayText(ReadOnlySpan<PianoSoundValueModel> sounds)
            {
                return sounds.Length switch {
                    0 => NoSoundButtonText,
                    1 => sounds[0].ToPitchDisplayString(),
                    _ => sounds.Length.ToString(),
                };
            }
        }

        private void NotifyMultiEventIdChanged(ReadOnlySpan<NoteModel> notes)
        {
            if (notes.IsSameForAll(n => n.EventId, out var eventId)) {
                _eventIdInput.SetValueWithoutNotify(eventId);
                if (eventId is "")
                    // Avoid display place holder
                    _eventIdInput.SetPlaceHolderText(LocalizableText.Raw(""));
            }
            else {
                _eventIdInput.SetValueWithoutNotify("");
                _eventIdInput.SetPlaceHolderText(LocalizableText.Localized(MultipleValuesPlaceHolderKey));
            }
        }

        //private void NotifyMultiWarningTypeChanged(ReadOnlySpan<NoteModel> notes)
        //{
        //    if (notes.IsSameForAll(n => n.WarningType, out var warningType))
        //        _warningTypeDropdown.SetValueWithoutNotify(warningType.ToIndex());
        //    else
        //        _warningTypeDropdown.SetValueWithoutNotify(-1);
        //}
    }
}
