using GP2_Team7.Objects;
using UnityEngine;


namespace GP2_Team7.Objects
{
    [DefaultExecutionOrder(200)]
    public class PortalCameraMovement : MonoBehaviour
    {
        private Transform thisPortal;
        private Transform portalToTeleportTo;
        private PortalParent portalParent;
        private MeshRenderer thisMeshRenderer;
        private Renderer portalToTeleportToRenderer;
        private Camera thisCamera;
        private Camera playerCamera;

        private void Start()
        {
            portalParent = transform.parent.GetComponent<PortalParent>();
            portalToTeleportTo = portalParent.PortalToTeleportTo;
            thisPortal = transform.parent;
            portalToTeleportToRenderer = portalParent.PortalToTeleportToRenderQuad.GetComponent<Renderer>();
            thisMeshRenderer = transform.parent.GetComponentInChildren<PortalTextureManager>().GetComponent<MeshRenderer>();
            thisCamera = transform.parent.GetComponentInChildren<Camera>();
            playerCamera = portalParent.PlayerCamera.GetComponent<Camera>();
        }

        void Update()
        {
            if (!PortalUtility.VisibleFromCamera(portalToTeleportToRenderer, playerCamera))
            {
                thisCamera.enabled = false;
                return;
            }

            thisCamera.enabled = true;

            Matrix4x4 m = thisPortal.localToWorldMatrix * portalToTeleportTo.worldToLocalMatrix *
                          playerCamera.transform.localToWorldMatrix;
            transform.SetPositionAndRotation(m.GetColumn(3), m.rotation);

            thisMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            thisMeshRenderer.material.SetInt("displayMask", 0);

            thisCamera.Render();
            thisCamera.enabled = false;

            thisMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }
    }
}