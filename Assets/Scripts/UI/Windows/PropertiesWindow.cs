using Deenote.Project.Models;
using Deenote.Project.Models.Datas;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.Windows
{
    [RequireComponent(typeof(Window))]
    public sealed class PropertiesWindow : MonoBehaviour
    {
        [SerializeField] Window _window;

        [Header("UI")]
        [Header("Project Info")]
        [SerializeField] Button _projectAudioButton;
        [SerializeField] TMP_InputField _projectNameInputField;
        [SerializeField] TMP_InputField _projectComposerInputField;
        [SerializeField] TMP_InputField _projectChartDesignerInputField;
        [SerializeField] TMP_Dropdown _projectLoadedChartDropdown;

        [Header("Chart Info")]
        [SerializeField] TMP_InputField _chartNameInputField;
        [SerializeField] TMP_Dropdown _chartDifficultyDropdown;
        [SerializeField] TMP_InputField _chartLevelInputField;
        [SerializeField] TMP_InputField _chartSpeedInputField;
        [SerializeField] TMP_InputField _chartRemapVMinInputField;
        [SerializeField] TMP_InputField _chartRemapVMaxInputField;
        [SerializeField] TMP_Text _selectedNotesText;

        [Header("Note Info")]
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

        #region Notify

        public void NotifyNoteSelectionChanged(List<NoteModel> selectedNotes)
        {
            _selectedNotesText.text = selectedNotes.Count.ToString();

            switch (selectedNotes.Count) {
                case 0: {
                    _notePositionInputField.SetTextWithoutNotify("-");
                    _noteTimeInputField.SetTextWithoutNotify("-");
                    _noteSizeInputField.SetTextWithoutNotify("-");
                    _noteShiftInputField.SetTextWithoutNotify("-");
                    _noteSpeedInputField.SetTextWithoutNotify("-");
                    _noteDurationInputField.SetTextWithoutNotify("-");
                    _noteVibrateToggle.SetIsOnWithoutNotify(false);
                    _noteSwipeToggle.SetIsOnWithoutNotify(false);
                    _noteWarningTypeInputField.SetTextWithoutNotify("-");
                    _noteEventIdInputField.SetTextWithoutNotify("-");
                    _noteIsLinkToggle.SetIsOnWithoutNotify(false);
                    _noteSoundsText.SetText("-");
                    SetControlsInteractable(false);
                    break;
                }

                case 1: {
                    var note = selectedNotes[0].Data;

                    _notePositionInputField.SetTextWithoutNotify(note.Position.ToString("F3"));
                    _noteTimeInputField.SetTextWithoutNotify(note.Time.ToString("F3"));
                    _noteSizeInputField.SetTextWithoutNotify(note.Size.ToString("F3"));
                    _noteShiftInputField.SetTextWithoutNotify(note.Shift.ToString("F3"));
                    _noteSpeedInputField.SetTextWithoutNotify(note.Speed.ToString("F3"));
                    _noteDurationInputField.SetTextWithoutNotify(note.Duration.ToString("F3"));
                    _noteVibrateToggle.SetIsOnWithoutNotify(note.Vibrate);
                    _noteSwipeToggle.SetIsOnWithoutNotify(note.IsSwipe);
                    _noteWarningTypeInputField.SetTextWithoutNotify(note.WarningType.ToString());
                    _noteEventIdInputField.SetTextWithoutNotify(note.EventId);
                    _noteIsLinkToggle.SetIsOnWithoutNotify(note.IsSlide);
                    _noteSoundsText.SetText(note.Sounds.Count switch {
                        0 => "-",
                        1 => note.Sounds[0].ToPitchDisplayString(),
                        _ => note.Sounds.Count.ToString(),
                    });

                    SetControlsInteractable(true);
                    break;
                }

                default: {
                    _notePositionInputField.SetTextWithoutNotify(GetDisplayString(selectedNotes, n => n.Position));
                    _noteTimeInputField.SetTextWithoutNotify(GetDisplayString(selectedNotes, n => n.Time));
                    _noteSizeInputField.SetTextWithoutNotify(GetDisplayString(selectedNotes, n => n.Size));
                    _noteShiftInputField.SetTextWithoutNotify(GetDisplayString(selectedNotes, n => n.Shift));
                    _noteSpeedInputField.SetTextWithoutNotify(GetDisplayString(selectedNotes, n => n.Speed));
                    _noteDurationInputField.SetTextWithoutNotify(GetDisplayString(selectedNotes, n => n.Duration));
                    _noteVibrateToggle.SetIsOnWithoutNotify(SameForAll(selectedNotes, n => n.Vibrate) ? selectedNotes[0].Data.Vibrate : false);
                    _noteSwipeToggle.SetIsOnWithoutNotify(SameForAll(selectedNotes, n => n.IsSwipe) ? selectedNotes[0].Data.IsSwipe : false);
                    _noteWarningTypeInputField.SetTextWithoutNotify(SameForAll(selectedNotes, n => n.WarningType) ? selectedNotes[0].Data.WarningType.ToInt32().ToString() : "-");
                    _noteEventIdInputField.SetTextWithoutNotify(SameForAll(selectedNotes, n => n.EventId) ? selectedNotes[0].Data.EventId : "-");
                    _noteIsLinkToggle.SetIsOnWithoutNotify(SameForAll(selectedNotes, n => n.IsSlide) ? selectedNotes[0].Data.IsSlide : false);
                    _noteSoundsText.text = "-";

                    SetControlsInteractable(true);
                    break;

                    static string GetDisplayString(List<NoteModel> notes, Func<NoteData, float> propertyGetter)
                    {
                        var compare = propertyGetter(notes[0].Data);
                        foreach (var note in notes) {
                            if (propertyGetter(note.Data) != compare)
                                return "-";
                        }
                        return compare.ToString("F3");
                    }

                    static bool SameForAll<T>(List<NoteModel> notes, Func<NoteData, T> propertyGetter)
                    {
                        var compare = propertyGetter(notes[0].Data);
                        foreach (var note in notes) {
                            if (!EqualityComparer<T>.Default.Equals(propertyGetter(note.Data), compare))
                                return false;
                        }
                        return true;
                    }
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
                _noteSoundsButton.interactable = value;
            }
        }

        #endregion
    }
}
