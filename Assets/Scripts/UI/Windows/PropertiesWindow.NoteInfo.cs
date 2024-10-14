using Deenote.Project.Comparers;
using Deenote.Project.Models;
using Deenote.Project.Models.Datas;
using Deenote.Utilities;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.Windows
{
    partial class PropertiesWindow
    {
        [Header("Note Info")]
        [SerializeField] Button _noteInfoGroupButton;
        [SerializeField] GameObject _noteInfoGroupGameObject;
        [SerializeField] TMP_InputField _notePositionInputField;
        [SerializeField] TMP_InputField _noteTimeInputField;
        [SerializeField] TMP_InputField _noteSizeInputField;
        [SerializeField] TMP_InputField _noteShiftInputField;
        [SerializeField] TMP_InputField _noteSpeedInputField;
        [SerializeField] TMP_InputField _noteDurationInputField;
        [SerializeField] Toggle _noteVibrateToggle;
        [SerializeField] Toggle _noteSwipeToggle;
        [SerializeField] TMP_InputField _noteWarningTypeInputField;
        [SerializeField] TMP_InputField _noteEventIdInputField;
        [SerializeField] Toggle _noteIsLinkToggle;
        [SerializeField] Button _noteSoundsButton;
        [SerializeField] TMP_Text _noteSoundsText;

        private void AwakeNoteInfo()
        {
            _noteInfoGroupButton.onClick.AddListener(() =>
                _noteInfoGroupGameObject.SetActive(!_noteInfoGroupGameObject.activeSelf));
            _notePositionInputField.onEndEdit.AddListener(OnNotePositionChanged);
            _noteTimeInputField.onEndEdit.AddListener(OnNoteTimeChanged);
            _noteSizeInputField.onEndEdit.AddListener(OnNoteSizeChanged);
            _noteShiftInputField.onEndEdit.AddListener(OnNoteShiftChanged);
            _noteSpeedInputField.onEndEdit.AddListener(OnNoteSpeedChanged);
            _noteDurationInputField.onEndEdit.AddListener(OnNoteDurationChanged);
            _noteVibrateToggle.onValueChanged.AddListener(OnNoteVibrateChanged);
            _noteSwipeToggle.onValueChanged.AddListener(OnNoteIsSwipeChanged);
            _noteWarningTypeInputField.onEndEdit.AddListener(OnNoteWarningTypeChanged);
            _noteEventIdInputField.onEndEdit.AddListener(OnNoteEventIdChanged);
            _noteIsLinkToggle.onValueChanged.AddListener(OnNoteIsLinkChanged);
            _noteSoundsButton.onClick.AddListener(() => MainSystem.PianoSoundEdit.Window.IsActivated = true);
        }

        #region Events

        private void OnNotePositionChanged(string value)
        {
            if (float.TryParse(value, out var pos))
                _editorController.EditSelectedNotesPosition(pos);
            else
                _editorController.EditSelectedNotesPosition(0f);
        }

        private void OnNoteTimeChanged(string value)
        {
            if (float.TryParse(value, out var time))
                _editorController.EditSelectedNotesTime(time);
            else
                _editorController.EditSelectedNotesTime(0f);
        }

        private void OnNoteSizeChanged(string value)
        {
            if (float.TryParse(value, out var size))
                _editorController.EditSelectedNotesSize(size);
            else
                _editorController.EditSelectedNotesSize(0f);
        }

        private void OnNoteShiftChanged(string value)
        {
            if (float.TryParse(value, out var shift))
                _editorController.EditSelectedNotesShift(shift);
            else
                _editorController.EditSelectedNotesShift(0f);
        }

        private void OnNoteSpeedChanged(string value)
        {
            if (float.TryParse(value, out var speed))
                _editorController.EditSelectedNotesSpeed(speed);
            else
                _editorController.EditSelectedNotesSpeed(0f);
        }

        private void OnNoteDurationChanged(string value)
        {
            if (float.TryParse(value, out var duration))
                _editorController.EditSelectedNotesDuration(duration);
            else
                _editorController.EditSelectedNotesDuration(0f);
        }

        private void OnNoteVibrateChanged(bool value)
        {
            _editorController.EditSelectedNotesVibrate(value);
        }

        private void OnNoteIsSwipeChanged(bool value)
        {
            _editorController.EditSelectedNotesIsSwipe(value);
        }

        private void OnNoteWarningTypeChanged(string value)
        {
            if (int.TryParse(value, out var wType))
                _editorController.EditSelectedNotesWarningType(WarningTypeExt.FromInt32(wType));
            else
                _editorController.EditSelectedNotesWarningType(WarningType.Default);
        }

        private void OnNoteEventIdChanged(string value)
        {
            _editorController.EditSelectedNotesEventId(value);
        }

        private void OnNoteIsLinkChanged(bool value)
        {
            if (value)
                _editorController.LinkSelectedNotes();
            else
                _editorController.UnlinkSelectedNotes();
        }

        #endregion

        #region Notify
        [Obsolete]
        public void NotifyNoteSelectionChanged(ReadOnlySpan<NoteModel> selectedNotes)
        {
            _selectedNotesText.text = selectedNotes.Length.ToString();

            switch (selectedNotes.Length) {
                case 0: {
                    NotifyNoteTimeChanged(null);
                    NotifyNotePositionChanged(null);
                    NotifyNoteSizeChanged(null);
                    NotifyNoteShiftChanged(null);
                    NotifyNoteSpeedChanged(null);
                    NotifyNoteDurationChanged(null);
                    NotifyNoteVibrateChanged(null);
                    NotifyNoteIsSwipeChanged(null);
                    NotifyNoteWarningTypeChanged(null);
                    NotifyNoteEventIdChanged(null);
                    NotifyNoteIsLinkChanged(null);
                    NotifyNotePianoSoundsChanged(default);

                    SetControlsInteractable(false);
                    break;
                }

                case 1: {
                    var note = selectedNotes[0].Data;

                    NotifyNoteTimeChanged(note.Time);
                    NotifyNotePositionChanged(note.Position);
                    NotifyNoteSizeChanged(note.Size);
                    NotifyNoteShiftChanged(note.Shift);
                    NotifyNoteSpeedChanged(note.Speed);
                    NotifyNoteDurationChanged(note.Duration);
                    NotifyNoteVibrateChanged(note.Vibrate);
                    NotifyNoteIsSwipeChanged(note.IsSwipe);
                    NotifyNoteWarningTypeChanged(note.WarningType);
                    NotifyNoteEventIdChanged(note.EventId);
                    NotifyNoteIsLinkChanged(note.IsSlide);
                    NotifyNotePianoSoundsChanged(note.Sounds.AsSpan());

                    SetControlsInteractable(true);
                    break;
                }

                default: {
                    NotifyNoteTimeChanged(selectedNotes.IsSameForAll(n => n.Data.Time, out var time) ? time : null);
                    NotifyNotePositionChanged(
                        selectedNotes.IsSameForAll(n => n.Data.Position, out var pos) ? pos : null);
                    NotifyNoteSizeChanged(selectedNotes.IsSameForAll(n => n.Data.Size, out var size) ? size : null);
                    NotifyNoteShiftChanged(selectedNotes.IsSameForAll(n => n.Data.Shift, out var shift) ? shift : null);
                    NotifyNoteSpeedChanged(selectedNotes.IsSameForAll(n => n.Data.Speed, out var speed) ? speed : null);
                    NotifyNoteDurationChanged(selectedNotes.IsSameForAll(n => n.Data.Duration, out var duration)
                        ? duration
                        : null);
                    NotifyNoteVibrateChanged(selectedNotes.IsSameForAll(n => n.Data.Vibrate, out var vibrate)
                        ? vibrate
                        : null);
                    NotifyNoteIsSwipeChanged(selectedNotes.IsSameForAll(n => n.Data.IsSwipe, out var swipe)
                        ? swipe
                        : null);
                    NotifyNoteWarningTypeChanged(selectedNotes.IsSameForAll(n => n.Data.WarningType, out var wType)
                        ? wType
                        : null);
                    NotifyNoteEventIdChanged(
                        selectedNotes.IsSameForAll(n => n.Data.EventId, out var evId) ? evId : null);
                    NotifyNoteIsLinkChanged(selectedNotes.IsSameForAll(n => n.Data.IsSlide, out var slide)
                        ? slide
                        : null);
                    NotifyNotePianoSoundsChanged(selectedNotes.IsSameForAll(n => n.Data.Sounds, out var sounds,
                        PianoSoundListDataEqualityComparer.Instance)
                        ? sounds.AsSpan()
                        : null);

                    SetControlsInteractable(true);
                    break;
                }
            }

            void SetControlsInteractable(bool value)
            {
                if (_notePositionInputField.interactable == value)
                    return;

                _notePositionInputField.interactable = value;
                _noteTimeInputField.interactable = value;
                _noteSizeInputField.interactable = value;
                _noteShiftInputField.interactable = value;
                _noteSpeedInputField.interactable = value;
                _noteDurationInputField.interactable = value;
                _noteVibrateToggle.interactable = value;
                _noteSwipeToggle.interactable = value;
                _noteWarningTypeInputField.interactable = value;
                _noteEventIdInputField.interactable = value;
                _noteIsLinkToggle.interactable = value;
            }
        }
        [Obsolete]
        public void NotifyNoteTimeChanged(float? value)
            => NotifyFloatValueChanged(_noteTimeInputField, value);
        [Obsolete]
        public void NotifyNotePositionChanged(float? value)
            => NotifyFloatValueChanged(_notePositionInputField, value);
        [Obsolete]
        public void NotifyNoteSizeChanged(float? value)
            => NotifyFloatValueChanged(_noteSizeInputField, value);
        [Obsolete]
        public void NotifyNoteShiftChanged(float? value)
            => NotifyFloatValueChanged(_noteShiftInputField, value);
        [Obsolete]
        public void NotifyNoteSpeedChanged(float? value)
            => NotifyFloatValueChanged(_noteSpeedInputField, value);
        [Obsolete]
        public void NotifyNoteDurationChanged(float? value)
            => NotifyFloatValueChanged(_noteDurationInputField, value);
        [Obsolete]
        public void NotifyNoteVibrateChanged(bool? value)
            => NotifyBooleanValueChanged(_noteVibrateToggle, value);
        [Obsolete]
        public void NotifyNoteIsSwipeChanged(bool? value)
            => NotifyBooleanValueChanged(_noteSwipeToggle, value);
        [Obsolete]
        public void NotifyNoteWarningTypeChanged(WarningType? value)
            => _noteWarningTypeInputField.SetTextWithoutNotify(value?.ToInt32().ToString() ?? "-");
        [Obsolete]
        public void NotifyNoteEventIdChanged(string value)
            => _noteEventIdInputField.SetTextWithoutNotify(value ?? "-");
        [Obsolete]
        public void NotifyNoteIsLinkChanged(bool? value)
            => NotifyBooleanValueChanged(_noteIsLinkToggle, value);
        [Obsolete] // Not Impl new version yet
        public void NotifyNotePianoSoundsChanged(ReadOnlySpan<PianoSoundData> value)
        {
            _noteSoundsText.text = value.Length switch {
                0 => "-",
                1 => value[0].ToPitchDisplayString(),
                _ => value.Length.ToString(),
            };
        }

        private void NotifyFloatValueChanged(TMP_InputField inputField, float? value)
            => inputField.SetTextWithoutNotify(value?.ToString("F3") ?? "-");

        private void NotifyBooleanValueChanged(Toggle toggle, bool? value)
            => toggle.SetIsOnWithoutNotify(value ?? false);

        #endregion
    }
}