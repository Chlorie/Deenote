using Deenote.Project.Models.Datas;
using System;

namespace Deenote.Project.Models
{
    [Serializable]
    public sealed class NoteModel
    {
        public NoteData Data { get; }

        /// <summary>
        /// Is selected in editor
        /// </summary>
        public bool IsSelected;

        public bool IsCollided;

        public NoteModel(NoteData data)
        {
            Data = data;
        }
    }
}