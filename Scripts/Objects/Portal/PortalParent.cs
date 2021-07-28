using UnityEngine;

namespace GP2_Team7.Objects
{
    using Cameras;
    using Managers;

//    [DefaultExecutionOrder(+200)]
    public class PortalParent : MonoBehaviour
    {
        [Header("To load a new scene")] [Tooltip("True to load new scene")] 
        [SerializeField] public bool IsLoadingNewScene;
        [SerializeField] public string NameOfTheSceneToLoad;
        
        [Header("Transforms for portal to work")] [SerializeField]
        public Transform PortalToTeleportTo;

        [SerializeField] public Transform PortalToTeleportToCameraTransform;
        [SerializeField] public Transform PortalToTeleportToCollider;
        [SerializeField] public Transform PortalToTeleportToRenderQuad;
        [HideInInspector] public Camera PortalToTeleportToCamera;
        [HideInInspector] public Transform player;
        [HideInInspector] public Transform PlayerCamera;

        private void Start()
        {
            player = GameManager.Player.transform;
            PlayerCamera = CameraController.MainCamera.transform;
            PortalToTeleportToCamera = PortalToTeleportToCameraTransform.GetComponent<Camera>();
        }

        public void TurnPortalOn()
        {
            transform.GetChild(0).gameObject.SetActive(true);
            transform.GetChild(1).gameObject.SetActive(true);
            transform.GetChild(2).gameObject.SetActive(true);

            // PortalToTeleportToCamera.gameObject.SetActive(true);
            // PortalToTeleportToCollider.gameObject.SetActive(true);
            // PortalToTeleportToRenderQuad.gameObject.SetActive(true);
        }

        public void TurnPortalOff()
        {
            transform.GetChild(0).gameObject.SetActive(false);
            transform.GetChild(1).gameObject.SetActive(false);
            transform.GetChild(2).gameObject.SetActive(false);

            
            
            // PortalToTeleportToCamera.gameObject.SetActive(false);
            // PortalToTeleportToCollider.gameObject.SetActive(false);
            // PortalToTeleportToRenderQuad.gameObject.SetActive(false);
        }
        
    }
}