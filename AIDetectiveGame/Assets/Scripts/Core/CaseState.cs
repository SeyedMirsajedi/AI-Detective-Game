using System.Collections.Generic;
using UnityEngine;

public class CaseState : MonoBehaviour
{
    private readonly List<ClueDefinition> discoveredClues =
        new List<ClueDefinition>();

    private readonly Dictionary<string, ContainerRuntimeState>
        containerStates =
            new Dictionary<string, ContainerRuntimeState>();

    private readonly Dictionary<string, NpcDialogueRuntimeState>
        npcDialogueStates =
            new Dictionary<string, NpcDialogueRuntimeState>();

    private readonly HashSet<string> completedProgressionRuleIds =
        new HashSet<string>();

    private static readonly DialogueMessage[] emptyDialogueHistory =
        new DialogueMessage[0];

    private ScenarioDefinition activeScenario;
    private bool isInitialized;

    public IReadOnlyList<ClueDefinition> DiscoveredClues =>
        discoveredClues;

    public ScenarioDefinition ActiveScenario =>
        activeScenario;

    public bool IsInitialized =>
        isInitialized;

    public bool InitializeScenario(
        ScenarioDefinition scenario)
    {
        if (scenario == null)
        {
            Debug.LogError(
                "CaseState cannot initialize with a null ScenarioDefinition.");

            return false;
        }

        if (isInitialized)
        {
            if (activeScenario == scenario)
            {
                return true;
            }

            Debug.LogWarning(
                "CaseState is already initialized with scenario \"" +
                activeScenario.name +
                "\". It will not be replaced by \"" +
                scenario.name +
                "\".");

            return false;
        }

        activeScenario = scenario;
        isInitialized = true;

        Debug.Log(
            "Case initialized: " +
            activeScenario.Title +
            " (" +
            activeScenario.ScenarioId +
            ")");

        return true;
    }

    public bool TryDiscoverClue(
        ClueDefinition clue)
    {
        if (clue == null)
        {
            Debug.LogWarning(
                "Cannot discover a null clue.");

            return false;
        }

        foreach (
            ClueDefinition discoveredClue
            in discoveredClues)
        {
            if (discoveredClue.ClueId ==
                clue.ClueId)
            {
                return false;
            }
        }

        discoveredClues.Add(clue);

        Debug.Log(
            "Clue discovered: " +
            clue.DisplayName);

        return true;
    }

    public bool IsContainerSearched(
        string containerId,
        bool initiallyLocked)
    {
        ContainerRuntimeState state =
            GetOrCreateContainerState(
                containerId,
                initiallyLocked);

        if (state == null)
        {
            return false;
        }

        return state.IsSearched;
    }

    public bool IsContainerLocked(
        string containerId,
        bool initiallyLocked)
    {
        ContainerRuntimeState state =
            GetOrCreateContainerState(
                containerId,
                initiallyLocked);

        if (state == null)
        {
            return initiallyLocked;
        }

        return state.IsLocked;
    }

    public void MarkContainerSearched(
        string containerId,
        bool initiallyLocked)
    {
        ContainerRuntimeState state =
            GetOrCreateContainerState(
                containerId,
                initiallyLocked);

        if (state == null)
        {
            return;
        }

        state.MarkSearched();
    }

    public void UnlockContainer(
        string containerId,
        bool initiallyLocked)
    {
        ContainerRuntimeState state =
            GetOrCreateContainerState(
                containerId,
                initiallyLocked);

        if (state == null)
        {
            return;
        }

        state.Unlock();
    }

    public IReadOnlyList<DialogueMessage>
        GetNpcConversationHistory(
            string npcId)
    {
        NpcDialogueRuntimeState state =
            GetOrCreateNpcDialogueState(
                npcId);

        if (state == null)
        {
            return emptyDialogueHistory;
        }

        return state.ConversationHistory;
    }

    public int GetNpcQuestionsUsed(
        string npcId)
    {
        NpcDialogueRuntimeState state =
            GetOrCreateNpcDialogueState(
                npcId);

        if (state == null)
        {
            return 0;
        }

        return state.QuestionsUsed;
    }

    public int GetNpcPlayerCharactersUsed(
        string npcId)
    {
        NpcDialogueRuntimeState state =
            GetOrCreateNpcDialogueState(
                npcId);

        if (state == null)
        {
            return 0;
        }

        return state.PlayerCharactersUsed;
    }

