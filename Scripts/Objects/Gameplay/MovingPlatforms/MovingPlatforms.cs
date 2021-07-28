using GP2_Team7.Objects;
using UnityEngine;

public class MovingPlatforms : Interactable
{
    public float Speed; //Frequency
    public float WidthToTravel; //Amplitude

    [Header("Which Axis")] [Tooltip("which Axis is the player trying to stop the platforms on")]
    public bool x;

    [Tooltip("which Axis is the player trying to stop the platforms on")]
    public bool z;
    // public bool y;

    [Tooltip("Transform that you can place out on an axis that the player will try to stop the" +
             "platform near")]
    public Transform AligneTransform;

    [Tooltip("The distance that will count as okey")]
    public float DistanceThreshold;

    private bool currentlyHoldingDown = false;
    private bool isMoving = true;
    private bool succeeded;
    private bool hasStarted;
    private Vector3 startPosition;
    private float time;


    public override void Interact()
    {
        currentlyHoldingDown = !currentlyHoldingDown;

        if (currentlyHoldingDown && !succeeded)
        {
            isMoving = !isMoving;
            if (!isMoving)
                if (CheckDistanceThreshold())
                {
                    base.Interact();
                    succeeded = true;
                }
        }
    }


    private void Update()
    {
        if (!enabled) return;

        if (!hasStarted)
        {
            startPosition = transform.position;
            hasStarted = true;
        }

        if (isMoving && hasStarted)
        {
            time += Time.deltaTime;
            transform.position = startPosition +
                                 (transform.right * (Mathf.Sin(time * Speed) * WidthToTravel));
        }
    }

    private bool CheckDistanceThreshold()
    {
        if (x)
        {
            float aligneX = AligneTransform.position.x;
            if (DistanceThreshold > Mathf.Abs(transform.position.x - aligneX))
            {
                return true;
            }
        }
        else if (z)
        {
            float aligneZ = AligneTransform.position.z;
            if (DistanceThreshold > Mathf.Abs(transform.position.z - aligneZ))
            {
                return true;
            }
        }

        return false;
    }
}