using UnityEngine;

[ExecuteInEditMode]
public class PlayerCameraController : MonoBehaviour
{
    #region TestVariables

    public Transform player;
    public Camera PlayerCamera;
    public Transform CameraTramsform;

    public Vector3 rotation;
    public Vector3 localPos;

    #endregion

    [SerializeField] private float PitchSpeed;
    [SerializeField] private float YawSpeed;
    [SerializeField] private Vector3 overTheShoulderOffset;
    [SerializeField] private Vector3 cameraTargetPosition;
    private Quaternion cameraTargetRotation;
    public Transform parentTransform;

    private float pitchDegresToRotate;

    private Vector3 RefVelocity;
    private Vector3 RefVelocity2;
    public float parentDampMovement;
    public float SmoothDamp2;

    private void Update()
    {
        Vector3 WASDInput = new Vector3(Input.GetAxis("Horizontal"),0f,Input.GetAxis("Vertical"));

        player.position += player.forward * WASDInput.z * (10f * Time.deltaTime);
        player.position += player.right * WASDInput.x * (10f * Time.deltaTime);

        
        player.rotation *=
            Quaternion.Euler(0f, Input.GetAxis("Mouse X") * YawSpeed, 0f);
    }

    private void LateUpdate()
    {
        // Vector3 parentTargetPos = player.position + (player.right);
        // parentTargetPos.y += overTheShoulderOffset.y;
        // parentTargetPos.x += overTheShoulderOffset.x;
        //
        // Vector3 dampedParentPos = Vector3.SmoothDamp(parentTransform.position, parentTargetPos, ref RefVelocity,
        //     parentDampMovement);
        // parentTransform.position = dampedParentPos;
        //
        // cameraTargetPosition = parentTargetPos + player.forward * overTheShoulderOffset.z;
        //
        // transform.position = cameraTargetPosition;
        //
        // pitchDegresToRotate -= PitchSpeed * Input.GetAxis("Mouse Y");
        // pitchDegresToRotate = Mathf.Clamp(pitchDegresToRotate, -20, 90);
        // parentTransform.rotation = Quaternion.Euler(pitchDegresToRotate,player.rotation.eulerAngles.y,0f);
        
        //all is shit
        /// TODO: finish this shit it sucks

        Vector3 parentTargetPos = player.position ;
        Vector3 parentDirection = player.forward;
        parentTransform.position += overTheShoulderOffset;
        Vector3 dampedParentTargetPos = Vector3.SmoothDamp(parentTransform.position , parentTargetPos, ref RefVelocity,
            parentDampMovement);
        parentTransform.position = dampedParentTargetPos;
        parentTransform.rotation = player.rotation;
        
        
        
        // cameraTargetPosition = (parentTransform.forward + new Vector3(overTheShoulderOffset.x, overTheShoulderOffset.y,0f));
        // cameraTargetPosition += Vector3.forward * -10;
        // cameraTargetPosition = new Vector3(overTheShoulderOffset.x + cameraTargetPosition.x,overTheShoulderOffset.y +cameraTargetPosition.y, cameraTargetPosition.z);
        // transform.position = cameraTargetPosition;

        cameraTargetPosition = parentDirection * overTheShoulderOffset.z ;
        // cameraTargetPosition = cameraTargetPosition - (player.right);
        cameraTargetPosition += new Vector3((parentTransform.localPosition.x + overTheShoulderOffset.x), (parentTransform.localPosition.y + overTheShoulderOffset.y),0f);
        
        // pitchDegresToRotate -= PitchSpeed * Input.GetAxis("Mouse Y");
        // pitchDegresToRotate = Mathf.Clamp(pitchDegresToRotate, -20, 90);
        // parentTransform.rotation = Quaternion.Euler(pitchDegresToRotate, player.rotation.eulerAngles.y,0f);
        
        Vector3 cameraTargetLocation = Vector3.SmoothDamp(transform.position, cameraTargetPosition, ref RefVelocity2, SmoothDamp2 );
        transform.position = cameraTargetLocation;
        transform.localRotation = Quaternion.Euler(player.forward);
    }

    // private void Awake()
    // {
    //     parentTransform.rotation = player.rotation;
    // }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawSphere(parentTransform.position + overTheShoulderOffset, 0.3f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(cameraTargetPosition, 0.2f);
        
        // Gizmos.color = Color.magenta;
        // Gizmos.DrawSphere(player.forward * player.position, 0.2f);

    }
}