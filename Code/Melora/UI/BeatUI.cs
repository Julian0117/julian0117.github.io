using UnityEngine;
using UnityEngine.UI;

public class BeatUI : MonoBehaviour
{
    [Header("Prefab & Layout")]
    [SerializeField] private GameObject togglePrefab;
    [SerializeField] private Transform layoutParent;

    [SerializeField] private float uiWidth = 160f;
    [SerializeField] private float uiHeight = 25f;

    [Header("Toggle Icons")]
    [SerializeField] private Sprite iconSprite;

    private BeatSound beatSound;

    void Awake()
    {
        beatSound = GetComponentInParent<BeatSound>();
    }

    void Start()
    {
        RebuildUI(BeatManager.Instance.beatsPerBar);
    }

    public void RebuildUI(int beatsPerBar)
    {
        if (beatsPerBar <= 0) beatsPerBar = layoutParent.childCount;

        // Alte Toggles löschen
        for (int i = layoutParent.childCount - 1; i >= 0; i--)
            Destroy(layoutParent.GetChild(i).gameObject);

        // Neue Toggles erzeugen
        for (int i = 0; i < beatsPerBar; i++)
        {
            GameObject toggleObj = Instantiate(togglePrefab, layoutParent);
            Toggle toggle = toggleObj.GetComponent<Toggle>();
            RectTransform rectTransform = toggleObj.GetComponent<RectTransform>();

            // Dimensionen setzen
            Vector2 toggleSize = new Vector2(uiWidth / beatsPerBar, uiHeight);
            rectTransform.sizeDelta = toggleSize;
            
            int capturedIndex = i; // Closure für Listener

            // Toggle initialisieren
            toggle.isOn = beatSound.GetBeat(capturedIndex);
            //Debug.Log($"GetBeat[{capturedIndex}] = {beatSound.GetBeat(capturedIndex)}");
            toggle.onValueChanged.AddListener(v =>
            {
                beatSound.SetBeat(capturedIndex, v);
            });

            // Icon zuweisen
            AssignIcon(toggleObj);
        }
    }

    public void AssignIcon(GameObject toggleObj)
    {
        if (iconSprite == null) return;

        // Background/Icon Hierarchie: Background/Icon
        Transform background = toggleObj.transform.Find("Background");
        if (background == null) return;

        Transform icon = background.Find("Icon");
        if (icon == null) return;

        Image iconImage = icon.GetComponent<Image>();
        if (iconImage == null) return;

        iconImage.sprite = iconSprite;
        iconImage.preserveAspect = true;

#if UNITY_EDITOR
        // Editor-Support
        UnityEditor.Undo.RecordObject(iconImage, "Set Icon Image");
        UnityEditor.EditorUtility.SetDirty(iconImage);
#endif
    }
}