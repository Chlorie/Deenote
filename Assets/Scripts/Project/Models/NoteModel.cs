using Deenote.Project.Models.Datas;
using System;
using UnityEngine;

namespace Deenote.Project.Models
{
    [Serializable]
    public sealed class NoteModel
    {
        [field: SerializeField]
        private NoteData __data;
        public NoteData Data => __data;

        [field: SerializeField]
        private bool _isSelected;
        /// <summary>
        /// If editor is selecting notes now, this field indicates whether the note is in selection range 
        /// </summary>
        private bool _isInSelection;

        /// <summary>
        /// Is selected in editor
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected ^ _isInSelection;
            set {
                _isSelected = value;
                _isInSelection = false;
            }
        }

        public bool IsCollided;

        public NoteModel(NoteData data)
        {
            __data = data;
        }

        public void SetIsInSelection(bool value)
        {
            _isInSelection = value;
        }
    }
}