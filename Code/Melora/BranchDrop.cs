using UnityEngine;

public class BranchDrop : MonoBehaviour
{
    private Rigidbody2D RB;

    private void Awake()
    {
        RB = GetComponent<Rigidbody2D>();
        RB.bodyType = RigidbodyType2D.Kinematic;   // 一开始不掉落
    }

    public void Drop()   // ← 改名，不再叫 DropBranch()
    {
        RB.bodyType = RigidbodyType2D.Dynamic;     // 开始掉落
    }
}
