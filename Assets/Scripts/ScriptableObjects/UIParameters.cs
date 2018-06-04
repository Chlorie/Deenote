using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/UI Parameters")]
public class UIParameters : ScriptableObject
{
    // Toolbar settings
    public float toolbarMainButtonSideSpace;
    public float toolbarItemMiddleSpace;
    public Color toolbarSelectableDefaultColor;
    public Color toolbarSelectableHighlightedColor;
    public Color toolbarSelectableSelectedColor;
    // Status bar settings
    public Color statusBarDefaultColor;
    public Color statusBarErrorColor;
    // Window settings
    public float minTagWidth;
    public float tagLeftSpace;
    public float tagRightSpace;
    // Cursors
    public Texture2D cursorDefault;
    public Vector2 cursorDefaultHotspot;
    public Texture2D cursorMove;
    public Vector2 cursorMoveHotspot;
    public Texture2D cursorHorizontal;
    public Vector2 cursorHorizontalHotspot;
    public Texture2D cursorVertical;
    public Vector2 cursorVerticalHotspot;
    public Texture2D cursorDiagonal;
    public Vector2 cursorDiagonalHotspot;
    public Texture2D cursorAntiDiagonal;
    public Vector2 cursorAntiDiagonalHotspot;
    // Colors
    public Color[] difficultyColors;
    // Stage
    public Sprite[] difficultySprites;
}
