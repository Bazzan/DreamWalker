using GP2_Team7.Managers;
using TMPro;
using UnityEngine;

public class DyslexicFontHandler : MonoBehaviour
{
    public UITextData textData;

    private TMP_Text[] _tmpText;
    private float[] _baseFontSizes;
        
    private void Awake()
    {
        _tmpText = GetComponentsInChildren<TMP_Text>(true);

        _baseFontSizes = new float[_tmpText.Length];
        for (int i = 0; i < _baseFontSizes.Length; i++)
        {
            _baseFontSizes[i] = _tmpText[i].fontSize;
        }

        SetFont();
    }

    private void OnEnable()
    {
        GameManager.onChangeSettings += SetFont;
    }

    private void OnDisable()
    {
        GameManager.onChangeSettings -= SetFont;
    }

    private void SetFont()
    {
        bool dyslexicFont = GameManager.UsingDyslexicFont;
        TMP_FontAsset font = dyslexicFont ? textData.tmpDyslexicFontAsset : textData.tmpFontAsset;
        
        for (int i = 0; i < _tmpText.Length; i++)
        {
            _tmpText[i].font = font;
            _tmpText[i].fontSize = _baseFontSizes[i] * (dyslexicFont ? textData.dyslexicFontSizeMultiplier : 1f);
        }
    }
}