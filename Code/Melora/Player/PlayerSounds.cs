using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private AudioClip[] footstepSounds;
    private int footstepIndex = 0;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip throwSound;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void PlayFootstepSound()
    {
        if (audioSource != null && footstepSounds != null)
        {
            audioSource.PlayOneShot(footstepSounds[footstepIndex]);
            footstepIndex = 1 - footstepIndex;
        }
    }

    private void PlayJumpSound()
    {
        if (audioSource != null && jumpSound != null) audioSource.PlayOneShot(jumpSound);
    }
}