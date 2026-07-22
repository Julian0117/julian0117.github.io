using UnityEngine;

public class GravityPowerup : MonoBehaviour
{
    [SerializeField] private GameObject gravityPopup;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerManager>().gravitySwitchUnlocked = true;
            AudioController.Instance.PlayPowerupSound();
            GameManager.Instance.ShowPopup(gravityPopup);
            gameObject.SetActive(false); // remove power-up after pickup
        }
    }
}