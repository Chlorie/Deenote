#nullable enable

using Deenote.Entities;
using Deenote.Entities.Models;
using Deenote.Library.Collections;
using System;
using UnityEngine.Pool;

namespace Deenote.Core.Editing
{
    public sealed class NotesClipBoard
    {
        private PooledObjectListView<NoteModel> _notes;

        public ReadOnlySpan<NoteModel> Notes => _notes.AsSpan();

        public NoteCoord BaseCoord => _notes.Count > 0 ? _notes[0].PositionCoord : new(0f, 0f);

        public NotesClipBoard()
        {
            _notes = new(new ObjectPool<NoteModel>(() => new NoteModel()));
        }

        public void SetNotes(ReadOnlySpan<NoteModel> notes)
        {
            using (var resetter = _notes.Resetting(notes.Length)) {
                foreach (var note in notes) {
                    _notes.Add(out var cnote);
                    note.CloneDataTo(cnote);
                }
            }
            NoteModel.CloneLinkDatas(notes, _notes.AsSpan());
        }
    }
}