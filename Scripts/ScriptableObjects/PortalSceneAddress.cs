using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GP2_Team7.Objects.Scriptables
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Portal Scene Address")]
    public class PortalSceneAddress : ScriptableObject
    {
        [SerializeField, Tooltip("The name of the scene in which the target portal exists.")]
        private string _sceneName;

        [SerializeField, Tooltip("The name of the GameObject containing the target JPortal.")]
        private string _portalGameObjectName;

        public string SceneName => _sceneName;
        public string PortalGameObjectName => _portalGameObjectName;
    }

}