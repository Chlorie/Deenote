public class NoteSelect
{
    public EditorController_Legacy editor;
    public Note note;
    public bool prevSelected = false;
    public bool selected = false;
    public void UpdateState()
    {
        int prevState = prevSelected != selected ? 1 : 0;
        int curState;
        float minX = editor.dragStartPoint.x < editor.dragEndPoint.x ? editor.dragStartPoint.x : editor.dragEndPoint.x;
        float maxX = editor.dragStartPoint.x + editor.dragEndPoint.x - minX;
        float minT = editor.dragStartPoint.y < editor.dragEndPoint.y ? editor.dragStartPoint.y : editor.dragEndPoint.y;
        float maxT = editor.dragStartPoint.y + editor.dragEndPoint.y - minT;
        selected = (note.position < maxX + Parameters.xAxisRange * note.size &&
            note.position > minX - Parameters.xAxisRange * note.size &&
            note.time < maxT + Parameters.tAxisRange &&
            note.time > minT - Parameters.tAxisRange);
        curState = prevSelected != selected ? 1 : 0;
        if (curState - prevState != 0) editor.UpdateSelectedAmount(curState - prevState, false);
    }
    public void EndDrag()
    {
        prevSelected = prevSelected != selected;
        selected = false;
    }
}
