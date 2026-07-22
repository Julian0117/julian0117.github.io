using UnityEngine;

public class FruitPowerup : MonoBehaviour
{
    [SerializeField] private GameObject fruitPopup;
    //private static bool firstFruit = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerManager>().AddFruit();
            AudioController.Instance.PlayFruitSound();
            Destroy(gameObject); // remove power-up after pickup
            //if (firstFruit) GameManager.Instance.ShowPopup(fruitPopup);
            //firstFruit = false;
        }
    }
}