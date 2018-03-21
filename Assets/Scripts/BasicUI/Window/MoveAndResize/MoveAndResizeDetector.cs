using UnityEngine;
using UnityEngine.EventSystems;

public class MoveAndResizeDetector : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public MoveAndResizeHandler handler;
    public enum ResizeAndMoveType
    {
        UpperLeft,
        Up,
        UpperRight,
        Right,
        LowerRight,
        Down,
        LowerLeft,
        Left,
        Move
    }
    public ResizeAndMoveType type;
    public void OnPointerEnter(PointerEventData eventData)
    {
        handler.ChangeCursorSprite(type);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        handler.ResetCursorSprite();
    }
    public void OnDrag(PointerEventData eventData)
    {
        handler.Dragging(type, eventData);
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        handler.BeginDrag(type, eventData);
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        handler.EndDrag(type, eventData);
    }
}
