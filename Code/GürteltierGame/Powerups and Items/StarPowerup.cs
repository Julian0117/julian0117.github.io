using UnityEngine;

public class StarPowerup : MonoBehaviour
{
    [SerializeField] private GameObject starPopup;
    private static bool firstStar = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerManager>().AddStar();
            AudioController.Instance.PlayStarSound();
            if (firstStar) GameManager.Instance.ShowPopup(starPopup);
            firstStar = false;
            gameObject.SetActive(false); // remove power-up after pickup
        }
    }
}