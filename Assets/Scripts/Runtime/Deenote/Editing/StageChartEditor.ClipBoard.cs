#nullable enable

using Deenote.Entities;
using Deenote.Entities.Models;
using Deenote.Library.Collections;
using System;
using UnityEngine.Pool;

namespace Deenote.Editing
{
    partial class StageChartEditor
    {
        private PooledObjectListView<NoteModel> _clipBoard;

        public ReadOnlySpan<NoteModel> ClipBoardNotes => _clipBoard.AsSpan();

        public NoteCoord ClipBoardBaseCoord => _clipBoard.Count > 0 ? _clipBoard[0].PositionCoord : new(0f, 0f);

        private void Awake_ClipBoard()
        {
            _clipBoard = new(new ObjectPool<NoteModel>(() => new NoteModel()));
        }

        public void CopySelectedNotes()
        {
            if (ClipBoardNotes.IsEmpty)
                return;

            _placer.CancelPlaceNote();
            _clipBoard.Clear();

            var notes = _selector.SelectedNotes;

            foreach (var note in notes) {
                _clipBoard.Add(out var clipNote);
                note.CloneDataTo(clipNote);
            }
            NoteModel.CloneLinkDatas(notes, _clipBoard.AsSpan());
        }

        public void CutSelectedNotes()
        {
            CopySelectedNotes();
            RemoveNotes(_selector.SelectedNotes);
        }

        public void PasteNotes()
        {
            if (ClipBoardNotes.IsEmpty)
                return;

            _placer.PreparePasteClipBoard();
        }
    }
}