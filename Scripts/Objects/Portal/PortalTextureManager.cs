using UnityEngine;

namespace GP2_Team7.Objects
{ 
    [DefaultExecutionOrder(+200)]
    public class PortalTextureManager : MonoBehaviour
    {
        private Camera portalToTeleportToCamera;
        private Material material;

        private void Start()
        {
            portalToTeleportToCamera = transform.parent.GetComponent<PortalParent>().PortalToTeleportToCamera;
            material = GetComponent<MeshRenderer>().material;

            if (portalToTeleportToCamera.targetTexture != null)
                portalToTeleportToCamera.targetTexture.Release();

            portalToTeleportToCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, 0);
            material.mainTexture = portalToTeleportToCamera.targetTexture;
        }
    }
}