using UnityEngine;

public abstract class GravityArea : MonoBehaviour
{
    [SerializeField] private int _priority;
    public int Priority => _priority;

    public CameraTargetFollow cameraTarget;

    [SerializeField] private float _gravityForce;
    public float Gravity => _gravityForce;

    [SerializeField] private bool _affectsCamera = false;
    public bool AffectsCamera => _affectsCamera;


    protected virtual void Start()
    {
        GetComponent<Collider>().isTrigger = true;
        cameraTarget = FindAnyObjectByType<CameraTargetFollow>();
    }

    public abstract Vector3 GetGravityDirection(GravityBody gravityBody);

    public virtual void OnBecameActive(GravityBody gravityBody)
    {
        if (AffectsCamera && cameraTarget != null)
        {
            cameraTarget.SetTargetGravityArea(this, gravityBody);
            //Debug.Log($"[GravityArea] Kamera-Ziel auf {name} gesetzt.");
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out GravityBody gravityBody))
        {
            gravityBody.AddGravityArea(this);
            //Debug.Log($"[GravityArea] GravityArea {name} hinzugefügt.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out GravityBody gravityBody))
        {
            gravityBody.RemoveGravityArea(this);
            //Debug.Log($"[GravityArea] GravityArea {name} entfernt.");
        }
    }
}