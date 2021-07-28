using System.Collections;
using GP2_Team7.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GP2_Team7.Objects
{
    [DefaultExecutionOrder(200)]
    public class PortalTeleporter : MonoBehaviour
    {
        private PortalParent portalParent;
        private Transform ColliderToTeleportTo;
        private bool isLoadingNewScene;
        private string nameOfScene;
        private Transform playerTransform;

        private bool isOverlapping = false;
        private float timer;
        private float cd;

        private void Start()
        {
            portalParent = transform.parent.GetComponent<PortalParent>();
            ColliderToTeleportTo = portalParent.PortalToTeleportToCollider;
            playerTransform = GameManager.Player.transform;
            isLoadingNewScene = portalParent.IsLoadingNewScene;
            nameOfScene = portalParent.NameOfTheSceneToLoad;
        }

        private void LateUpdate()
        {
            if (!CanPort()) return;

            if (isOverlapping)
            {
                TeleportPlayer();
                isOverlapping = false;
            }
        }

        private bool CanPort()
        {
            if (timer < cd)
            {
                timer += Time.deltaTime;
                return false;
            }
            return true;
        }

        private void TeleportPlayer()
        {
            Matrix4x4 m = ColliderToTeleportTo.localToWorldMatrix * transform.worldToLocalMatrix *
                          playerTransform.localToWorldMatrix;
            playerTransform.SetPositionAndRotation(m.GetColumn(3), m.rotation);
            
            IsPorting(2f);
            ColliderToTeleportTo.GetComponent<PortalTeleporter>().IsPorting(2f);
        }

        public void IsPorting(float time)
        {
            timer = Time.time;
            cd = Time.time + time;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (isLoadingNewScene)
                {
                    StartCoroutine(ChangeScene());
                    return;
                }
                isOverlapping = true;
                Debug.Log("enter");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
                isOverlapping = false;
        }

        private IEnumerator ChangeScene()
        {
            SceneManager.LoadSceneAsync(nameOfScene);
            yield return null;
        }
    }
}

// Vector3 portalToPlayer = playerTransform.position - transform.position;
// float dotProduct = Vector3.Dot(transform.forward, playerTransform.forward);

// Debug.Log(dotProduct);


// If this is true: The player has moved across the portal
// if (dotProduct > 0f)
// {

// float rotationDiff = -Quaternion.Angle(transform.rotation, ColliderToTeleportTo.rotation);
// Debug.Log(rotationDiff);
//
// Teleport him!
// if ( 0 > Vector3.Dot(transform.forward, ColliderToTeleportTo.forward))
// {
//     rotationDiff += 180;
//     playerTransform.Rotate(Vector3.up, rotationDiff);
// }
// playerTransform.Rotate(Vector3.up, Quaternion.Angle(transform.rotation, ColliderToTeleportTo.rotation));

// Vector3 positionOffset = Quaternion.Euler(0f, rotationDiff, 0f) * portalToPlayer;
// playerTransform.position = ColliderToTeleportTo.position + positionOffset + playerTransform.transform.forward;
// }


// private IEnumerator ChangeScene()
// {
//     Scene currentScene = SceneManager.GetActiveScene();
//     Scene sceneToActivate = SceneManager.GetSceneByName(nameOfScene);
//     AsyncOperation loadNewScene = SceneManager.LoadSceneAsync(nameOfScene, LoadSceneMode.Additive);
//     loadNewScene.allowSceneActivation = false;
//     while (!loadNewScene.isDone)
//     {
//         if (loadNewScene.progress <= 0.89f)
//         {
//             player.position = portalParent.SpawnPoint;
//             player.rotation = Quaternion.Euler(portalParent.SpawnRotation);
//
//
//
//             SceneManager.MoveGameObjectToScene(player.gameObject, SceneManager.GetSceneByName(nameOfScene));
//             SceneManager.MoveGameObjectToScene(CameraController.Main.gameObject,
//                 SceneManager.GetSceneByName(nameOfScene));
//             SceneManager.MoveGameObjectToScene(GameManager.Instance.gameObject,
//                 SceneManager.GetSceneByName(nameOfScene));
//             // SceneManager.SetActiveScene(sceneToActivate);
//             loadNewScene.allowSceneActivation = true;
//             // SceneManager.
//             SceneManager.UnloadSceneAsync(currentScene);
//         }
//
//         Debug.Log(loadNewScene.progress);
//         yield return null;
//     }
// }