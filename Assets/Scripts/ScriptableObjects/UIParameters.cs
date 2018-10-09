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
    public Color toolbarSelectableEnabledTextColor;
    public Color toolbarSelectableDisabledTextColor;
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
    public Color transparent;
    public Color[] difficultyColors;
    // Stage
    public Sprite[] difficultySprites;
    public Sprite pianoNoteSprite;
    public Sprite slideNoteSprite;
    public Sprite otherNoteSprite;
    public float[] orthogonalDistancesPerSecond;
    // Perspective view
    public Vector2 perspectiveRenderTextureSize;
    public Sprite[] noteDisappearingSprites;
    public float disappearingSpriteTimePerFrame;
    public float circleMaxScale;
    public float circleIncreaseTime;
    public float waveWidth;
    public float waveMaxScale;
    public float waveExpandTime;
    public float waveShrinkTime;
    public Color slideNoteWaveColor;
    public Color glowColor;
    public float glowWidth;
    public float glowMaxScale;
    public float glowExpandTime;
    public float glowShrinkTime;
    public float noteAnimationLength;
    public float pianoNoteScale;
    public float slideNoteScale;
    public float otherNoteScale;
    public float disappearingSpriteScale;
    public float[] perspectiveDistancesPerSecond;
    public float perspectiveMaxDistance;
    public float perspectiveOpaqueDistance;
    public float perspectiveHorizontalScale;
    public float comboNoNumberLength;
    public float comboNumberBlackOutTime;
    public float comboShadowMaxTime;
    public float comboShadowMinAlpha;
    public float comboShockWaveMaxTime;
    public float comboStrikeShowTime;
    public float comboCharmingExpandTime;
    public float comboCharmingShrinkTime;
    public float judgeLineEffectShrinkTime;
    public float lightEffectAngularFrequency;
    public float lightMaskMinScale;
    public float lightMaskMaxScale;
    // Other UI
    public float minDeltaAlpha;
    // Control
    public float epsilonTime;
    public float slowScrollSpeed;
    public float fastScrollSpeed;
}
