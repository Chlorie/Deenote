// ReSharper disable FieldCanBeMadeReadOnly.Global
using UnityEngine;

public struct RectData
{
    public float minX;
    public float minY;
    public float sizeX;
    public float sizeY;
    public RectData(Rect rect)
    {
        Vector2 min = rect.min;
        Vector2 size = rect.size;
        minX = min.x; minY = min.y;
        sizeX = size.x; sizeY = size.y;
    }
    public Rect ToRect() => new Rect(new Vector2(minX, minY), new Vector2(sizeX, sizeY));
}
