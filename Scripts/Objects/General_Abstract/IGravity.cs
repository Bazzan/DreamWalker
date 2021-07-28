using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GP2_Team7
{
    /// <summary>
    /// Interface that indicates that this 
    /// object is affected by gravity.
    /// </summary>
    public interface IGravity
    {
        Vector3 Position { get; }

        Quaternion GravityRotation { get; set; }
        /// <summary>
        /// The object rotation in its gravity space.
        /// </summary>
        Quaternion LocalGravityRotation { get; set; }
    }
}
