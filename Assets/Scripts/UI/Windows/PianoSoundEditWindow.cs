using Cysharp.Threading.Tasks;
using Deenote.Edit;
using Deenote.GameStage;
using Deenote.Project.Comparers;
using Deenote.Project.Models;
using Deenote.Project.Models.Datas;
using Deenote.UI.Windows.Elements;
using Deenote.Utilities;
using Deenote.Utilities.Robustness;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.Windows
{
    [RequireComponent(typeof(Window))]
    public sealed class PianoSoundEditWindow : MonoBehaviour
    {
        [SerializeField] Window _window;
        public Window Window => _window;

        [Header("Notify")]
        [SerializeField] GameStageController _gameStageController;
        [SerializeField] EditorController _editorController;
        [SerializeField] PropertiesWindow _propertiesWindow;

        [Header("UI")]
        [SerializeField] Button _playButton;
        [SerializeField] Button _saveButton;
        [SerializeField] Button _revertButton;

        [Header("Prefabs")]
        [SerializeField] PianoSoundListItemController _soundItemPrefab;
        [SerializeField] Transform _soundItemParentTransform;
        private PooledObjectListView<PianoSoundListItemController> _soundItems;

        private List<NoteData> _editingNotes;

        private bool __isDirty;

        public bool IsDirty
        {
            get => __isDirty;
            set {
                __isDirty = value;
                _saveButton.interactable = __isDirty;
                _revertButton.interactable = __isDirty;
            }
        }

        private void Awake()
        {
            _playButton.onClick.AddListener(OnPlaySoundAsync);
            _saveButton.onClick.AddListener(SaveDataToNotes);
            _revertButton.onClick.AddListener(LoadSoundDatas);

            _soundItems = new PooledObjectListView<PianoSoundListItemController>(UnityUtils.CreateObjectPool(() =>
            {
                var item = Instantiate(_soundItemPrefab, _soundItemParentTransform);
                item.OnCreated(this);
                return item;
            }));
            _editingNotes = new();
        }

        public void AddSound(in PianoSoundValueData pianoSound)
        {
            if (MainSystem.Editor.SelectedNotes.IsEmpty)
                return;

            _soundItems.Add(out var soundItem);
            soundItem.Initialize(pianoSound);
            soundItem.transform.SetAsLastSibling();
            IsDirty = true;
        }

        public void RemoveSound(PianoSoundListItemController soundItem)
        {
            _soundItems.Remove(soundItem);
            IsDirty = true;
        }

        private void LoadSoundDatas()
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
            _soundItems.SetCount(sounds.Count);
            for (int i = 0; i < sounds.Count; i++) {
                _soundItems[i].Initialize(sounds[i].GetValues());
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

        private void SaveDataToNotes()
        {
            if (!IsDirty)
                return;

            var sounds = _soundItems.Count > 512
                ? new PianoSoundValueData[_soundItems.Count]
                : (stackalloc PianoSoundValueData[_soundItems.Count]);
            for (int i = 0; i < sounds.Length; i++) {
                sounds[i] = _soundItems[i].PianoSound.GetValues();
            }
            _editorController.EditSelectedNoteSounds(sounds);

            IsDirty = false;
        }

        #region Events

        private UniTaskVoid OnPlaySoundAsync()
        {
            foreach (var item in _soundItems) {
                MainSystem.PianoSoundManager.PlaySoundAsync(item.PianoSound, 1f, 1f).Forget();
            }
            return default;
        }

        #endregion

        #region Notify

        public void NotifySelectedNotesChanging(ReadOnlySpan<NoteModel> selectedNotes)
        {
            SaveDataToNotes();
        }

        public void NotifySelectedNotesChanged(ReadOnlySpan<NoteModel> selectedNotes)
        {
            if (!Window.IsActivated)
                return;

            _editingNotes.Clear();

            foreach (var note in selectedNotes) {
                _editingNotes.Add(note.Data);
            }
            LoadSoundDatas();
        }

        #endregion
    }
}