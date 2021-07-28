using TMPro;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/UI Text Data")]
public class UITextData : ScriptableObject
{
    public TMP_FontAsset tmpFontAsset;
    public TMP_FontAsset tmpDyslexicFontAsset;
    
    [Tooltip("Font size of the subtitles when the screen resolution is at 1080p (will scale to fit approximately the same amount of space on the screen regardless of resolution")]
    public int fontSize;

    [Tooltip("Font size of the subtitles when the screen resolution is at 1080p and dyslexic font is selected (will scale to fit approximately the same amount of space on the screen regardless of resolution")]
    public float dyslexicFontSizeMultiplier;
    
    [Header("Only for subtitles")]
    
    public Font font;
    public Font dyslexicFont;

    [Tooltip("How many pixels above and below (combined) the text the background should extend")]
    public float verticalBackgroundPadding;

    [Tooltip("How many pixels to the right and left (combined) of the text the background should extend")]
    public float horizontalBackgroundPadding;

    public float lineOffset;

    public float offsetFromBottomOfScreen;

    public Color textColor;
    public Color backgroundColor;
}