    public bool TryAddNpcPlayerMessage(
        string npcId,
        string message,
        int maxQuestions,
        int maxPlayerCharacters)
    {
        if (string.IsNullOrWhiteSpace(
                message))
        {
            return false;
        }

        NpcDialogueRuntimeState state =
            GetOrCreateNpcDialogueState(
                npcId);

        if (state == null)
        {
            return false;
        }

        string trimmedMessage =
            message.Trim();

        int questionsRemaining =
            Mathf.Max(
                0,
                maxQuestions -
                state.QuestionsUsed);

        int charactersRemaining =
            Mathf.Max(
                0,
                maxPlayerCharacters -
                state.PlayerCharactersUsed);

        if (questionsRemaining <= 0 ||
            charactersRemaining <= 0)
        {
            return false;
        }

        if (trimmedMessage.Length >
            charactersRemaining)
        {
            return false;
        }

        state.AddPlayerMessage(
            trimmedMessage);

        return true;
    }

    public void AddNpcDialogueMessage(
        string npcId,
        string npcDisplayName,
        string message)
    {
        NpcDialogueRuntimeState state =
            GetOrCreateNpcDialogueState(
                npcId);

        if (state == null)
        {
            return;
        }

        state.AddNpcMessage(
            npcDisplayName,
            message);
    }

    public bool CompleteNpcInitialConversationIfMeaningful(
        string npcId)
    {
        NpcDialogueRuntimeState state =
            GetOrCreateNpcDialogueState(
                npcId);

        if (state == null)
        {
            return false;
        }

        if (state.InitialConversationCompleted)
        {
            return false;
        }

        if (!state.HasReceivedPlayerMessage)
        {
            return false;
        }

        state.MarkInitialConversationCompleted();

        Debug.Log(
            "Initial meaningful conversation completed for NPC: " +
            npcId);

        EvaluateNpcProgressionRules(
            npcId);

        return true;
    }


    public bool TryGetNpcInitialConversationReveal(
    string npcId,
    out string revealText)
    {
        revealText = string.Empty;

        if (string.IsNullOrWhiteSpace(npcId))
        {
            return false;
        }

        if (activeScenario == null)
        {
            return false;
        }

        NpcDialogueRuntimeState state =
            GetOrCreateNpcDialogueState(
                npcId);

        if (state == null)
        {
            return false;
        }

        // Do not show the reveal again after this NPC's
        // initial conversation progression has completed.
        if (state.InitialConversationCompleted)
        {
            return false;
        }

        // A provider failure never commits the player's message.
        // Therefore a failed AI request cannot trigger the reveal.
        if (!state.HasReceivedPlayerMessage)
        {
            return false;
        }

        ScenarioDefinition.ProgressionRule[] rules =
            activeScenario.ProgressionRules;

        if (rules == null)
        {
            return false;
        }

        foreach (
            ScenarioDefinition.ProgressionRule rule
            in rules)
        {
            if (rule == null)
            {
                continue;
            }

            if (completedProgressionRuleIds.Contains(
                    rule.RuleId))
            {
                continue;
            }

            if (rule.TriggerNpc == null)
            {
                continue;
            }

            if (rule.TriggerNpc.NpcId != npcId)
            {
                continue;
            }

            if (!rule.RequireInitialMeaningfulConversation)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(
                    rule.AuthoredClosingReveal))
            {
                continue;
            }

            revealText =
                rule.AuthoredClosingReveal.Trim();

            return true;
        }

