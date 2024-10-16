using Deenote.Project.Models;
using Deenote.Project.Models.Datas;
using Deenote.UI.ComponentModel;
using Deenote.UI.Controls;
using Deenote.UI.Views.Elements;
using Deenote.Utilities;
using System;
using UnityEngine;

namespace Deenote.UI.Views
{
    public sealed class NoteInfoPanelView : MonoBehaviour, INotifyPropertyChange<NoteInfoPanelView, NoteInfoPanelView.NotifyProperty>
    {
        [SerializeField] KVInputProperty _positionProperty = default!;
        [SerializeField] KVInputProperty _timeProperty = default!;
        [SerializeField] KVInputProperty _sizeProperty = default!;
        [SerializeField] KVInputProperty _durationProperty = default!;
        [SerializeField] KVInputProperty _shiftProperty = default!;
        [SerializeField] KVInputProperty _speedProperty = default!;
        [SerializeField] KVSelectListProperty _kindProperty = default!;

        [SerializeField] KVBooleanProperty _vibrateProperty = default!;
        [SerializeField] KVInputProperty _warningTypeProperty = default!;
        [SerializeField] KVInputProperty _eventIdProperty = default!;

        // Events and notification are set in PianoSoundPropertyPanel.Start() 
        [SerializeField] PianoSoundPropertyPanel _pianoSoundEditPanelView = default!;

        private bool _isIneffectivePropertiesVisible;
        public bool IsIneffectivePropertiesVisible
        {
            get => _isIneffectivePropertiesVisible;
            set {
                if (_isIneffectivePropertiesVisible == value)
                    return;

                _isIneffectivePropertiesVisible = value;
                _shiftProperty.gameObject.SetActive(_isIneffectivePropertiesVisible);
                _vibrateProperty.gameObject.SetActive(_isIneffectivePropertiesVisible);
                _warningTypeProperty.gameObject.SetActive(_isIneffectivePropertiesVisible);
                _eventIdProperty.gameObject.SetActive(_isIneffectivePropertiesVisible);

                _propertyChangeNotifier.Invoke(this, NotifyProperty.IneffectivePropertiesVisiblility);
            }
        }

