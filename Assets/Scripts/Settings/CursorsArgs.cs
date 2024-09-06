using UnityEngine;

namespace Deenote.Settings
{
    [CreateAssetMenu(fileName = nameof(CursorsArgs), menuName = $"{nameof(Deenote)}/{nameof(CursorsArgs)}")]
    public sealed class CursorsArgs : ScriptableObject
    {
        public Texture2D DefaultCursor;
        public Vector2 DefaultCursorHotspot;
        public Texture2D MoveCursor;
        public Vector2 MoveCursorHotspot;
        public Texture2D HorizontalCursor;
        public Vector2 HorizontalCursorHotspot;
        public Texture2D VerticalCursor;
        public Vector2 VerticalCursorHotspot;
        public Texture2D DiagonalCursor;
        public Vector2 DiagonalCursorHotspot;
        public Texture2D AntiDiagonalCursor;
        public Vector2 AntiDiagonalCursorHotspot;
    }
}