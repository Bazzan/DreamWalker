using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GP2_Team7.Objects.Scriptables
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Movement Data")]
    public class MovementData : ScriptableObject
    {
        [Tooltip("In m/s.")]
        public float moveSpeed = 8;

        public float acceleration = 1;

        [Tooltip("in m/s^2")]
        public float gravity = 9.81f;

        public float rotationValue = 21;
    }

}
