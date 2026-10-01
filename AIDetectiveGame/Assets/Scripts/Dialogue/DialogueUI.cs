using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class DialogueUI : MonoBehaviour
{
    [Header("Dialogue Panel")]

    [SerializeField]
    private GameObject dialoguePanel;

    [SerializeField]
    private TMP_Text npcNameText;

    [SerializeField]
    private Image portraitImage;

    [Header("Conversation")]

    [SerializeField]
    private TMP_Text conversationText;

    [SerializeField]
    private ScrollRect conversationScrollRect;

    [SerializeField]
    private TMP_InputField playerInputField;

    [SerializeField]
    private TMP_Text dialogueLimitText;

    [SerializeField]
    private TMP_Text dialogueStatusText;

    [SerializeField]
    private Button sendButton;

    [Header("Dialogue Service")]

    [SerializeField]
    private MonoBehaviour dialogueServiceSource;

    [Header("Other UI")]

    [SerializeField]
    private ClueNotebookUI clueNotebookUI;

    [SerializeField]
    private Button notebookButton;

    [Header("Player")]

    [SerializeField]
    private PlayerMovement playerMovement;

    [SerializeField]
    private PlayerInteraction playerInteraction;

    private NpcDialogue activeNpc;

    private IDialogueService dialogueService;

    private bool isWaitingForResponse;

    private DialogueEmotion lastEmotion =
        DialogueEmotion.Neutral;

    public bool IsOpen =>
        dialoguePanel != null &&
        dialoguePanel.activeSelf;

    private void Start()
    {
        ResolveDialogueService();

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        if (playerInputField != null)
        {
            playerInputField.onSubmit.AddListener(
                HandleInputSubmitted);
        }
    }

    private void OnDestroy()
    {
        if (playerInputField != null)
        {
            playerInputField.onSubmit.RemoveListener(
                HandleInputSubmitted);
        }
    }

    private void Update()
    {
        if (!IsOpen ||
            Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.escapeKey
            .wasPressedThisFrame)
        {
            CloseDialogue();
        }
    }

    public void OpenDialogue(NpcDialogue npc)
    {
        if (npc == null)
        {
            Debug.LogWarning(
                "DialogueUI was asked to open with no NpcDialogue.",
                this);

            return;
        }

        activeNpc = npc;

        if (npcNameText != null)
        {
            npcNameText.text =
                activeNpc.DisplayName;
        }

        UpdatePortrait();
        RefreshConversationHistory();

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        if (playerInteraction != null)
        {
            playerInteraction
                .SetInteractionEnabled(false);
        }

        SetNotebookEnabled(false);

        SetDialogueStatus("");


        RefreshDialogueLimits();
        FocusInputField();
    }

    public async void SendPlayerMessage()
    {
        if (isWaitingForResponse)
        {
            return;
        }

        if (activeNpc == null ||
            playerInputField == null)
        {
            return;
        }

        if (dialogueService == null)
        {
            ResolveDialogueService();

            if (dialogueService == null)
            {
                Debug.LogError(
                    "DialogueUI cannot send a message because " +
                    "no valid IDialogueService is assigned.",
                    this);

                return;
            }
        }

        if (!activeNpc.CanAskAnotherQuestion)
        {
            RefreshDialogueLimits();
            return;
        }

        string playerMessage =
            playerInputField.text;

        if (string.IsNullOrWhiteSpace(
                playerMessage))
        {
            FocusInputField();
            return;
        }

        playerMessage =
            playerMessage.Trim();

        if (playerMessage.Length >
            activeNpc.CharactersRemaining)
        {
            RefreshDialogueLimits();
            FocusInputField();
            return;
        }

        DialogueRequest request =
            activeNpc.BuildDialogueRequest(
                playerMessage);

        if (request == null)
        {
            Debug.LogWarning(
                "DialogueUI could not build a dialogue request.",
                this);

            FocusInputField();
            return;
        }

        isWaitingForResponse = true;

        SetDialogueStatus("Thinking...");

        RefreshDialogueLimits();

        try
        {
            DialogueResponse response =
                await dialogueService
                    .GenerateReplyAsync(
                        request);

            if (response == null)
            {
                Debug.LogWarning(
                    "Dialogue service returned a null response.",
                    this);

                SetDialogueStatus(
                    "Unable to get a response. Please try again.");

                return;
            }

            if (string.IsNullOrWhiteSpace(
        response.ReplyText))
            {
                Debug.LogWarning(
                    "Dialogue service returned an empty reply.",
                    this);

                SetDialogueStatus(
                    "Unable to get a response. Please try again.");

                return;
            }

            if (activeNpc == null)
            {
                Debug.LogWarning(
                    "The active NPC became unavailable before " +
                    "the dialogue response was completed.",
                    this);

                return;
            }

            bool messageAccepted =
                activeNpc.TryAddPlayerMessage(
                    playerMessage);

            if (!messageAccepted)
            {
                Debug.LogWarning(
                    "The dialogue service returned successfully, " +
                    "but the player message could not be committed.",
                    this);

                return;
            }

            string npcReply =
                response.ReplyText.Trim();

            activeNpc.AddNpcMessage(
                npcReply);

            lastEmotion =
                response.Emotion;

            SetDialogueStatus("");

            Debug.Log(
                "Dialogue response emotion for " +
                activeNpc.DisplayName +
                ": " +
                lastEmotion,
                this);

            playerInputField.text = "";

            RefreshConversationHistory();
            RefreshDialogueLimits();
            ScrollToBottom();
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                "Dialogue service failed: " +
                exception.Message,
                this);

            SetDialogueStatus(
                "Unable to get a response. Please try again.");
        }
        finally
        {
            isWaitingForResponse = false;

            RefreshDialogueLimits();
            FocusInputField();
        }
    }

    public void CloseDialogue()
    {
        if (!IsOpen)
        {
            return;
        }

        if (isWaitingForResponse)
        {
            return;
        }

        if (activeNpc != null)
        {
            // Before actually closing the first meaningful
            // conversation, check whether this NPC has an
            // authored scenario reveal that the player
            // should see.
            if (activeNpc.TryGetInitialConversationReveal(
                    out string revealText))
            {
                // Add the authored reveal as an NPC message.
                // This is Unity-authored story content,
                // NOT generated by the LLM.
                activeNpc.AddNpcMessage(
                    revealText);

                // Now complete the authoritative progression.
                // This may discover C05 and unlock the Study.
                activeNpc.NotifyDialogueClosed();

                // Keep the dialogue panel open so the player
                // actually has time to read the reveal.
                SetDialogueStatus(
                    "Important information revealed.");

                RefreshConversationHistory();
                RefreshDialogueLimits();
                ScrollToBottom();

                return;
            }

            // Normal closing behavior.
            activeNpc.NotifyDialogueClosed();
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        if (playerInteraction != null)
        {
            playerInteraction
                .SetInteractionEnabled(true);
        }

        SetNotebookEnabled(true);

        EventSystem.current
            ?.SetSelectedGameObject(null);

        activeNpc = null;
    }

    private void HandleInputSubmitted(
        string submittedText)
    {
        if (!IsOpen ||
            isWaitingForResponse)
        {
            return;
        }

        SendPlayerMessage();
    }

    private void SetNotebookEnabled(
        bool isEnabled)
    {
        if (clueNotebookUI != null)
        {
            clueNotebookUI.enabled =
                isEnabled;
        }

        if (notebookButton != null)
        {
            notebookButton.interactable =
                isEnabled;
        }
    }

    private void RefreshConversationHistory()
    {
        if (conversationText == null)
        {
            return;
        }

        if (activeNpc == null)
        {
            conversationText.text = "";
            return;
        }

        StringBuilder builder =
            new StringBuilder();

        foreach (
            DialogueMessage message
            in activeNpc.ConversationHistory)
        {
            builder.AppendLine(
                message.Speaker + ":");

            builder.AppendLine(
                message.Text);

            builder.AppendLine();
        }

        conversationText.text =
            builder.ToString();
    }

    private void RefreshDialogueLimits()
    {
        if (activeNpc == null)
        {
            return;
        }

        if (dialogueLimitText != null)
        {
            dialogueLimitText.text =
                activeNpc.QuestionsRemaining +
                " questions left | " +
                activeNpc.CharactersRemaining +
                " chars left";
        }

        bool canAsk =
            activeNpc.CanAskAnotherQuestion &&
            !isWaitingForResponse;

        if (sendButton != null)
        {
            sendButton.interactable =
                canAsk;
        }

        if (playerInputField != null)
        {
            playerInputField.interactable =
                canAsk;

            if (activeNpc.CanAskAnotherQuestion)
            {
                playerInputField.characterLimit =
                    activeNpc.CharactersRemaining;
            }
            else
            {
                playerInputField.characterLimit = 1;
                playerInputField.text = "";
            }
        }
    }

    private void ScrollToBottom()
    {
        if (conversationScrollRect == null)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();

        conversationScrollRect
            .verticalNormalizedPosition = 0f;
    }

    private void FocusInputField()
    {
        if (playerInputField == null)
        {
            return;
        }

        if (!playerInputField.interactable)
        {
            return;
        }

        EventSystem.current
            ?.SetSelectedGameObject(
                playerInputField.gameObject);

        playerInputField
            .ActivateInputField();
    }

    private void UpdatePortrait()
    {
        if (portraitImage == null)
        {
            return;
        }

        if (activeNpc != null &&
            activeNpc.Portrait != null)
        {
            portraitImage.sprite =
                activeNpc.Portrait;

            portraitImage.gameObject
                .SetActive(true);
        }
        else
        {
            portraitImage.sprite = null;

            portraitImage.gameObject
                .SetActive(false);
        }
    }

    private void SetDialogueStatus(
    string message)
    {
        if (dialogueStatusText == null)
        {
            return;
        }

        dialogueStatusText.text =
            message ?? "";
    }

    private void ResolveDialogueService()
    {
        if (dialogueServiceSource == null)
        {
            dialogueService = null;

            Debug.LogError(
                "DialogueUI has no Dialogue Service Source assigned.",
                this);

            return;
        }

        dialogueService =
            dialogueServiceSource
                as IDialogueService;

        if (dialogueService == null)
        {
            Debug.LogError(
                "The assigned Dialogue Service Source does not " +
                "implement IDialogueService.",
                this);
        }
    }
}