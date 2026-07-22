using UnityEngine;
using UnityEngine.InputSystem;

public class AlarmClock : MonoBehaviour
{
    private int beatSoundCount = 0;
    
    [Header("Einstellungen")]
    [SerializeField] private float shakeSpeed = 50f;
    [SerializeField] private float shakeAmount = 15f;

    [SerializeField] private bool isRinging = false;
    private Quaternion originalRotation;

    private void Awake()
    {
        originalRotation = transform.rotation;
    }

    private void Update()
    {
        if (isRinging)
        {
            float zRotation = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount;
            transform.rotation = originalRotation * Quaternion.Euler(0, 0, zRotation);
        }

        if (isRinging && Keyboard.current != null)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                StopAlarmAndDeactivate();
            }
        }
    }

    public void CountBeatSound()
    {
        beatSoundCount++;
        if (beatSoundCount > 8)
        {
            gameObject.SetActive(true);
            isRinging = true;
        }
    }

    private void StopAlarmAndDeactivate()
    {
        isRinging = false;
        transform.rotation = originalRotation; 
        
        gameObject.SetActive(false);
    }
}