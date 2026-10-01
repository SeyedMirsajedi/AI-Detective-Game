using UnityEngine;

public class NpcInteractable : MonoBehaviour, IInteractable
{
    [SerializeField]
    private NpcDialogue npcDialogue;

    [SerializeField]
    private DialogueUI dialogueUI;

    public InteractionMode Mode =>
        InteractionMode.Press;

    public float HoldDuration => 0f;

    public string InteractionPrompt => "talk";

    private void Reset()
    {
        npcDialogue =
            GetComponent<NpcDialogue>();
    }

    private void Awake()
    {
        if (npcDialogue == null)
        {
            npcDialogue =
                GetComponent<NpcDialogue>();
        }
    }

    public void InitializeRuntime(
        DialogueUI runtimeDialogueUI)
    {
        dialogueUI =
            runtimeDialogueUI;
    }

    public void Interact()
    {
        if (npcDialogue == null)
        {
            Debug.LogWarning(
                "NpcInteractable has no NpcDialogue assigned.",
                this);

            return;
        }

        if (dialogueUI == null)
        {
            Debug.LogWarning(
                "NpcInteractable has no DialogueUI assigned.",
                this);

            return;
        }

        dialogueUI.OpenDialogue(
            npcDialogue);
    }
}