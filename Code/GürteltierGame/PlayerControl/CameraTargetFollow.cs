using UnityEngine.InputSystem;
using UnityEngine;

public class CameraTargetFollow : MonoBehaviour
{
    public Transform player;
    [SerializeField]
    private Transform gravityUpTarget;  // Statt planetCenter
    Vector3 gravityUp;
    public bool isPlanet;
    public Vector3 localOffset = new(0, 0, 0);

    //public InputActionReference lookInputAction;

    public void SetTargetGravityArea(GravityArea gravityArea, GravityBody body)
    {
        // Optional: GravityAreas können ein eigenes Ziel definieren
        gravityUpTarget = gravityArea.transform;
        isPlanet = gravityArea is GravityAreaPlanet ? true : false;
        if (!isPlanet) gravityUp = -gravityArea.transform.up;
        //Debug.Log("GravityArea isPlanet?:" + isPlanet);
    }

    void LateUpdate()
    {
        if (player == null || gravityUpTarget == null)
            return;
        if (isPlanet)
        {
            gravityUp = (player.position - gravityUpTarget.position).normalized;
        }
        //Debug.Log("gravityUp is: " + gravityUp);

        Vector3 projectedForward = Vector3.ProjectOnPlane(transform.forward, gravityUp).normalized;

        if (projectedForward.magnitude < 0.1f)
        {
            projectedForward = Vector3.ProjectOnPlane(transform.right, gravityUp).normalized;
        }

        Quaternion surfaceRotation = Quaternion.LookRotation(projectedForward, gravityUp);
        Vector3 targetPos = player.position + surfaceRotation * localOffset;
        targetPos += gravityUp * 0.15f;


        transform.position = targetPos;
        transform.rotation = surfaceRotation;
    }
}