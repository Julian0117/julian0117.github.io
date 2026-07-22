using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;

public class InteractableNPC : MonoBehaviour, Interactable
{
    [Header("UI")]
    public GameObject panel;
    public Image interactPrompt;
    public TMP_Text dialogueText;
    public TMP_Text helpText;

    [Header("Input")]
    public InputActionReference interactAction;

    [Header("Dialogue")]
    public string[] lines;
    public float textSpeed = 0.05f;

    private bool dialogueActive = false;
    private bool dialogueDone = false;
    private int index = 0;
    private Coroutine typingCoroutine;
    private bool lineFullyDisplayed = false;

    private Rigidbody playerRb;
    private RigidbodyConstraints originalConstraints;

    [Header("NPC Move After Dialogue")]
    public Transform moveTarget;
    public GameObject hiddenObject;


    private void Awake()
    {
        playerRb = GameObject.FindWithTag("Player").GetComponent<Rigidbody>();
        originalConstraints = playerRb.constraints;
    }

    public void ShowPrompt()
    {
        panel.gameObject.SetActive(false);
        interactPrompt.gameObject.SetActive(true);
    }

    public void HidePrompt()
    {
        interactPrompt.gameObject.SetActive(false);
        if (dialogueDone) return;
        panel.gameObject.SetActive(true);
    }

    public void Interact()
    {
        if (!dialogueActive)
        {
            dialogueText.text = string.Empty;
            interactPrompt.gameObject.SetActive(false);
            helpText.gameObject.SetActive(false);
            panel.SetActive(true);
            dialogueText.gameObject.SetActive(true);
            dialogueActive = true;
            playerRb.constraints = RigidbodyConstraints.FreezeAll;
            index = 0;
            typingCoroutine = StartCoroutine(TypeLine());
        }
        else
        {
            if (!lineFullyDisplayed)
            {
                // Zeige den aktuellen Satz sofort komplett
                StopCoroutine(typingCoroutine);
                dialogueText.text = lines[index];
                lineFullyDisplayed = true;
            }
            else
            {
                NextLine();
            }
        }
    }

    private IEnumerator TypeLine()
    {
        lineFullyDisplayed = false;
        dialogueText.text = string.Empty;

        foreach (char c in lines[index].ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        lineFullyDisplayed = true;
    }

    private void NextLine()
    {
        if (index < lines.Length - 1)
        {
            index++;
            typingCoroutine = StartCoroutine(TypeLine());
        }
        else
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        panel.SetActive(false);
        interactPrompt.gameObject.SetActive(true);
        dialogueActive = false;
        dialogueDone = true;

        playerRb.constraints = originalConstraints;

        if (moveTarget != null)
        {
            transform.position = moveTarget.position;
            transform.rotation = moveTarget.rotation;
            hiddenObject.SetActive(true);
        }
    }
}