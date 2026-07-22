using UnityEngine;

[ExecuteInEditMode]
public class ZAlignToPlanet : MonoBehaviour
{
    private Transform planetCenter;
    public string alignTarget = "Planet1";

    public void Start()
    {
        GameObject planet = GameObject.Find(alignTarget);
        if (planet != null)
        {
            planetCenter = planet.transform;
        }
        else
        {
            Debug.LogWarning(alignTarget + " nicht gefunden!");
        }
    }

    void Update()
    {
        if (planetCenter == null) return;

        Vector3 gravityUp = (transform.position - planetCenter.position).normalized;

        transform.rotation = Quaternion.FromToRotation(Vector3.forward, gravityUp);
    }
}