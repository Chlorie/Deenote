#nullable enable

using UnityEngine;

namespace Deenote.Settings
{
    [CreateAssetMenu(fileName = nameof(CursorsArgs), menuName = $"{nameof(Deenote)}/{nameof(CursorsArgs)}")]
    public sealed class CursorsArgs : ScriptableObject
    {
        public Texture2D DefaultCursor = default!;
        public Vector2 DefaultCursorHotspot;
        public Texture2D MoveCursor = default!;
        public Vector2 MoveCursorHotspot;
        public Texture2D HorizontalCursor = default!;
        public Vector2 HorizontalCursorHotspot;
        public Texture2D VerticalCursor = default!;
        public Vector2 VerticalCursorHotspot;
        public Texture2D DiagonalCursor = default!;
        public Vector2 DiagonalCursorHotspot;
        public Texture2D AntiDiagonalCursor = default!;
        public Vector2 AntiDiagonalCursorHotspot;
    }
}