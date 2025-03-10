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

        private readonly List<NoteModel> _editingNotes = new();

        private bool _isDirty_bf;
        private bool _isPanelActive_bf;
        private bool IsDirty => _isDirty_bf;

        public bool IsPanelActive
        {
            get => _isPanelActive_bf;
            set {
                if (Utils.SetField(ref _isPanelActive_bf, value)) {
                    this.gameObject.SetActive(value);
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
        }

        private void Start()
        {
            _playSoundButton.Clicked += () =>
            {
                foreach (var item in _soundItems) {
                    MainWindow.PianoSoundPlayer.PlaySound(item.Sound);
                }
            };
            _revertButton.Clicked += LoadEditingNotesSoundDatas;
            _pianoKeysPanel.KeyClicked += pitch =>
            {
                if (_editingNotes.Count == 0)
                    return;
                _soundItems.Add(out var item);
                item.Initialize(new PianoSoundValueModel(0f, 0f, pitch, 0));
                item.transform.SetAsLastSibling();
                SetDirty();
            };
        }

        private void OnEnable()
        {
            MainSystem.StageChartEditor.RegisterNotificationAndInvoke(
                StageChartEditor.NotificationFlag.NoteSounds, _OnSelectedNotesSoundsChanged);
            MainSystem.StageChartEditor.Selector.SelectedNotesChanging += _OnSelectedNotesChanging;
            MainSystem.StageChartEditor.Selector.SelectedNotesChanged += _OnSelectedNotesChanged;

            _OnSelectedNotesChanged(MainSystem.StageChartEditor.Selector);
        }

        private void OnDisable()
        {
            MainSystem.StageChartEditor.UnregisterNotification(
                StageChartEditor.NotificationFlag.NoteSounds, _OnSelectedNotesSoundsChanged);
            MainSystem.StageChartEditor.Selector.SelectedNotesChanging -= _OnSelectedNotesChanging;
            MainSystem.StageChartEditor.Selector.SelectedNotesChanged -= _OnSelectedNotesChanged;

            SaveSoundDatas();
        }

        #region Event Handlers

        private void _OnSelectedNotesChanging(StageNoteSelector selector)
        {
            SaveSoundDatas();
        }

        private void _OnSelectedNotesSoundsChanged(StageChartEditor editor)
        {
            ResetEditingNotesAndLoad(editor.Selector.SelectedNotes);
        }

        private void _OnSelectedNotesChanged(StageNoteSelector selector)
        {
            ResetEditingNotesAndLoad(selector.SelectedNotes);
            if (selector.SelectedNotes.IsEmpty) {
                _playSoundButton.IsInteractable = false;
                _revertButton.IsInteractable = false;
            }
            else {
                _playSoundButton.IsInteractable = true;
                _revertButton.IsInteractable = true;
            }
        }

        #endregion

        internal void RemoveSoundItem(NoteInfoPianoSoundEditListItem item)
        {
            _soundItems.Remove(item);
            SetDirty();
        }

        private void SaveSoundDatas()
        {
            if (!IsDirty)
                return;

            if (_editingNotes.Count == 0)
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
            if (IsPanelActive) {
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

            if (!NoteModel.HasSameSounds(_editingNotes.AsSpan())) {
                _soundItems.Clear();
                return;
            }

            var sounds = _editingNotes[0].Sounds.AsSpan();
            using (var resetter = _soundItems.Resetting(sounds.Length)) {
                foreach (var s in sounds) {
                    resetter.Add(out var item);
                    item.Initialize(s);
                }
            }

            SetDirty(false);
        }
    }
}