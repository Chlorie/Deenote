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
            if (Selector.SelectedNotes.IsEmpty)
                return;

            Placer.CancelPlaceNote();
            ClipBoard.SetNotes(Selector.SelectedNotes);
        }

        public void CutSelectedNotes()
        {
            CopySelectedNotes();
            RemoveNotes(Selector.SelectedNotes);
        }

        public void PasteNotes()
        {
            if (ClipBoard.Notes.IsEmpty)
                return;

            Placer.PreparePasteClipBoard();
        }
    }
}