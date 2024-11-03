#nullable enable

using Deenote.Project.Comparers;
using Deenote.Project.Models;
using Deenote.Project.Models.Datas;
using Deenote.UI.ComponentModel;
using Deenote.UI.Controls;
using Deenote.Utilities;
using Deenote.Utilities.Robustness;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Deenote.UI.Views.Elements
{
    public sealed class PianoSoundPropertyPanel : MonoBehaviour
    {
        private const int WhiteKeyCount = 75;

        [SerializeField] KVButtonProperty _soundsProperty;
        [SerializeField] GameObject _editPanelGameObject;
        [SerializeField] Button _playButton;
        [SerializeField] Button _revertButton;
        [SerializeField] UnityEngine.UI.ScrollRect _scrollRect;
        [SerializeField] PianoOctaveView[] _octaves;
        [SerializeField] Button[] _quickScrollButtons;

        [SerializeField] Transform _soundPropertyItemParentTransform;

        [Header("Prefabs")]
        [SerializeField] PianoSoundPropertyItem _soundPropertyItemPrefab;

        private PooledObjectListView<PianoSoundPropertyItem> _soundItems;

        private readonly List<NoteData> _editingNotes = new();

        private bool _isDirty;
        internal bool IsDirty
        {
            get => _isDirty;
            set {
                if (_isDirty == value)
                    return;
                _isDirty = value;
                _revertButton.IsInteractable = _isDirty;
                _playButton.IsInteractable = _soundItems.Count > 0;
                _soundsProperty.Button.Image.sprite = _isDirty
                    ? MainSystem.Args.KnownIconsArgs.NoteInfoSoundsAcceptSprite
                    : MainSystem.Args.KnownIconsArgs.NoteInfoSoundsCollapseSprite;
            }
        }

        private void Awake()
        {
            Debug.Assert(_octaves.Length == 8 - (-2) + 1, "Piano has invalid octave count.");
            Debug.Assert(_quickScrollButtons.Length == _octaves.Length);
            foreach (var octave in _octaves) {
                octave.Parent = this;
            }
            for (int i = 0; i < _quickScrollButtons.Length; i++) {
                var cpitch = i * PianoOctaveView.OctaveKeyCount;
                _quickScrollButtons[i].OnClick.AddListener(() => ScrollTo(cpitch));
            }
            _soundItems = new PooledObjectListView<PianoSoundPropertyItem>(
                UnityUtils.CreateObjectPool(() =>
                {
                    var item = Instantiate(_soundPropertyItemPrefab, _soundPropertyItemParentTransform);
                    item.Parent = this;
                    return item;
                }, defaultCapacity: 0));
        }

        private void Start()
        {
            _soundsProperty.Button.OnClick.AddListener(() =>
            {
                if (_editPanelGameObject.activeSelf) {
                    SaveDataToEditingNotes();
                    _soundsProperty.Button.Image.sprite = MainSystem.Args.KnownIconsArgs.NoteInfoSoundsEditSprite;
                    _editPanelGameObject.SetActive(false);
                }
                else {
                    ResetEditingNotesAndLoad(MainSystem.Editor.SelectedNotes);
                    _editPanelGameObject.SetActive(true);
                    _soundsProperty.Button.Image.sprite = MainSystem.Args.KnownIconsArgs.NoteInfoSoundsCollapseSprite;
                }
            });
            _soundsProperty.Button.OnClick.Invoke();
            _playButton.OnClick.AddListener(() =>
            {
                foreach (var item in _soundItems) {
                    MainSystem.PianoSoundManager.PlaySoundAsync(item.SoundData, 1f, 1f).Forget();
                }
            });
            _revertButton.OnClick.AddListener(LoadEditingNotesSoundDatas);

            MainSystem.Editor.RegisterPropertyChangeNotificationAndInvoke(
                Edit.EditorController.NotifyProperty.NoteSounds,
                editor => NotifySoundsChanged(editor.SelectedNotes));

            MainSystem.Editor.RegisterPropertyChangeNotificationAndInvoke(
                Edit.EditorController.NotifyProperty.SelectedNotes_Changing,
                editor => SaveDataToEditingNotes());
            MainSystem.Editor.RegisterPropertyChangeNotificationAndInvoke(
                Edit.EditorController.NotifyProperty.SelectedNotes,
                editor => NotifySoundsChanged(editor.SelectedNotes));

            void NotifySoundsChanged(ReadOnlySpan<NoteModel> selectedNotes)
            {
                switch (selectedNotes.Length) {
                    case 0:
                        _soundsProperty.Button.Text.SetRawText("-");
                        break;
                    case 1: {
                        var sounds = selectedNotes[0].Data.Sounds;
                        _soundsProperty.Button.Text.SetRawText(sounds.Count switch {
                            0 => "-",
                            1 => sounds[0].ToPitchDisplayString(),
                            _ => sounds.Count.ToString(),
                        });
                        break;
                    }
                    default: {
                        if (SameForAll(selectedNotes, out var sounds)) {
                            _soundsProperty.Button.Text.SetRawText(sounds.Count switch {
                                0 => "-",
                                1 => sounds[0].ToPitchDisplayString(),
                                _ => sounds.Count.ToString(),
                            });
                        }
                        else {
                            _soundsProperty.Button.Text.SetRawText("-");
                        }
                        break;
                    }
                }

                // If edit panel is not expanded, do not update sound list.
                if (_editPanelGameObject.activeSelf) {
                    ResetEditingNotesAndLoad(selectedNotes);
                }
            }

            void ResetEditingNotesAndLoad(ReadOnlySpan<NoteModel> notes)
            {
                _editingNotes.Clear();
                foreach (var note in notes) {
                    _editingNotes.Add(note.Data);
                }
                LoadEditingNotesSoundDatas();
            }

            static bool SameForAll(ReadOnlySpan<NoteModel> notes, [NotNullWhen(true)] out List<PianoSoundData>? sounds)
            {
                sounds = notes[0].Data.Sounds;

                for (int i = 1; i < notes.Length; i++) {
                    var other = notes[i].Data.Sounds;
                    if (sounds.Count != other.Count) {
                        sounds = null;
                        return false;
                    }

                    for (int j = 0; j < other.Count; j++) {
                        if (!PianoSoundDataEqualityComparer.Instance.Equals(sounds[j], other[j])) {
                            sounds = null;
                            return false;
                        }
                    }
                }

                return true;
            }
        }

        internal void AddSoundData(in PianoSoundValueData sound)
        {
            _soundItems.Add(out var item);
            item.Initialize(sound);
            item.transform.SetAsLastSibling();
            IsDirty = true;
        }

        internal void RemoveSoundItem(PianoSoundPropertyItem item)
        {
            _soundItems.Remove(item);
            IsDirty = true;
        }

        private void LoadEditingNotesSoundDatas()
        {
            if (_editingNotes.Count == 0) {
                _soundItems.Clear();
                return;
            }

            if (!SameForAll(_editingNotes)) {
                _soundItems.Clear();
                return;
            }

            var sounds = _editingNotes[0].Sounds;
            using (var resettingSounds = _soundItems.Resetting()) {
                foreach (var sd in sounds) {
                    resettingSounds.Add(out var item);
                    item.Initialize(sd.GetValues());
                }
            }

            IsDirty = false;

            static bool SameForAll(List<NoteData> notes)
            {
                var first = notes[0].Sounds;

                for (int i = 1; i < notes.Count; i++) {
                    var sounds = notes[i].Sounds;
                    if (first.Count != sounds.Count)
                        return false;

                    for (int j = 0; j < sounds.Count; j++) {
                        if (!PianoSoundDataEqualityComparer.Instance.Equals(first[j], sounds[j]))
                            return false;
                    }
                }

                return true;
            }
        }

        private void SaveDataToEditingNotes()
        {
            if (!IsDirty)
                return;

            var sounds = _soundItems.Count > 512
                ? new PianoSoundValueData[_soundItems.Count]
                : (stackalloc PianoSoundValueData[_soundItems.Count]);
            for (int i = 0; i < sounds.Length; i++) {
                sounds[i] = _soundItems[i].SoundData;
            }
            MainSystem.Editor.EditSelectedNoteSounds(sounds);
            IsDirty = false;
        }

        internal void ScrollTo(int pitch)
        {
            int octaveNumber = pitch / PianoOctaveView.OctaveKeyCount;

            float contentWidth = _scrollRect.content.rect.width;
            float viewWidth = _scrollRect.viewport.rect.width;

            float destX = (octaveNumber * PianoOctaveView.OctaveWhiteKeyCount / (float)WhiteKeyCount) * contentWidth;
            float moveRange = contentWidth - viewWidth;

            _scrollRect.horizontalNormalizedPosition = Mathf.Clamp01(destX / moveRange);
        }
    }
}