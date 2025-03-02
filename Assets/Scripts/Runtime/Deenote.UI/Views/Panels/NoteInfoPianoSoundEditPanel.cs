#nullable enable

using Deenote.Core.Editing;
using Deenote.Entities.Models;
using Deenote.Library;
using Deenote.Library.Collections;
using Deenote.Library.Components;
using Deenote.UI.Views.Elements;
using Deenote.UIFramework.Controls;
using System;
using System.Collections.Generic;
using Trarizon.Library.Collections;
using UnityEngine;

namespace Deenote.UI.Views.Panels
{
    public sealed class NoteInfoPianoSoundEditPanel : MonoBehaviour
    {
        [SerializeField] Button _playSoundButton = default!;
        [SerializeField] Button _revertButton = default!;
        [SerializeField] NoteInfoPianoKeysPanel _pianoKeysPanel = default!;
        [SerializeField] RectTransform _soundListContentTransform = default!;

        [Header("Prefabs")]
        [SerializeField] NoteInfoPianoSoundEditListItem _soundItemPrefab = default!;

        private PooledObjectListView<NoteInfoPianoSoundEditListItem> _soundItems;

        private List<NoteModel> _editingNotes = new();

        private bool _isDirty_bf;
        private bool _isActive_bf;
        private bool IsDirty => _isDirty_bf;

        public bool IsActive
        {
            get => _isActive_bf;
            set {
                if (Utils.SetField(ref _isActive_bf, value)) {
                    this.gameObject.SetActive(value);
                    if (value) {
                        ResetEditingNotesAndLoad(MainSystem.StageChartEditor.Selector.SelectedNotes);
                    }
                    else {
                        SaveDataToEditingNotes();
                    }
                }
            }
        }

        public event Action<bool>? IsDirtyChanged;

        internal void SetDirty(bool isDirty = true)
        {
            if (Utils.SetField(ref _isDirty_bf, isDirty)) {
                _revertButton.IsInteractable = isDirty;
                IsDirtyChanged?.Invoke(isDirty);
            }
        }

        private void Awake()
        {
            _soundItems = new(UnityUtils.CreateObjectPool(_soundItemPrefab, _soundListContentTransform,
                item => item.OnInstantiate(this), defaultCapacity: 0));

            _playSoundButton.Clicked += () =>
            {
                foreach (var item in _soundItems) {
                    MainWindow.PianoSoundPlayer.PlaySound(item.Sound);
                }
            };
            _revertButton.Clicked += LoadEditingNotesSoundDatas;
            _pianoKeysPanel.KeyClicked += pitch =>
            {
                _soundItems.Add(out var item);
                item.Initialize(new PianoSoundValueModel(0f, 0f, pitch, 0));
                item.transform.SetAsLastSibling();
                SetDirty();
            };

            MainSystem.StageChartEditor.RegisterNotificationAndInvoke(
                StageChartEditor.NotificationFlag.NoteSounds,
                editor => ResetEditingNotesAndLoad(editor.Selector.SelectedNotes));
            MainSystem.StageChartEditor.Selector.RegisterNotificationAndInvoke(
                StageNoteSelector.NotificationFlag.SelectedNotesChanging,
                selector => SaveDataToEditingNotes());
            MainSystem.StageChartEditor.Selector.RegisterNotificationAndInvoke(
                StageNoteSelector.NotificationFlag.SelectedNotesChanged,
                selector => ResetEditingNotesAndLoad(selector.SelectedNotes));
        }

        internal void RemoveSoundItem(NoteInfoPianoSoundEditListItem item)
        {
            _soundItems.Remove(item);
            SetDirty();
        }

        private void SaveDataToEditingNotes()
        {
            if (!IsDirty)
                return;

            var sounds = _soundItems.Count > 512
                ? new PianoSoundValueModel[_soundItems.Count]
                : (stackalloc PianoSoundValueModel[_soundItems.Count]);

            for (int i = 0; i < sounds.Length; i++) {
                sounds[i] = _soundItems[i].Sound;
            }
            MainSystem.StageChartEditor.EditSelectedNoteSounds(sounds);
            SetDirty(false);
        }

        private void ResetEditingNotesAndLoad(ReadOnlySpan<NoteModel> notes)
        {
            if (IsActive) {
                _editingNotes.Clear();
                foreach (var note in notes) {
                    _editingNotes.Add(note);
                }
                LoadEditingNotesSoundDatas();
            }
        }

        private void LoadEditingNotesSoundDatas()
        {
            if (_editingNotes.Count == 0) {
                _soundItems.Clear();
                return;
            }

            if (!HasSameSounds(_editingNotes.AsSpan())) {
                _soundItems.Clear();
                return;
            }

            var sounds = _editingNotes[0].Sounds;
            using (var resetter = _soundItems.Resetting(sounds.Length)) {
                foreach (var s in sounds) {
                    resetter.Add(out var item);
                    item.Initialize(s);
                }
            }

            SetDirty(false);
        }

        internal static bool HasSameSounds(ReadOnlySpan<NoteModel> notes)
        {
            var first = notes[0].Sounds;

            for (int i = 1; i < notes.Length; i++) {
                var sounds = notes[i].Sounds;
                if (first.Length != sounds.Length)
                    return false;

                for (int j = 0; j < sounds.Length; j++) {
                    if (first[j] != sounds[j])
                        return false;
                }
            }
            return true;
        }
    }
}