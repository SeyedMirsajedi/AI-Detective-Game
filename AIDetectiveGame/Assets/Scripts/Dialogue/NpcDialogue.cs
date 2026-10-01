using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class NpcDialogue : MonoBehaviour
{
    [Header("NPC Identity")]

    [SerializeField]
    private string npcId;

    [Header("NPC Information")]

    [SerializeField]
    private string displayName = "NPC";

    [SerializeField]
    private Sprite portrait;

    [Header("Temporary Dialogue")]

    [SerializeField]
    [TextArea(2, 4)]
    private string hardCodedResponse =
        "I understand, detective. I don't have a useful answer to that yet.";

    [Header("Dialogue Limits")]

    [SerializeField]
    [Min(1)]
    private int maxQuestions = 5;

    [SerializeField]
    [Min(1)]
    private int maxTotalPlayerCharacters = 500;

    [Header("Development")]

    [SerializeField]
    private bool logSanitizedRequests = true;

    private CaseState caseState;

    private static readonly DialogueMessage[] emptyHistory =
        new DialogueMessage[0];

    public string NpcId => npcId;

    public string DisplayName => displayName;

    public Sprite Portrait => portrait;

    public IReadOnlyList<DialogueMessage>
        ConversationHistory
    {
        get
        {
            ResolvePersistentCaseState();

            if (caseState == null ||
                string.IsNullOrWhiteSpace(npcId))
            {
                return emptyHistory;
            }

            return caseState
                .GetNpcConversationHistory(
                    npcId);
        }
    }

    public int QuestionsRemaining
    {
        get
        {
            ResolvePersistentCaseState();

            if (caseState == null ||
                string.IsNullOrWhiteSpace(npcId))
            {
                return maxQuestions;
            }

            int questionsUsed =
                caseState.GetNpcQuestionsUsed(
                    npcId);

            return Mathf.Max(
                0,
                maxQuestions -
                questionsUsed);
        }
    }

    public int CharactersRemaining
    {
        get
        {
            ResolvePersistentCaseState();

            if (caseState == null ||
                string.IsNullOrWhiteSpace(npcId))
            {
                return maxTotalPlayerCharacters;
            }

            int charactersUsed =
                caseState
                    .GetNpcPlayerCharactersUsed(
                        npcId);

            return Mathf.Max(
                0,
                maxTotalPlayerCharacters -
                charactersUsed);
        }
    }

    public bool CanAskAnotherQuestion =>
        QuestionsRemaining > 0 &&
        CharactersRemaining > 0;

    public bool InitialConversationCompleted
    {
        get
        {
            ResolvePersistentCaseState();

            if (caseState == null ||
                string.IsNullOrWhiteSpace(npcId))
            {
                return false;
            }

            return caseState
                .IsNpcInitialConversationCompleted(
                    npcId);
        }
    }

    private void Start()
    {
        ResolvePersistentCaseState();

        if (string.IsNullOrWhiteSpace(npcId))
        {
            Debug.LogWarning(
                "NpcDialogue on \"" +
                gameObject.name +
                "\" has an empty NPC ID. " +
                "Dialogue state will not persist.",
                this);
        }

        if (caseState == null)
        {
            Debug.LogWarning(
                "NpcDialogue on \"" +
                gameObject.name +
                "\" could not find the persistent CaseState.",
                this);
        }
    }

    public void InitializeRuntime(
        NpcDefinition npcDefinition,
        string runtimeHardCodedResponse,
        CaseState runtimeCaseState)
    {
        if (npcDefinition == null)
        {
            Debug.LogError(
                "NpcDialogue cannot initialize with a null NpcDefinition.",
                this);

            return;
        }

        npcId =
            npcDefinition.NpcId;

        displayName =
            npcDefinition.DisplayName;

        portrait =
            npcDefinition.Portrait;

        maxQuestions =
            npcDefinition.DefaultMaxQuestions;

        maxTotalPlayerCharacters =
            npcDefinition.DefaultMaxPlayerCharacters;

        if (!string.IsNullOrWhiteSpace(
                runtimeHardCodedResponse))
        {
            hardCodedResponse =
                runtimeHardCodedResponse;
        }

        caseState =
            runtimeCaseState;
    }

    public DialogueRequest BuildDialogueRequest(
        string playerMessage)
    {
        if (string.IsNullOrWhiteSpace(
                playerMessage))
        {
            Debug.LogWarning(
                "NpcDialogue cannot build a dialogue request " +
                "from an empty player message.",
                this);

            return null;
        }

        ResolvePersistentCaseState();

        if (caseState == null)
        {
            Debug.LogWarning(
                "NpcDialogue cannot build a dialogue request " +
                "because no persistent CaseState is available.",
                this);

            return null;
        }

        if (string.IsNullOrWhiteSpace(
                npcId))
        {
            Debug.LogWarning(
                "NpcDialogue cannot build a dialogue request " +
                "because its NPC ID is empty.",
                this);

            return null;
        }

        ScenarioDefinition scenario =
            caseState.ActiveScenario;

        if (scenario == null)
        {
            Debug.LogWarning(
                "NpcDialogue cannot build a dialogue request " +
                "because CaseState has no active scenario.",
                this);

            return null;
        }

        ScenarioDefinition.ScenarioNpcProfile profile =
            scenario.GetNpcProfile(
                npcId);

        if (profile == null)
        {
            Debug.LogWarning(
                "NpcDialogue could not find a scenario NPC profile " +
                "for NPC ID \"" +
                npcId +
                "\".",
                this);

            return null;
        }

        NpcDefinition npcDefinition =
            profile.NpcDefinition;

        DialogueRequest request =
            new DialogueRequest
            {
                ScenarioId =
                    scenario.ScenarioId,

                NpcId =
                    npcId,

                ModelId =
                    DialogueModelSettingUI.GetSelectedModelId(),

                NpcDisplayName =
                    displayName,

                NpcRole =
                    npcDefinition != null
                        ? npcDefinition.Role
                        : string.Empty,

                PlayerMessage =
                    playerMessage.Trim(),

                LieOrEvasionNotes =
                    profile.AllowedLieOrEvasionNotes
            };

        CopyFacts(
            profile.KnownFacts,
            request.KnownFacts);

        CopyFacts(
            profile.PrivateFacts,
            request.PrivateFacts);

        CopyFacts(
            profile.FalseBeliefs,
            request.FalseBeliefs);

        foreach (
            DialogueMessage historyMessage
            in ConversationHistory)
        {
            if (historyMessage == null)
            {
                continue;
            }

            request
                .PreviousConversationHistory
                .Add(
                    new DialogueHistoryMessageData
                    {
                        Speaker =
                            historyMessage.Speaker,

                        Text =
                            historyMessage.Text
                    });
        }

        if (logSanitizedRequests)
        {
            LogDialogueRequest(
                request);
        }

        return request;
    }

    public string GetResponse(
        string playerMessage)
    {
        return hardCodedResponse;
    }

    public bool TryAddPlayerMessage(
        string message)
    {
        if (string.IsNullOrWhiteSpace(
                message))
        {
            return false;
        }

        ResolvePersistentCaseState();

        if (caseState == null)
        {
            Debug.LogWarning(
                "NpcDialogue cannot store the player message " +
                "because no persistent CaseState is available.",
                this);

            return false;
        }

        if (string.IsNullOrWhiteSpace(
                npcId))
        {
            Debug.LogWarning(
                "NpcDialogue cannot store the player message " +
                "because its NPC ID is empty.",
                this);

            return false;
        }

        return caseState
            .TryAddNpcPlayerMessage(
                npcId,
                message,
                maxQuestions,
                maxTotalPlayerCharacters);
    }

    public void AddNpcMessage(
        string message)
    {
        ResolvePersistentCaseState();

        if (caseState == null)
        {
            Debug.LogWarning(
                "NpcDialogue cannot store the NPC response " +
                "because no persistent CaseState is available.",
                this);

            return;
        }

        if (string.IsNullOrWhiteSpace(
                npcId))
        {
            Debug.LogWarning(
                "NpcDialogue cannot store the NPC response " +
                "because its NPC ID is empty.",
                this);

            return;
        }

        caseState.AddNpcDialogueMessage(
            npcId,
            displayName,
            message);
    }

    public bool TryGetInitialConversationReveal(
    out string revealText)
    {
        revealText = string.Empty;

        ResolvePersistentCaseState();

        if (caseState == null ||
            string.IsNullOrWhiteSpace(npcId))
        {
            return false;
        }

        return caseState
            .TryGetNpcInitialConversationReveal(
                npcId,
                out revealText);
    }

    public void NotifyDialogueClosed()
    {
        ResolvePersistentCaseState();

        if (caseState == null ||
            string.IsNullOrWhiteSpace(npcId))
        {
            return;
        }

        caseState
            .CompleteNpcInitialConversationIfMeaningful(
                npcId);
    }

    [ContextMenu(
        "Debug Log Sanitized Dialogue Request")]
    private void DebugLogSanitizedDialogueRequest()
    {
        DialogueRequest request =
            BuildDialogueRequest(
                "Development request test.");

        if (request == null)
        {
            Debug.LogWarning(
                "The development dialogue request " +
                "could not be built.",
                this);
        }
    }

    private static void CopyFacts(
        CaseFactDefinition[] sourceFacts,
        List<DialogueFactData> destination)
    {
        if (sourceFacts == null ||
            destination == null)
        {
            return;
        }

        foreach (
            CaseFactDefinition fact
            in sourceFacts)
        {
            if (fact == null)
            {
                continue;
            }

            destination.Add(
                new DialogueFactData
                {
                    FactId =
                        fact.FactId,

                    Description =
                        fact.DeveloperDescription
                });
        }
    }

    private void LogDialogueRequest(
        DialogueRequest request)
    {
        if (request == null)
        {
            return;
        }

        StringBuilder builder =
            new StringBuilder();

        builder.AppendLine(
            $"Model: {request.ModelId}");

        builder.AppendLine(
            "[Dialogue Request]");

        builder.AppendLine(
            "Scenario: " +
            request.ScenarioId);

        builder.AppendLine(
            "NPC: " +
            request.NpcDisplayName +
            " (" +
            request.NpcId +
            ")");

        builder.AppendLine(
            "Role: " +
            request.NpcRole);

        builder.AppendLine();

        builder.AppendLine(
            "Player Message:");

        builder.AppendLine(
            request.PlayerMessage);

        AppendFactSection(
            builder,
            "Known Facts",
            request.KnownFacts);

        AppendFactSection(
            builder,
            "Private Facts",
            request.PrivateFacts);

        AppendFactSection(
            builder,
            "False Beliefs",
            request.FalseBeliefs);

        builder.AppendLine();

        builder.AppendLine(
            "Lie / Evasion Notes:");

        builder.AppendLine(
            string.IsNullOrWhiteSpace(
                request.LieOrEvasionNotes)
                ? "(none)"
                : request.LieOrEvasionNotes);

        builder.AppendLine();

        builder.AppendLine(
            "Previous Conversation History:");

        if (request
                .PreviousConversationHistory
                .Count == 0)
        {
            builder.AppendLine(
                "(none)");
        }
        else
        {
            foreach (
                DialogueHistoryMessageData message
                in request
                    .PreviousConversationHistory)
            {
                builder.AppendLine(
                    message.Speaker +
                    ": " +
                    message.Text);
            }
        }

        Debug.Log(
            builder.ToString(),
            this);
    }

    private static void AppendFactSection(
        StringBuilder builder,
        string sectionName,
        List<DialogueFactData> facts)
    {
        builder.AppendLine();

        builder.AppendLine(
            sectionName + ":");

        if (facts == null ||
            facts.Count == 0)
        {
            builder.AppendLine(
                "(none)");

            return;
        }

        foreach (
            DialogueFactData fact
            in facts)
        {
            builder.AppendLine(
                "- " +
                fact.FactId +
                ": " +
                fact.Description);
        }
    }

    private void ResolvePersistentCaseState()
    {
        if (caseState != null)
        {
            return;
        }

        if (RoomNavigationState.Instance == null)
        {
            return;
        }

        caseState =
            RoomNavigationState.Instance
                .GetComponent<CaseState>();
    }
}

public class DialogueMessage
{
    public string Speaker
    {
        get;
    }

    public string Text
    {
        get;
    }

    public DialogueMessage(
        string speaker,
        string text)
    {
        Speaker = speaker;
        Text = text;
    }
}