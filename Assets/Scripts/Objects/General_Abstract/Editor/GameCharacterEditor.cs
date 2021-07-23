using UnityEngine;
using UnityEditor;

namespace GP2_Team7.Objects
{
    [CustomEditor(typeof(GameCharacter))]
    [CanEditMultipleObjects]
    public class GameCharacterEditor : Editor
    {
        private GameCharacter _gc;

        private bool _isReady = false;

        private const string _cullString = "GP2_Team7.Objects.Avatars.";

        private void OnEnable()
        {
            _isReady = false;

            _gc = target as GameCharacter;

            _isReady = true;
        }

        public override void OnInspectorGUI()
        {
            FontStyle originalFontStyle = EditorStyles.label.fontStyle;
            EditorStyles.label.fontStyle = FontStyle.Bold;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Current Avatar: " + _gc.Avatar.GetType().ToString().Substring(_cullString.Length));
            EditorGUILayout.EndHorizontal();

            EditorStyles.label.fontStyle = originalFontStyle;

            base.OnInspectorGUI();
        }
    }

}
