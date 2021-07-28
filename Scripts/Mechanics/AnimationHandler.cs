using UnityEngine;
using GP2_Team7.Objects;

public class AnimationHandler : MonoBehaviour
{
    public static AnimationHandler AnimationHandlerStatic;
    public InteractableToggle interactable;
    public Animator AnimatorThingRow1;
    public GameObject[] enableTheseWhenPlayed;

    void Start()
    {
        AnimationHandlerStatic = this;
        interactable.eventToTrigger = Animate;
    }

    public void Animate(int test)
    {
        AnimatorThingRow1.SetTrigger("Trigger");
        AnimatorThingRow1.SetBool("IsDone", true);

        foreach (GameObject obj in enableTheseWhenPlayed)
        {
            obj.SetActive(true);
        }
    }
}
