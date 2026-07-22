using UnityEngine;

[ExecuteInEditMode]
public class EditorAlignToPlanet : MonoBehaviour
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

        // Richtung vom Zentrum zur aktuellen Position (z. B. "nach oben")
        Vector3 gravityUp = (transform.position - planetCenter.position).normalized;

        // Beispiel-Rotation: "Up" zeigt weg vom Planeten, Forward beliebig
        transform.rotation = Quaternion.FromToRotation(Vector3.up, gravityUp);
    }
}