using System.Collections.Generic;
using UnityEngine;

public class CanaryTrigger : MonoBehaviour
{
    //private CapsuleCollider2D nestCollider;
    [SerializeField] private BeatSound beatSound;
    [SerializeField] private List<GameObject> children;
    [SerializeField] private int childCount = 0;
    [SerializeField] private int maxChildren = 4;

    void Start()
    {
        //nestCollider = GetComponent<CapsuleCollider2D>();
        InitializeChildren();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Babybird"))
        {
            children[childCount].SetActive(true);
            childCount++;
            Destroy(other.gameObject);
            if (childCount == maxChildren) beatSound.FirstEnable();
        }
    }

    private void InitializeChildren()
    {
        foreach (Transform child in transform)
        {
            //Debug.Log(child.name);
            children.Add(child.gameObject);

            child.gameObject.SetActive(false);
        }
    }
}