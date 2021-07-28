using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GP2_Team7.Objects.Cameras
{
    /// <summary>
    /// Interface that tells CameraControllers where to
    /// place the camera when focusing on this object. 
    /// Not a mandatory interface, but the camera will
    /// usually strictly focus on the transform origin
    /// otherwise.
    /// </summary>
    public interface IFocable
    {
        Vector3 FocusPoint { get; }
    }
}
