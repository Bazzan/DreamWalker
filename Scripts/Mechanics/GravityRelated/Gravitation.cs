using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GP2_Team7
{
    using Objects.Cameras;
    /// <summary>
    /// The Gravitation class contains functions that are useful
    /// for objects that are affected by gravity.
    /// </summary>
    public static class Gravitation
    {
        /// <summary>
        /// Returns a gravity rotation from a normal.
        /// </summary>
        public static void SetGravityFromNormal(Quaternion currentGravityRotation, Vector3 normal, out Quaternion newGravityRotation)
        {
            newGravityRotation =
                Quaternion.FromToRotation(currentGravityRotation * Vector3.up, normal) * currentGravityRotation;
        }

        /// <summary>
        /// Returns VectorToCameraRotation in gravity space.
        /// </summary>
        public static Vector3 VectorToCameraRotation(Quaternion gravityRotation, Vector3 vector)
        {
            return /*Quaternion.Inverse(gravityRotation) * */CameraController.VectorToCameraRotation(vector);
        }
    }
}
