using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GP2_Team7.Objects.Scriptables
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Camera Controller Values")]
    public class CameraControllerValues : ScriptableObject
    {
        public float moveDamp = 0.3f;

        public float mouseSensitivity = 1;

        public Vector2 pitchConstraints = new Vector2(-60, 85);

        public Vector3 cameraOffset = new Vector3(2.5f, 2.2f, 4);

        public float cameraColliderRadius = 0.3f;

        public float rotationSmoothValue = 25f;
    }
}
