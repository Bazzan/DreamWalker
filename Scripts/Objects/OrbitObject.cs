using UnityEngine;

public class OrbitObject : MonoBehaviour
{
    public Vector3 Offset;

    [Header("Which axis to rotate around")]
    public bool Xaxis;
    public bool Yaxis;
    public bool Zaxis;

    public bool lockRotationAroundOwnAxis = false;

    public float speed;

    [Header("How meny degrees to rotate per second")]
    public Vector3 Rotate;


    private Vector3 startPosition;
    private Quaternion originalRotation;

    private void Awake()
    {
        startPosition = transform.position;
        if (!Xaxis || !Yaxis || !Zaxis)
            Xaxis = true;

        originalRotation = transform.rotation;
    }


    private void Update()
    {
       
        if (Offset != Vector3.zero)
        {
            if (Xaxis)
                transform.RotateAround(Offset + startPosition, Vector3.right, speed * Time.deltaTime);
            else if (Yaxis)
                transform.RotateAround(Offset + startPosition, Vector3.up, speed * Time.deltaTime);
            else if (Zaxis)
                transform.RotateAround(Offset + startPosition, Vector3.forward, speed * Time.deltaTime);

            if (lockRotationAroundOwnAxis)
            {
                transform.rotation = originalRotation;
            }
        }

        if (Rotate != Vector3.zero)
        {
            transform.rotation *= Quaternion.Euler(Rotate * Time.deltaTime);
        }
    }

    private void OnValidate()
    {
        startPosition = transform.position;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(Offset + startPosition, 0.3f);
    }
}