using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GP2_Team7.Objects
{
    using Cameras;

    public class JPortalBlit : MonoBehaviour
    {
        private static List<Light> _currentDirectionalLights = new List<Light>();

        [SerializeField, Range(0, 10)]
        private int _frameDelay = 0;

        private Camera _camera;

        Color pcl;
        public Color cl;

        RenderTexture _renderTex;

        public Texture t;

        Vector3[] positions = new Vector3[16];
        Quaternion[] rotations = new Quaternion[16];

        // Start is called before the first frame update
        void Awake()
        {
            _camera = GetComponent<Camera>();

            _camera.enabled = false;

            JPortal.onAnyPortalExistenceStatus += OnPortalExistence;

            gameObject.SetActive(JPortal.JPortalsExist);
        }

        private void OnPostRender()
        {
            SetRenderTexture();

            _camera.Render();

            GL.PushMatrix();
            GL.LoadOrtho();

            RenderTexture.active = _renderTex;
            Graphics.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), t);

            GL.PopMatrix();
        }

        private void SetRenderTexture()
        {
            if (_renderTex == null || _renderTex.width != Screen.width || _renderTex.height != Screen.height)
            {
                if (_renderTex != null)
                {
                    _renderTex.Release();
                }

                _renderTex = new RenderTexture(Screen.width, Screen.height, 0);

                _camera.targetTexture = _renderTex;
            }
        }

        private void OnPortalExistence(bool status)
        {
            gameObject.SetActive(status);
        }
    }
}

