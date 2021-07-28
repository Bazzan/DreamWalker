using UnityEngine;
using UnityEngine.Assertions;

public class UIRaycaster : MonoBehaviour
{
    private Camera _camera;

    private void Awake()
    {
        _camera = Camera.main;
        Assert.IsNotNull(_camera, "No game object is tagged with MainCamera in scene");
    }

    private void Update()
    {
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            // todo finish
        }
    }
}