        return false;
    }

    public bool IsNpcInitialConversationCompleted(
        string npcId)
    {
        NpcDialogueRuntimeState state =
            GetOrCreateNpcDialogueState(
                npcId);

        if (state == null)
        {
            return false;
        }

        return state
            .InitialConversationCompleted;
    }

    public bool IsProgressionRuleCompleted(
        string ruleId)
    {
        if (string.IsNullOrWhiteSpace(
                ruleId))
        {
            return false;
        }

        return completedProgressionRuleIds.Contains(
            ruleId);
    }

    private void EvaluateNpcProgressionRules(
        string npcId)
    {
        if (activeScenario == null)
        {
            Debug.LogWarning(
                "Cannot evaluate NPC progression because " +
                "there is no active scenario.");

            return;
        }

        ScenarioDefinition.ProgressionRule[] rules =
            activeScenario.ProgressionRules;

        if (rules == null)
        {
            return;
        }

        foreach (
            ScenarioDefinition.ProgressionRule rule
            in rules)
        {
            if (rule == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(
                    rule.RuleId))
            {
                Debug.LogWarning(
                    "Scenario \"" +
                    activeScenario.name +
                    "\" contains a progression rule " +
                    "with an empty Rule ID.");

                continue;
            }

            if (completedProgressionRuleIds.Contains(
                    rule.RuleId))
            {
                continue;
            }

            NpcDefinition triggerNpc =
                rule.TriggerNpc;

            if (triggerNpc == null)
            {
                Debug.LogWarning(
                    "Progression rule \"" +
                    rule.RuleId +
                    "\" has no Trigger NPC.");

                continue;
            }

            if (triggerNpc.NpcId != npcId)
            {
                continue;
            }

            if (rule.RequireInitialMeaningfulConversation &&
                !IsNpcInitialConversationCompleted(
                    npcId))
            {
                continue;
            }

            ExecuteProgressionRule(
                rule);
        }
    }

    private void ExecuteProgressionRule(
        ScenarioDefinition.ProgressionRule rule)
    {
        if (rule.UnlockRoom)
        {
            if (RoomNavigationState.Instance == null)
            {
                Debug.LogWarning(
                    "Progression rule \"" +
                    rule.RuleId +
                    "\" wanted to unlock a room, but " +
                    "RoomNavigationState.Instance is null.");

                return;
            }

            RoomNavigationState.Instance.UnlockRoom(
                rule.RoomToUnlock);

            Debug.Log(
                "Progression rule \"" +
                rule.RuleId +
                "\" unlocked room: " +
                rule.RoomToUnlock);
        }

        if (rule.DiscoverClue)
        {
            if (rule.ClueToDiscover == null)
            {
                Debug.LogWarning(
                    "Progression rule \"" +
                    rule.RuleId +
                    "\" is configured to discover a clue, " +
                    "but Clue To Discover is null.");

                return;
            }

            TryDiscoverClue(
                rule.ClueToDiscover);
        }

        completedProgressionRuleIds.Add(
            rule.RuleId);

        Debug.Log(
            "Progression rule completed: " +
            rule.RuleId);
    }

    private ContainerRuntimeState
        GetOrCreateContainerState(
            string containerId,
            bool initiallyLocked)
    {
        if (string.IsNullOrWhiteSpace(
                containerId))
        {
            Debug.LogWarning(
                "Cannot access persistent container state because " +
                "the Container ID is empty.");

            return null;
        }

        if (containerStates.TryGetValue(
                containerId,
                out ContainerRuntimeState
                    existingState))
        {
            return existingState;
        }

        ContainerRuntimeState newState =
            new ContainerRuntimeState(
                initiallyLocked);

        containerStates.Add(
            containerId,
            newState);

        return newState;
    }

    private NpcDialogueRuntimeState
        GetOrCreateNpcDialogueState(
            string npcId)
    {
        if (string.IsNullOrWhiteSpace(
                npcId))
        {
            Debug.LogWarning(
                "Cannot access persistent NPC dialogue state because " +
                "the NPC ID is empty.");

            return null;
        }

        if (npcDialogueStates.TryGetValue(
                npcId,
                out NpcDialogueRuntimeState
                    existingState))
        {
            return existingState;
        }

        NpcDialogueRuntimeState newState =
            new NpcDialogueRuntimeState();

        npcDialogueStates.Add(
            npcId,
            newState);

        return newState;
    }

    private class ContainerRuntimeState
    {
        public bool IsSearched
        {
            get;
            private set;
        }

        public bool IsLocked
        {
            get;
            private set;
        }

        public ContainerRuntimeState(
            bool initiallyLocked)
        {
            IsSearched = false;
            IsLocked = initiallyLocked;
        }

        public void MarkSearched()
        {
            IsSearched = true;
        }

        public void Unlock()
        {
            IsLocked = false;
        }
    }

    private class NpcDialogueRuntimeState
    {
        private readonly List<DialogueMessage>
            conversationHistory =
                new List<DialogueMessage>();

        public IReadOnlyList<DialogueMessage>
            ConversationHistory =>
                conversationHistory;

        public int QuestionsUsed
        {
            get;
            private set;
        }

        public int PlayerCharactersUsed
        {
            get;
            private set;
        }

        public bool HasReceivedPlayerMessage
        {
            get;
            private set;
        }

        public bool InitialConversationCompleted
        {
            get;
            private set;
        }

        public void AddPlayerMessage(
            string message)
        {
            conversationHistory.Add(
                new DialogueMessage(
                    "Detective",
                    message));

            QuestionsUsed++;

            PlayerCharactersUsed +=
                message.Length;

            HasReceivedPlayerMessage = true;
        }

        public void AddNpcMessage(
            string npcDisplayName,
            string message)
        {
            conversationHistory.Add(
                new DialogueMessage(
                    npcDisplayName,
                    message));
        }

        public void MarkInitialConversationCompleted()
        {
            InitialConversationCompleted = true;
        }
    }
}