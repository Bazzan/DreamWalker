using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GP2_Team7.Objects
{
    using EditorScripts;

    [CustomEditor(typeof(VoidLight))]
    [CanEditMultipleObjects]
    public class VoidLightEditor : EditorLib
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (Button("Set Waypoint 1 to Object Position"))
            {
                foreach (VoidLight voidLight in targets)
                {
                    voidLight._waypoint1 = voidLight.transform.position;
                }
            }

            if (Button("Set Waypoint 2 to Object Position"))
            {
                foreach (VoidLight voidLight in targets)
                {
                    voidLight._waypoint2 = voidLight.transform.position;
                }
            }

            Space();

            if (Button("Set Object Position to Waypoint 1"))
            {
                foreach (VoidLight voidLight in targets)
                {
                    voidLight.transform.position = voidLight._waypoint1;
                }
            }

            if (Button("Set Object Position to Waypoint 2"))
            {
                foreach (VoidLight voidLight in targets)
                {
                    voidLight.transform.position = voidLight._waypoint2;
                }
            }
        }
    }
}
