using UnityEngine;
public class FrogEat : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Frosch hat etwas getroffen: " + collision.gameObject.name);

        if (collision.gameObject.CompareTag("Fly"))
        {
            Debug.Log("Frosch hat die Fliege gefressen!");
            Destroy(collision.gameObject);
        }
    }
}


