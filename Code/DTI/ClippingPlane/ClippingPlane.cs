using UnityEngine;

/// <summary>
///     The Class <c>ClippingPlane</c> is used to ensure that the clipping plane GameObject,
///     to which this Scipt is attached to does not leave the boundaries of its parent cube.
///     <param>parent</param> GameObject which contains the parent of the GameObject which this script is attached to.
/// </summary>
public class ClippingPlane : MonoBehaviour {
    private GameObject parent;
    //[SerializeField] float bounds = 1.0f;
    private Vector3 originalScale;
    /// <summary>
    ///     Initialises <param>parent<param> with the parent of the GameObject to which this script is attached to.
    /// </summary>
    void Start() {
        parent = transform.parent.gameObject;
        originalScale = transform.localScale;
    }

    private void LateUpdate()
    {
        gameObject.transform.localScale = originalScale;
    }

    /// <summary>
    ///     Checks if the attached GameObject is within the bounds of the parent cube.
    /// </summary>
    //void Update() {
    //    transform.parent = parent.transform;
    //    Vector3 newPos = transform.localPosition;
    //    if (newPos.x > bounds) 
    //        newPos = new Vector3(bounds, newPos.y, newPos.z);
    //    if (newPos.y > bounds) 
    //        newPos = new Vector3(newPos.x, bounds, newPos.z);
    //    if (newPos.z > bounds) 
    //        newPos = new Vector3(newPos.x, newPos.y, bounds);
    //    if (newPos.x < -bounds) 
    //        newPos = new Vector3(-bounds, newPos.y, newPos.z);
    //    if (newPos.y < -bounds) 
    //        newPos = new Vector3(newPos.x, -bounds, newPos.z);
    //    if (newPos.z < -bounds) 
    //        newPos = new Vector3(newPos.x, newPos.y, -bounds);
    //    gameObject.transform.localPosition = newPos;
    //}
}