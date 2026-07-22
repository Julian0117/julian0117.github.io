using UnityEngine;

public class PressurePlateTarget : MonoBehaviour
{
    [Header("Object to be activated")]
    public GameObject targetObject;

    public void OnPlatePressed()
    {
        Debug.Log("Press animation done, Ziel aktivieren.");
        targetObject.SetActive(true);
    }

    public void OnPlateReleased()
    {
        Debug.Log("Release animation done, Ziel deaktivieren.");
        targetObject.SetActive(false);
    }
}
