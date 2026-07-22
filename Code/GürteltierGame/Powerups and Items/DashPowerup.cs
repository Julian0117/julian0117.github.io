using UnityEngine;

public class DashPowerup : MonoBehaviour
{
    [SerializeField] private GameObject dashPopup;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerManager>().dashUnlocked = true;
            AudioController.Instance.PlayPowerupSound();
            GameManager.Instance.ShowPopup(dashPopup);
            gameObject.SetActive(false); // remove power-up after pickup
        }
    }
}