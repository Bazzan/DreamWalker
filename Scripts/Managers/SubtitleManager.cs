using System;
using UnityEngine;

namespace GP2_Team7.Managers
{
    [DefaultExecutionOrder(-2)]
    public class SubtitleManager : MonoBehaviour
    {
        public UITextData uiTextFormatting;
        public static SubtitleManager Instance { get; private set; }

        private bool _renderSubtitles = false;
        private string[] _currentSubtitle;
        private Color _currentSubtitleColor;
        private Texture2D _texture2D;
        private float _timeOfStartDraw;
        private float _secondsToDisplay;

        private static readonly Vector2 referenceResolution = new Vector2(1920f, 1080f);

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else if(Instance != this)
                Destroy(this);
        }

        private void Start()
        {
            _texture2D = new Texture2D(1, 1);
            _texture2D.SetPixel(0,0, uiTextFormatting.backgroundColor);
            _texture2D.wrapMode = TextureWrapMode.Repeat;
            _texture2D.Apply();
        }

        private void OnGUI()
        {
            if (!_renderSubtitles || PauseManager.IsPaused)
                return;

            if ((Time.time - _timeOfStartDraw) > _secondsToDisplay)
            {
                _renderSubtitles = false;
                return;
            }

            for (int i = 0; i < _currentSubtitle.Length; i++)
            {
                string sub = _currentSubtitle[_currentSubtitle.Length - 1 - i];
                Rect rect = GetRect(i, sub, _currentSubtitleColor, out GUIStyle style);
                
                GUI.DrawTexture(rect, _texture2D, ScaleMode.StretchToFill, true);
                GUI.Label(rect, sub, style);
            }
        }

        private Rect GetRect(int index, string currentSubtitle, Color subtitleColor, out GUIStyle style)
        {
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
                
            float scaleMultiplier = screenWidth > screenHeight ? screenHeight / referenceResolution.x : screenWidth / referenceResolution.y;
            float offset = uiTextFormatting.offsetFromBottomOfScreen * scaleMultiplier;
            float y = screenHeight;
            
            int fontSize = (int) Math.Round(uiTextFormatting.fontSize * (GameManager.UsingDyslexicFont ? uiTextFormatting.dyslexicFontSizeMultiplier : 1f) * scaleMultiplier);

            style = new GUIStyle {alignment = TextAnchor.MiddleCenter, fontSize = fontSize, font = GameManager.UsingDyslexicFont ? uiTextFormatting.dyslexicFont : uiTextFormatting.font, normal = new GUIStyleState {textColor = subtitleColor}};
            Vector2 size = style.CalcSize(new GUIContent(currentSubtitle));
            size.x += uiTextFormatting.horizontalBackgroundPadding;
            size.y += uiTextFormatting.verticalBackgroundPadding;

            float x = screenWidth * 0.5f - size.x * 0.5f;
            y -= size.y * (index + 1) + offset + uiTextFormatting.lineOffset * scaleMultiplier * index;

            return new Rect
            (
                x,
                y, 
                size.x, 
                size.y
            );
        }

        public static void DrawSubtitles(string subtitle, float lengthToDisplay, Color subtitleColor)
        {
            Instance._renderSubtitles = false;
            Instance._timeOfStartDraw = Time.time;
            Instance._secondsToDisplay = lengthToDisplay;
            Instance._currentSubtitleColor = subtitleColor;

            Instance._currentSubtitle = subtitle.Split('\n');
            
            Instance._renderSubtitles = true;
        }
    }
}