using UnityEngine;
using System.Collections;

public class PressurePlateAnimationTrigger : MonoBehaviour
{
    [Header("References")]
    public Animator movableAnimator;

    [Header("Time until Trigger gets deactivated")]
    [SerializeField] public float activeTime = 5f;

    private int objectsInTrigger = 0;
    private Coroutine releaseCoroutine;

    private void Start()
    {
        // Suche Animator im Parent unter "MovablePart"
        Transform parent = transform.parent;
        if (parent != null)
        {
            Transform movablePart = parent.Find("MovablePart");
            if (movablePart != null)
            {
                movableAnimator = movablePart.GetComponent<Animator>();
            }
        }

        if (movableAnimator == null)
        {
            Debug.LogWarning($"{name}: Kein Animator gefunden!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        objectsInTrigger++;

        if (releaseCoroutine != null)
        {
            StopCoroutine(releaseCoroutine);
            releaseCoroutine = null;
        }

        if (objectsInTrigger == 1)
        {
            Debug.Log("PressurePlate Pressed");
            movableAnimator.SetBool("Pressed", true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        objectsInTrigger--;

        if (objectsInTrigger == 0)
        {
            Debug.Log($"PressurePlate Releasing in {activeTime} seconds...");
            releaseCoroutine = StartCoroutine(DelayedRelease());
        }
    }

    private IEnumerator DelayedRelease()
    {
        yield return new WaitForSeconds(activeTime);

        if (objectsInTrigger == 0)
        {
            Debug.Log("PressurePlate Released");
            movableAnimator.SetBool("Pressed", false);
        }

        releaseCoroutine = null;
    }
}