using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class CaseResolutionController : MonoBehaviour
{
    public enum AccusationResult
    {
        Unavailable,
        InvalidSuspect,
        Correct,
        Incorrect
    }

    [Header("Runtime References")]

    [SerializeField]
    private CaseState caseState;

    public ScenarioDefinition ActiveScenario
    {
        get
        {
            if (caseState == null)
            {
                return null;
            }

            return caseState.ActiveScenario;
        }
    }

    public int MeaningfulConversationCount =>
        GetMeaningfulConversationCount();

    public int RequiredConversationCount =>
        GetRequiredConversationCount();

    public int ImportantEvidenceCount =>
        GetImportantEvidenceCount();

    public int RequiredImportantEvidenceCount =>
        GetRequiredImportantEvidenceCount();

    public bool CanConcludeCase =>
        IsAccusationAvailable();

    private void Reset()
    {
        caseState =
            GetComponent<CaseState>();
    }

    private void Awake()
    {
        if (caseState == null)
        {
            caseState =
                GetComponent<CaseState>();
        }

        if (caseState == null)
        {
            Debug.LogError(
                "CaseResolutionController requires a CaseState " +
                "on the same GameObject.",
                this);
        }
    }

    public int GetMeaningfulConversationCount()
    {
        ScenarioDefinition scenario =
            ActiveScenario;

        if (caseState == null ||
            scenario == null ||
            scenario.NpcProfiles == null)
        {
            return 0;
        }

        HashSet<string> countedNpcIds =
            new HashSet<string>();

        int completedCount = 0;

        foreach (
            ScenarioDefinition.ScenarioNpcProfile profile
            in scenario.NpcProfiles)
        {
            if (profile == null ||
                profile.NpcDefinition == null)
            {
                continue;
            }

            string npcId =
                profile.NpcDefinition.NpcId;

            if (string.IsNullOrWhiteSpace(npcId))
            {
                continue;
            }

            if (!countedNpcIds.Add(npcId))
            {
                continue;
            }

            if (caseState
                .IsNpcInitialConversationCompleted(
                    npcId))
            {
                completedCount++;
            }
        }

        return completedCount;
    }

    public int GetRequiredConversationCount()
    {
        ScenarioDefinition scenario =
            ActiveScenario;

        if (scenario == null ||
            scenario.NpcProfiles == null)
        {
            return 0;
        }

        HashSet<string> requiredNpcIds =
            new HashSet<string>();

        foreach (
            ScenarioDefinition.ScenarioNpcProfile profile
            in scenario.NpcProfiles)
        {
            if (profile == null ||
                profile.NpcDefinition == null)
            {
                continue;
            }

            string npcId =
                profile.NpcDefinition.NpcId;

            if (string.IsNullOrWhiteSpace(npcId))
            {
                continue;
            }

            requiredNpcIds.Add(npcId);
        }

        return requiredNpcIds.Count;
    }

    public int GetImportantEvidenceCount()
    {
        ScenarioDefinition scenario =
            ActiveScenario;

        if (caseState == null ||
            scenario == null ||
            scenario.AccusationGateEvidence == null)
        {
            return 0;
        }

        HashSet<string> discoveredClueIds =
            new HashSet<string>();

        foreach (
            ClueDefinition discoveredClue
            in caseState.DiscoveredClues)
        {
            if (discoveredClue == null ||
                string.IsNullOrWhiteSpace(
                    discoveredClue.ClueId))
            {
                continue;
            }

            discoveredClueIds.Add(
                discoveredClue.ClueId);
        }

        HashSet<string> countedGateClueIds =
            new HashSet<string>();

        int discoveredGateCount = 0;

        foreach (
            ClueDefinition gateClue
            in scenario.AccusationGateEvidence)
        {
            if (gateClue == null ||
                string.IsNullOrWhiteSpace(
                    gateClue.ClueId))
            {
                continue;
            }

            if (!countedGateClueIds.Add(
                    gateClue.ClueId))
            {
                continue;
            }

            if (discoveredClueIds.Contains(
                    gateClue.ClueId))
            {
                discoveredGateCount++;
            }
        }

        return discoveredGateCount;
    }

    public int GetRequiredImportantEvidenceCount()
    {
        ScenarioDefinition scenario =
            ActiveScenario;

        if (scenario == null)
        {
            return 0;
        }

        return Mathf.Max(
            0,
            scenario.MinimumAccusationEvidenceCount);
    }

    public bool IsAccusationAvailable()
    {
        if (caseState == null ||
            ActiveScenario == null)
        {
            return false;
        }

        int requiredConversations =
            GetRequiredConversationCount();

        int requiredEvidence =
            GetRequiredImportantEvidenceCount();

        if (requiredConversations <= 0 ||
            requiredEvidence <= 0)
        {
            return false;
        }

        bool conversationsComplete =
            GetMeaningfulConversationCount() >=
            requiredConversations;

        bool evidenceComplete =
            GetImportantEvidenceCount() >=
            requiredEvidence;

        return conversationsComplete &&
               evidenceComplete;
    }

    public bool IsCorrectSuspect(
        NpcDefinition selectedSuspect)
    {
        ScenarioDefinition scenario =
            ActiveScenario;

        if (scenario == null ||
            scenario.Culprit == null ||
            selectedSuspect == null)
        {
            return false;
        }

        string selectedNpcId =
            selectedSuspect.NpcId;

        string culpritNpcId =
            scenario.Culprit.NpcId;

        if (string.IsNullOrWhiteSpace(
                selectedNpcId) ||
            string.IsNullOrWhiteSpace(
                culpritNpcId))
        {
            return false;
        }

        return selectedNpcId ==
               culpritNpcId;
    }

    public AccusationResult EvaluateAccusation(
        NpcDefinition selectedSuspect)
    {
        if (!IsAccusationAvailable())
        {
            return AccusationResult.Unavailable;
        }

        if (selectedSuspect == null ||
            !IsScenarioNpc(selectedSuspect))
        {
            return AccusationResult.InvalidSuspect;
        }

        if (IsCorrectSuspect(
                selectedSuspect))
        {
            return AccusationResult.Correct;
        }

        return AccusationResult.Incorrect;
    }

    private bool IsScenarioNpc(
        NpcDefinition npc)
    {
        ScenarioDefinition scenario =
            ActiveScenario;

        if (scenario == null ||
            scenario.NpcProfiles == null ||
            npc == null ||
            string.IsNullOrWhiteSpace(
                npc.NpcId))
        {
            return false;
        }

        foreach (
            ScenarioDefinition.ScenarioNpcProfile profile
            in scenario.NpcProfiles)
        {
            if (profile == null ||
                profile.NpcDefinition == null)
            {
                continue;
            }

            if (profile.NpcDefinition.NpcId ==
                npc.NpcId)
            {
                return true;
            }
        }

        return false;
    }

    [ContextMenu("Debug/Log Accusation Progress")]
    private void DebugLogAccusationProgress()
    {
        if (caseState == null)
        {
            Debug.LogWarning(
                "Accusation progress cannot be calculated " +
                "because CaseState is missing.",
                this);

            return;
        }

        if (ActiveScenario == null)
        {
            Debug.LogWarning(
                "Accusation progress cannot be calculated " +
                "because no active scenario is initialized.",
                this);

            return;
        }

        Debug.Log(
            "Accusation Progress\n" +
            "Interviews: " +
            GetMeaningfulConversationCount() +
            " / " +
            GetRequiredConversationCount() +
            "\nImportant Evidence: " +
            GetImportantEvidenceCount() +
            " / " +
            GetRequiredImportantEvidenceCount() +
            "\nConclude Case Available: " +
            IsAccusationAvailable(),
            this);
    }
}