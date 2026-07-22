using UnityEngine;
using System.Collections;

public class BranchTrigger : MonoBehaviour
{
    public BranchDrop branch;
    public GameObject owl;
    public float delay = 2f;

    private bool activated = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (activated) return;

        if (collision.CompareTag("Player"))
        {
            activated = true;
            StartCoroutine(BreakAfterDelay());
        }
    }

    IEnumerator BreakAfterDelay()
    {
        yield return new WaitForSeconds(delay);

        branch.Drop();
        owl.GetComponent<OwlSwitch>().Activate();
    }
}
