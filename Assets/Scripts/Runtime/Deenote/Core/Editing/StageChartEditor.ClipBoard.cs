#nullable enable

namespace Deenote.Core.Editing
{
    partial class StageChartEditor
    {
        private NotesClipBoard _noteClipBoard_bf = default!;
        public NotesClipBoard ClipBoard => _noteClipBoard_bf;

        private void Awake_ClipBoard()
        {
            _noteClipBoard_bf = new();
        }

        public void CopySelectedNotes()
        {
            if (_selector.SelectedNotes.IsEmpty)
                return;

            _placer.CancelPlaceNote();
            ClipBoard.SetNotes(_selector.SelectedNotes);
        }

        public void CutSelectedNotes()
        {
            CopySelectedNotes();
            RemoveNotes(_selector.SelectedNotes);
        }

        public void PasteNotes()
        {
            if (ClipBoard.Notes.IsEmpty)
                return;

            _placer.PreparePasteClipBoard();
        }
    }
}