        private void Start()
        {
            const string MultipleValueText = "-";
            const int Kind_Click = 0, Kind_Slide = 1, Kind_Swipe = 2;

            // Properties
            {
                _positionProperty.InputField.OnEndEdit.AddListener(val =>
                {
                    if (float.TryParse(val, out var fval))
                        MainSystem.Editor.EditSelectedNotesPosition(fval);
                    else
                        NotifyFloatValueChanged(_positionProperty.InputField);
                });
                MainSystem.Editor.RegisterPropertyChangeNotification(
                    Edit.EditorController.NotifyProperty.NotePosition,
                    editor => NotifyMultiFloatValueChanged(_positionProperty.InputField, editor.SelectedNotes, n => n.Data.Position));

                _timeProperty.InputField.OnEndEdit.AddListener(val =>
                {
                    if (float.TryParse(val, out var fval))
                        MainSystem.Editor.EditSelectedNotesTime(fval);
                    else
                        NotifyFloatValueChanged(_timeProperty.InputField);
                });
                MainSystem.Editor.RegisterPropertyChangeNotification(
                    Edit.EditorController.NotifyProperty.NoteTime,
                    editor => NotifyMultiFloatValueChanged(_timeProperty.InputField, editor.SelectedNotes, n => n.Data.Time));

                _sizeProperty.InputField.OnEndEdit.AddListener(val =>
                {
                    if (float.TryParse(val, out var fval))
                        MainSystem.Editor.EditSelectedNotesSize(fval);
                    else
                        NotifyFloatValueChanged(_sizeProperty.InputField);
                });
                MainSystem.Editor.RegisterPropertyChangeNotification(
                    Edit.EditorController.NotifyProperty.NoteSize,
                    editor => NotifyMultiFloatValueChanged(_sizeProperty.InputField, editor.SelectedNotes, n => n.Data.Size));

                _durationProperty.InputField.OnEndEdit.AddListener(val =>
                {
                    if (float.TryParse(val, out var fval))
                        MainSystem.Editor.EditSelectedNotesDuration(fval);
                    else
                        NotifyFloatValueChanged(_durationProperty.InputField);
                });
                MainSystem.Editor.RegisterPropertyChangeNotification(
                    Edit.EditorController.NotifyProperty.NoteDuration,
                    editor => NotifyMultiFloatValueChanged(_durationProperty.InputField, editor.SelectedNotes, n => n.Data.Duration));

                _shiftProperty.InputField.OnEndEdit.AddListener(val =>
                {
                    if (float.TryParse(val, out var fval))
                        MainSystem.Editor.EditSelectedNotesShift(fval);
                    else
                        NotifyFloatValueChanged(_shiftProperty.InputField);
                });
                MainSystem.Editor.RegisterPropertyChangeNotification(
                    Edit.EditorController.NotifyProperty.NoteShift,
                    editor => NotifyMultiFloatValueChanged(_shiftProperty.InputField, editor.SelectedNotes, n => n.Data.Shift));

                _speedProperty.InputField.OnEndEdit.AddListener(val =>
                {
                    if (float.TryParse(val, out var fval))
                        MainSystem.Editor.EditSelectedNotesSpeed(fval);
                    else
                        NotifyFloatValueChanged(_speedProperty.InputField);
                });
                MainSystem.Editor.RegisterPropertyChangeNotification(
                    Edit.EditorController.NotifyProperty.NoteSpeed,
                    editor => NotifyMultiFloatValueChanged(_speedProperty.InputField, editor.SelectedNotes, n => n.Data.Speed));

                _kindProperty.ToggleList.OnSelectedIndexChanged.AddListener(val =>
                {
                    MainSystem.Editor.EditSelectedNotesKind(val switch {
                        Kind_Click => NoteData.NoteKind.Click,
                        Kind_Slide => NoteData.NoteKind.Slide,
                        Kind_Swipe => NoteData.NoteKind.Swipe,
                        _ => default!,
                    });
                });
                MainSystem.Editor.RegisterPropertyChangeNotification(
                    Edit.EditorController.NotifyProperty.NoteKind,
                    editor => _kindProperty.ToggleList.SetSelectedIndexWithoutNotify(editor.SelectedNotes.IsSameForAll(n => n.Data.Kind, out var kind) ? ToInt(kind) : -1));

                _vibrateProperty.CheckBox.OnValueChanged.AddListener(val =>
                {
                    if (val is not { } toggle)
                        return;
                    MainSystem.Editor.EditSelectedNotesVibrate(toggle);
                });
                MainSystem.Editor.RegisterPropertyChangeNotification(
                    Edit.EditorController.NotifyProperty.NoteVibrate,
                    editor => _vibrateProperty.CheckBox.SetValueWithoutNotify(editor.SelectedNotes.IsSameForAll(n => n.Data.Vibrate, out var flag) ? flag : null));

                _warningTypeProperty.InputField.OnEndEdit.AddListener(val =>
                {
                    if (int.TryParse(val, out var ival))
                        MainSystem.Editor.EditSelectedNotesWarningType(WarningTypeExt.FromInt32(ival));
                    else
                        _warningTypeProperty.InputField.SetValueWithoutNotify(MultipleValueText);
                });
                MainSystem.Editor.RegisterPropertyChangeNotification(
                    Edit.EditorController.NotifyProperty.NoteWarningType,
                    editor => _warningTypeProperty.InputField.SetValueWithoutNotify(editor.SelectedNotes.IsSameForAll(n => n.Data.WarningType, out var wt) ? wt.ToInt32().ToString() : MultipleValueText));

                _eventIdProperty.InputField.OnEndEdit.AddListener(val => MainSystem.Editor.EditSelectedNotesEventId(val));
                MainSystem.Editor.RegisterPropertyChangeNotification(
                    Edit.EditorController.NotifyProperty.NoteEventId,
                    editor => _eventIdProperty.InputField.SetValueWithoutNotify(editor.SelectedNotes.IsSameForAll(n => n.Data.EventId, out var evId) ? evId : ""));
            }

            // Universal
            {
                // Selected Notes
                MainSystem.Editor.RegisterPropertyChangeNotification(
                    Edit.EditorController.NotifyProperty.SelectedNotes,
                    editor =>
                    {
                        var selectedNotes = editor.SelectedNotes;
                        switch (selectedNotes.Length) {
                            case 0:
                                NotifyFloatValueChanged(_positionProperty.InputField);
                                NotifyFloatValueChanged(_timeProperty.InputField);
                                NotifyFloatValueChanged(_sizeProperty.InputField);
                                NotifyFloatValueChanged(_durationProperty.InputField);
                                NotifyFloatValueChanged(_shiftProperty.InputField);
                                NotifyFloatValueChanged(_speedProperty.InputField);
                                _kindProperty.ToggleList.SetSelectedIndexWithoutNotify(-1);

                                _vibrateProperty.CheckBox.SetValueWithoutNotify(null);
                                _warningTypeProperty.InputField.SetValueWithoutNotify(MultipleValueText);
                                _eventIdProperty.InputField.SetValueWithoutNotify(MultipleValueText);

                                SetInteractable(false);
                                break;
                            case 1:
                                var note = selectedNotes[0].Data;
                                NotifyFloatValueChanged(_positionProperty.InputField, note.Position);
                                NotifyFloatValueChanged(_timeProperty.InputField, note.Time);
                                NotifyFloatValueChanged(_sizeProperty.InputField, note.Size);
                                NotifyFloatValueChanged(_durationProperty.InputField, note.Duration);
                                NotifyFloatValueChanged(_shiftProperty.InputField, note.Shift);
                                NotifyFloatValueChanged(_speedProperty.InputField, note.Speed);
                                _kindProperty.ToggleList.SetSelectedIndexWithoutNotify(ToInt(note.Kind));

                                _vibrateProperty.CheckBox.SetValueWithoutNotify(note.Vibrate);
                                _warningTypeProperty.InputField.SetValueWithoutNotify(note.WarningType.ToInt32().ToString());
                                _eventIdProperty.InputField.SetValueWithoutNotify(note.EventId);

                                SetInteractable(true);
                                break;
                            default:
                                NotifyMultiFloatValueChanged(_positionProperty.InputField, selectedNotes, n => n.Data.Position);
                                NotifyMultiFloatValueChanged(_timeProperty.InputField, selectedNotes, n => n.Data.Time);
                                NotifyMultiFloatValueChanged(_sizeProperty.InputField, selectedNotes, n => n.Data.Size);
                                NotifyMultiFloatValueChanged(_durationProperty.InputField, selectedNotes, n => n.Data.Duration);
                                NotifyMultiFloatValueChanged(_shiftProperty.InputField, selectedNotes, n => n.Data.Shift);
                                NotifyMultiFloatValueChanged(_speedProperty.InputField, selectedNotes, n => n.Data.Speed);
                                _kindProperty.ToggleList.SetSelectedIndexWithoutNotify(selectedNotes.IsSameForAll(n => n.Data.Kind, out var kind) ? ToInt(kind) : -1);

                                _vibrateProperty.CheckBox.SetValueWithoutNotify(selectedNotes.IsSameForAll(n => n.Data.Vibrate, out var vibrate) ? vibrate : null);
                                _warningTypeProperty.InputField.SetValueWithoutNotify(selectedNotes.IsSameForAll(n => n.Data.WarningType, out var wt) ? wt.ToInt32().ToString() : MultipleValueText);
                                _eventIdProperty.InputField.SetValueWithoutNotify(editor.SelectedNotes.IsSameForAll(n => n.Data.EventId, out var evId) ? evId : "");

                                SetInteractable(true);
                                break;
                        }
                    });
            }

            void SetInteractable(bool interactable)
            {
                if (_positionProperty.InputField.IsInteractable == interactable)
                    return;

                _positionProperty.InputField.IsInteractable = interactable;
                _timeProperty.InputField.IsInteractable = interactable;
                _sizeProperty.InputField.IsInteractable = interactable;
                _durationProperty.InputField.IsInteractable = interactable;
                _shiftProperty.InputField.IsInteractable = interactable;
                _speedProperty.InputField.IsInteractable = interactable;
                _kindProperty.ToggleList.IsInteractable = interactable;
                _vibrateProperty.CheckBox.UnityToggle.interactable = interactable;
                _warningTypeProperty.InputField.IsInteractable = interactable;
                _eventIdProperty.InputField.IsInteractable = interactable;
            }

            static void NotifyMultiFloatValueChanged(InputField inputField, ReadOnlySpan<NoteModel> models, Func<NoteModel, float> selector)
                => inputField.SetValueWithoutNotify(models.IsSameForAll(selector, out var val) ? val.ToString("F3") : MultipleValueText);

            static void NotifyFloatValueChanged(InputField inputField, float? value = null)
                => inputField.SetValueWithoutNotify(value?.ToString("F3") ?? MultipleValueText);

            static int ToInt(NoteData.NoteKind kind) => kind switch {
                NoteData.NoteKind.Click => Kind_Click,
                NoteData.NoteKind.Slide => Kind_Slide,
                NoteData.NoteKind.Swipe => Kind_Swipe,
                _ => -1,
            };
        }

        private PropertyChangeNotifier<NoteInfoPanelView, NotifyProperty> _propertyChangeNotifier;

        public void RegisterPropertyChangeNotification(NotifyProperty flag, Action<NoteInfoPanelView> action)
            => _propertyChangeNotifier.AddListener(flag, action);

        public enum NotifyProperty
        {
            IneffectivePropertiesVisiblility
        }
    }
}