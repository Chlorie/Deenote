using UnityEngine;
using UnityEngine.EventSystems;

public class MoveAndResizeHandler : MonoBehaviour
{
    private enum Action
    {
        None,
        ResizeTopLeft,
        ResizeTop,
        ResizeTopRight,
        ResizeRight,
        ResizeBottomRight,
        ResizeBottom,
        ResizeBottomLeft,
        ResizeLeft,
        Move
    }
    private bool _dragging = false;
    private Rect _beginDragRect;
    private Vector2 _beginDragPoint;
    private Window _window;
    public UIParameters uiParameters;
    public void ChangeCursorSprite(MoveAndResizeDetector.ResizeAndMoveType position)
    {
        if (!_dragging)
            switch (position)
            {
                case MoveAndResizeDetector.ResizeAndMoveType.UpperLeft:
                case MoveAndResizeDetector.ResizeAndMoveType.LowerRight:
                    if (_window.horizontalResizable)
                    {
                        if (_window.verticalResizable)
                            Cursor.SetCursor(uiParameters.cursorDiagonal, uiParameters.cursorDiagonalHotspot, CursorMode.Auto);
                        else
                            Cursor.SetCursor(uiParameters.cursorHorizontal, uiParameters.cursorHorizontalHotspot, CursorMode.Auto);
                    }
                    else
                    {
                        if (_window.verticalResizable)
                            Cursor.SetCursor(uiParameters.cursorVertical, uiParameters.cursorVerticalHotspot, CursorMode.Auto);
                        else
                            Cursor.SetCursor(uiParameters.cursorDefault, uiParameters.cursorDefaultHotspot, CursorMode.Auto);
                    }
                    break;
                case MoveAndResizeDetector.ResizeAndMoveType.UpperRight:
                case MoveAndResizeDetector.ResizeAndMoveType.LowerLeft:
                    if (_window.horizontalResizable)
                    {
                        if (_window.verticalResizable)
                            Cursor.SetCursor(uiParameters.cursorAntiDiagonal, uiParameters.cursorAntiDiagonalHotspot, CursorMode.Auto);
                        else
                            Cursor.SetCursor(uiParameters.cursorHorizontal, uiParameters.cursorHorizontalHotspot, CursorMode.Auto);
                    }
                    else
                    {
                        if (_window.verticalResizable)
                            Cursor.SetCursor(uiParameters.cursorVertical, uiParameters.cursorVerticalHotspot, CursorMode.Auto);
                        else
                            Cursor.SetCursor(uiParameters.cursorDefault, uiParameters.cursorDefaultHotspot, CursorMode.Auto);
                    }
                    break;
                case MoveAndResizeDetector.ResizeAndMoveType.Up:
                case MoveAndResizeDetector.ResizeAndMoveType.Down:
                    if (_window.verticalResizable)
                        Cursor.SetCursor(uiParameters.cursorVertical, uiParameters.cursorVerticalHotspot, CursorMode.Auto);
                    else
                        Cursor.SetCursor(uiParameters.cursorDefault, uiParameters.cursorDefaultHotspot, CursorMode.Auto);
                    break;
                case MoveAndResizeDetector.ResizeAndMoveType.Left:
                case MoveAndResizeDetector.ResizeAndMoveType.Right:
                    if (_window.horizontalResizable)
                        Cursor.SetCursor(uiParameters.cursorHorizontal, uiParameters.cursorHorizontalHotspot, CursorMode.Auto);
                    else
                        Cursor.SetCursor(uiParameters.cursorDefault, uiParameters.cursorDefaultHotspot, CursorMode.Auto);
                    break;
                case MoveAndResizeDetector.ResizeAndMoveType.Move:
                    if (_window.movable)
                        Cursor.SetCursor(uiParameters.cursorMove, uiParameters.cursorMoveHotspot, CursorMode.Auto);
                    else
                        Cursor.SetCursor(uiParameters.cursorDefault, uiParameters.cursorDefaultHotspot, CursorMode.Auto);
                    break;
            }
    }
    public void ResetCursorSprite()
    {
        if (!_dragging) Cursor.SetCursor(uiParameters.cursorDefault, uiParameters.cursorDefaultHotspot, CursorMode.Auto);
    }
    public void BeginDrag(MoveAndResizeDetector.ResizeAndMoveType position, PointerEventData data)
    {
        ChangeCursorSprite(position);
        _dragging = true;
        _beginDragRect = _window.rect;
        _beginDragPoint = data.position;
    }
    public void EndDrag(MoveAndResizeDetector.ResizeAndMoveType position, PointerEventData data)
    {
        _dragging = false;
        ResetCursorSprite();
    }
    public void Dragging(MoveAndResizeDetector.ResizeAndMoveType position, PointerEventData data)
    {
        Rect rect = new Rect(_beginDragRect);
        Action action = Action.None;
        switch (position)
        {
            case MoveAndResizeDetector.ResizeAndMoveType.Move:
                if (_window.movable)
                    action = Action.Move;
                else
                    action = Action.None;
                break;
            case MoveAndResizeDetector.ResizeAndMoveType.UpperLeft:
                if (_window.horizontalResizable)
                {
                    if (_window.verticalResizable)
                        action = Action.ResizeTopLeft;
                    else
                        action = Action.ResizeLeft;
                }
                else
                {
                    if (_window.verticalResizable)
                        action = Action.ResizeTop;
                    else
                        action = Action.None;
                }
                break;
            case MoveAndResizeDetector.ResizeAndMoveType.UpperRight:
                if (_window.horizontalResizable)
                {
                    if (_window.verticalResizable)
                        action = Action.ResizeTopRight;
                    else
                        action = Action.ResizeRight;
                }
                else
                {
                    if (_window.verticalResizable)
                        action = Action.ResizeTop;
                    else
                        action = Action.None;
                }
                break;
            case MoveAndResizeDetector.ResizeAndMoveType.LowerRight:
                if (_window.horizontalResizable)
                {
                    if (_window.verticalResizable)
                        action = Action.ResizeBottomRight;
                    else
                        action = Action.ResizeRight;
                }
                else
                {
                    if (_window.verticalResizable)
                        action = Action.ResizeBottom;
                    else
                        action = Action.None;
                }
                break;
            case MoveAndResizeDetector.ResizeAndMoveType.LowerLeft:
                if (_window.horizontalResizable)
                {
                    if (_window.verticalResizable)
                        action = Action.ResizeBottomLeft;
                    else
                        action = Action.ResizeLeft;
                }
                else
                {
                    if (_window.verticalResizable)
                        action = Action.ResizeBottom;
                    else
                        action = Action.None;
                }
                break;
            case MoveAndResizeDetector.ResizeAndMoveType.Left:
                if (_window.horizontalResizable)
                    action = Action.ResizeLeft;
                else
                    action = Action.None;
                break;
            case MoveAndResizeDetector.ResizeAndMoveType.Right:
                if (_window.horizontalResizable)
                    action = Action.ResizeRight;
                else
                    action = Action.None;
                break;
            case MoveAndResizeDetector.ResizeAndMoveType.Up:
                if (_window.verticalResizable)
                    action = Action.ResizeTop;
                else
                    action = Action.None;
                break;
            case MoveAndResizeDetector.ResizeAndMoveType.Down:
                if (_window.verticalResizable)
                    action = Action.ResizeBottom;
                else
                    action = Action.None;
                break;
        }
        float x = data.position.x - _beginDragPoint.x, y = data.position.y - _beginDragPoint.y;
        Vector2 dx, dy;
        Vector2Int aspectRatio = _window.aspectRatio;
        int ratioCheck = 0;
        switch (action)
        {
            case Action.Move:
                rect.position += new Vector2(x, y);
                break;
            case Action.ResizeTopLeft:
                rect.position += new Vector2(x, 0.0f);
                rect.size += new Vector2(-x, y);
                ratioCheck = RatioCheck(rect.size, aspectRatio);
                if (ratioCheck > 0)
                    rect.size += AdjustY(rect.size, aspectRatio);
                else if (ratioCheck < 0)
                {
                    dx = AdjustX(rect.size, aspectRatio);
                    rect.position -= dx; rect.size += dx;
                }
                break;
            case Action.ResizeTopRight:
                rect.size += new Vector2(x, y);
                ratioCheck = RatioCheck(rect.size, aspectRatio);
                if (ratioCheck > 0)
                    rect.size += AdjustY(rect.size, aspectRatio);
                else if (ratioCheck < 0)
                    rect.size += AdjustX(rect.size, aspectRatio);
                break;
            case Action.ResizeBottomRight:
                rect.position += new Vector2(0.0f, y);
                rect.size += new Vector2(x, -y);
                ratioCheck = RatioCheck(rect.size, aspectRatio);
                if (ratioCheck > 0)
                {
                    dy = AdjustY(rect.size, aspectRatio);
                    rect.position -= dy; rect.size += dy;
                }
                else if (ratioCheck < 0)
                    rect.size += AdjustX(rect.size, aspectRatio);
                break;
            case Action.ResizeBottomLeft:
                rect.position += new Vector2(x, y);
                rect.size -= new Vector2(x, y);
                ratioCheck = RatioCheck(rect.size, aspectRatio);
                if (ratioCheck > 0)
                {
                    dy = AdjustY(rect.size, aspectRatio);
                    rect.position -= dy; rect.size += dy;
                }
                else if (ratioCheck < 0)
                {
                    dx = AdjustX(rect.size, aspectRatio);
                    rect.position -= dx; rect.size += dx;
                }
                break;
            case Action.ResizeTop:
                rect.size += new Vector2(0.0f, y);
                ratioCheck = RatioCheck(rect.size, aspectRatio);
                if (ratioCheck != 0) rect.size += AdjustX(rect.size, aspectRatio);
                break;
            case Action.ResizeBottom:
                rect.position += new Vector2(0.0f, y);
                rect.size -= new Vector2(0.0f, y);
                ratioCheck = RatioCheck(rect.size, aspectRatio);
                if (ratioCheck != 0) rect.size += AdjustX(rect.size, aspectRatio);
                break;
            case Action.ResizeLeft:
                rect.position += new Vector2(x, 0.0f);
                rect.size -= new Vector2(x, 0.0f);
                ratioCheck = RatioCheck(rect.size, aspectRatio);
                if (ratioCheck != 0) rect.size += AdjustY(rect.size, aspectRatio);
                break;
            case Action.ResizeRight:
                rect.size += new Vector2(x, 0.0f);
                ratioCheck = RatioCheck(rect.size, aspectRatio);
                if (ratioCheck != 0) rect.size += AdjustY(rect.size, aspectRatio);
                break;
        }
        _window.rect = rect;
    }
    // 0: Correct ratio, >0: x too large, <0: y too large
    private int RatioCheck(Vector2 size, Vector2Int aspectRatio)
    {
        if (aspectRatio.x == 0 || aspectRatio.y == 0) return 0;
        return (size.x * aspectRatio.y).CompareTo(size.y * aspectRatio.x);
    }
    private Vector2 AdjustX(Vector2 size, Vector2Int aspectRatio)
    {
        float x = size.y * aspectRatio.x / aspectRatio.y - size.x;
        return new Vector2(x, 0.0f);
    }
    private Vector2 AdjustY(Vector2 size, Vector2Int aspectRatio)
    {
        float y = size.x * aspectRatio.y / aspectRatio.x - size.y;
        return new Vector2(0.0f, y);
    }
    private void Start()
    {
        _window = GetComponent<Window>();
    }
}
