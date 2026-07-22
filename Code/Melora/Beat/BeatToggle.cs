using UnityEngine;
using UnityEngine.UI;

public class BeatToggle : MonoBehaviour
{
    [SerializeField] private int beatIndex;
    private Toggle toggle;
    private BeatSound beatSound;

    void Awake()
    {
        toggle = GetComponent<Toggle>();
        beatSound = GetComponentInParent<BeatSound>();

        toggle.onValueChanged.AddListener(value => beatSound.SetBeat(beatIndex, value));
    }
}