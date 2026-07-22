using UnityEngine;
using UnityEngine.InputSystem;

public class Intro : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject introPanel;

    [Header("Input")]
    public InputActionReference interactAction;

    [SerializeField] private int currentPageIndex = 0;
    private Transform[] pages;

    private void OnEnable()
    {
        if (interactAction != null)
            interactAction.action.performed += OnInteract;
    }

    private void OnDisable()
    {
        if (interactAction != null)
            interactAction.action.performed -= OnInteract;
    }

    private void Start()
    {
        if (introPanel == null)
        {
            Debug.LogError("IntroPanel ist nicht zugewiesen.");
            return;
        }

        pages = new Transform[introPanel.transform.childCount];
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i] = introPanel.transform.GetChild(i);
            pages[i].gameObject.SetActive(false);
        }

        if (pages.Length == 0)
        {
            Debug.LogWarning("IntroPanel hat keine Kindobjekte.");
            return;
        }

        introPanel.SetActive(true);
        pages[0].gameObject.SetActive(true);
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (pages.Length == 0 || currentPageIndex >= pages.Length) return;

        pages[currentPageIndex].gameObject.SetActive(false);
        currentPageIndex++;

        if (currentPageIndex >= pages.Length)
        {
            introPanel.SetActive(false);
        }
        else
        {
            pages[currentPageIndex].gameObject.SetActive(true);
        }
    }
}