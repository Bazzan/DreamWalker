using GP2_Team7.Managers;
using UnityEngine;

namespace GP2_Team7.Objects
{
    using Cameras;

    [DefaultExecutionOrder(200)]
    public class PortalCameraMovementTest : MonoBehaviour
    {
        // [Header("PortalToTeleportTo")] 
        private Transform PortalToTeleportTo;
        private Transform ThisPortal;
        private Transform PlayerCameraTransform;
        private Camera PlayerCamera;
        private Transform portalToTeleportToRenderQuad;
        private Renderer portalToTeleportToRenderQuadRenderer;
        private Camera ThisCamera;
        private Transform Player;
        private void Start()
        {
            ThisCamera = GetComponent<Camera>();
            portalToTeleportToRenderQuad = transform.parent.GetComponent<PortalParent>().PortalToTeleportToRenderQuad;
            portalToTeleportToRenderQuadRenderer = portalToTeleportToRenderQuad.GetComponent<Renderer>();
            PortalToTeleportTo = transform.parent.GetComponent<PortalParent>().PortalToTeleportToCollider;
            ThisPortal = transform.parent;
            PlayerCameraTransform = CameraController.MainCamera.transform;
            PlayerCamera = PlayerCameraTransform.GetComponent<Camera>();
            Player = GameManager.Player.transform;
        }

        private void Update()
        {
            if (PortalUtility.VisibleFromCamera(portalToTeleportToRenderQuadRenderer, PlayerCamera))
            {
                ThisCamera.enabled = true;
                Vector3 playerOffsetFromPortal = PlayerCameraTransform.position - PortalToTeleportTo.position;
                transform.localPosition = ThisPortal.position +  new Vector3(playerOffsetFromPortal.x, playerOffsetFromPortal.y,  playerOffsetFromPortal.z);

                float angularDifferenceBetweenPortalRotations =
                    Quaternion.Angle(ThisPortal.rotation, PortalToTeleportTo.rotation);
                Quaternion portalRotationalDifference =
                    Quaternion.AngleAxis(angularDifferenceBetweenPortalRotations, Vector3.up);
                Vector3 newCameraDirection = portalRotationalDifference * Player.forward;

                transform.rotation = Quaternion.LookRotation(newCameraDirection, Vector3.up);
                
            }
            else
            {
                ThisCamera.enabled = false;
            }
        }
    }
}