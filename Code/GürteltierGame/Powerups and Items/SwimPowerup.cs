using UnityEngine;

public class SwimPowerUp : MonoBehaviour
{
    [SerializeField] private GameObject swimPopup;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerManager>().swimUnlocked = true;
            AudioController.Instance.PlayPowerupSound();
            GameManager.Instance.ShowPopup(swimPopup);
            gameObject.SetActive(false); // remove power-up after pickup
        }
    }
}