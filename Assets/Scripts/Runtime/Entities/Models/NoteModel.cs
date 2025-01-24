#nullable enable

using System;
using UnityEngine;

namespace Deenote.Entities.Models
{
    [Serializable]
    public sealed partial class NoteModel : IStageNoteNode
    {
        [field: SerializeField]
        private bool _isSelected;
        /// <summary>
        /// If editor is selecting notes now, this field indicates whether the note is in selection range 
        /// </summary>
        private bool _isInSelectionRange;

        /// <summary>
        /// Is selected in editor
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected ^ _isInSelectionRange;
            set {
                _isSelected = value;
                _isInSelectionRange = false;
            }
        }

        private int _collisionCount_bf;
        public int CollisionCount
        {
            get => _collisionCount_bf;
            set {
                _collisionCount_bf = value;
                Debug.Assert(_collisionCount_bf >= 0);
            }
        }

        public bool IsCollided => CollisionCount > 0;

        public NoteModel()
        {
            _sounds = new();
        }

        public NoteModel Clone(bool cloneSounds = true)
        {
            var note = new NoteModel();
            CloneDataTo(note, cloneSounds);
            return note;
        }

        /// <summary>
        /// Clone datas into another note, you may require <see cref="NoteModel.CloneLinkDatas(ReadOnlySpan{NoteModel}, ReadOnlySpan{NoteModel})"/>
        /// if you are cloning a note collection
        /// </summary>
        public void CloneDataTo(NoteModel note, bool cloneSounds = true)
        {
            note.Position = Position;
            note.Time = Time;
            note.Size = Size;
            note.Kind = Kind;
            note.Shift = Shift;
            note.Speed = Speed;
            note.Duration = Duration;
            note.WarningType = WarningType;
            note.EventId = EventId;
#pragma warning disable CS0618
            note._serializeType = _serializeType;
            note.Vibrate = Vibrate;
#pragma warning restore CS0618

            if (cloneSounds && _sounds.Count > 0) {
                note._sounds.Capacity = _sounds.Count;
                foreach (var s in Sounds) {
                    note._sounds.Add(s);
                }
            }
        }

        public void SetIsInSelectionRange(bool value)
        {
            _isInSelectionRange = value;
        }

        public void ApplySelection()
        {
            _isSelected = IsSelected;
            _isInSelectionRange = false;
        }
    }
}