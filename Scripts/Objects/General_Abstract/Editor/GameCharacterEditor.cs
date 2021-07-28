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

        private void OnEnable()
        {
            _isReady = false;

            _gc = target as GameCharacter;

            _isReady = true;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }

}
