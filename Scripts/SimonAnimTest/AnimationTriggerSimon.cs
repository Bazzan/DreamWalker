using UnityEngine;

public class AnimationTriggerSimon : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("PlayAnim");
        AnimationHandler.AnimationHandlerStatic.Animate(0);
    }
}
