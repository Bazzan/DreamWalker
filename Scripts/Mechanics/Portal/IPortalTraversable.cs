using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GP2_Team7.Objects
{
    public interface IPortalTraversable
    {
        //bool TransPortalMode { get; }

        GameObject PortalModel { get; }

        GameObject PortalClone { get; set; }

        JPortal InPortal { get; }

        JPortal OutPortal { get; }

        void OnPortalTraverse(JPortal inPortal, JPortal outPortal, Vector3 newPosition, Quaternion newRotation);
    }
}
