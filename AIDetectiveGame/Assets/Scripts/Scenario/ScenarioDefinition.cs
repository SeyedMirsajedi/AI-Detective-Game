using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "Scenario_New",
    menuName = "AI Detective/Scenario/Scenario Definition")]
public class ScenarioDefinition : ScriptableObject
{
    [Serializable]
    public class InitialRoomAccessEntry
    {
        [SerializeField]
        private RoomId roomId;

        [SerializeField]
        private bool initiallyUnlocked;

        public RoomId RoomId => roomId;

        public bool InitiallyUnlocked => initiallyUnlocked;
    }

    [Serializable]
    public class ScenarioNpcProfile
    {
        [Header("NPC")]

        [SerializeField]
        private NpcDefinition npcDefinition;

        [Header("Starting Placement")]

        [SerializeField]
        private RoomId startingRoom;

        [SerializeField]
        private string spawnPointId;

        [Header("Knowledge")]

        [SerializeField]
        private CaseFactDefinition[] knownFacts;

        [SerializeField]
        private CaseFactDefinition[] privateFacts;

        [SerializeField]
        private CaseFactDefinition[] falseBeliefs;

        [Header("Dialogue Behaviour")]

        [SerializeField]
        [TextArea(2, 5)]
        private string allowedLieOrEvasionNotes;

        [SerializeField]
        [TextArea(2, 5)]
        private string temporaryHardCodedResponse;

        public NpcDefinition NpcDefinition =>
            npcDefinition;

        public RoomId StartingRoom =>
            startingRoom;

        public string SpawnPointId =>
            spawnPointId;

        public CaseFactDefinition[] KnownFacts =>
            knownFacts;

        public CaseFactDefinition[] PrivateFacts =>
            privateFacts;

        public CaseFactDefinition[] FalseBeliefs =>
            falseBeliefs;

        public string AllowedLieOrEvasionNotes =>
            allowedLieOrEvasionNotes;

        public string TemporaryHardCodedResponse =>
            temporaryHardCodedResponse;
    }

    [Serializable]
    public class ProgressionRule
    {
        [Header("Rule Identity")]

        [SerializeField]
        private string ruleId;

        [Header("Trigger")]

        [SerializeField]
        private NpcDefinition triggerNpc;

        [SerializeField]
        private bool requireInitialMeaningfulConversation;

        [Header("Authored Story Reveal")]

        [SerializeField]
        [TextArea(3, 6)]
        private string authoredClosingReveal;

        [Header("Actions")]

        [SerializeField]
        private bool unlockRoom;

        [SerializeField]
        private RoomId roomToUnlock;

        [SerializeField]
        private bool discoverClue;

        [SerializeField]
        private ClueDefinition clueToDiscover;

        public string RuleId => ruleId;

        public NpcDefinition TriggerNpc =>
            triggerNpc;

        public bool RequireInitialMeaningfulConversation =>
            requireInitialMeaningfulConversation;

        public string AuthoredClosingReveal =>
            authoredClosingReveal;

        public bool UnlockRoom =>
            unlockRoom;

        public RoomId RoomToUnlock =>
            roomToUnlock;

        public bool DiscoverClue =>
            discoverClue;

        public ClueDefinition ClueToDiscover =>
            clueToDiscover;
    }

    [Header("Scenario Identity")]

    [SerializeField]
    private string scenarioId;

    [SerializeField]
    private string title;

    [Header("Case Truth")]

    [SerializeField]
    private string victimName;

    [SerializeField]
    [Min(0)]
    private int victimAge;

    [SerializeField]
    private NpcDefinition culprit;

    [SerializeField]
    [TextArea(2, 5)]
    private string motive;

    [SerializeField]
    [TextArea(2, 4)]
    private string weaponAndMethod;

    [SerializeField]
    private RoomId crimeRoom;

    [SerializeField]
    private string crimeWindow;

    [Header("Authoritative Facts")]

    [SerializeField]
    private CaseFactDefinition[] caseFacts;

    [Header("Required Evidence")]

    [SerializeField]
    private ClueDefinition[] requiredEvidence;

    [Header("Accusation Gate")]

    [SerializeField]
    private ClueDefinition[] accusationGateEvidence;

    [SerializeField]
    [Min(1)]
    private int minimumAccusationEvidenceCount = 2;

    [Header("Case Resolution")]

    [SerializeField]
    [TextArea(6, 12)]
    private string correctResolutionSummary;

    [SerializeField]
    [TextArea(4, 8)]
    private string failedResolutionSummary;

    [Header("Room Content")]

    [SerializeField]
    private RoomContentDefinition[] roomContentDefinitions;

    [Header("Initial Room Access")]

    [SerializeField]
    private InitialRoomAccessEntry[] initialRoomAccess;

    [Header("NPC Scenario Profiles")]

    [SerializeField]
    private ScenarioNpcProfile[] npcProfiles;

    [Header("Progression")]

    [SerializeField]
    private ProgressionRule[] progressionRules;

    public string ScenarioId => scenarioId;

    public string Title => title;

    public string VictimName => victimName;

    public int VictimAge => victimAge;

    public NpcDefinition Culprit => culprit;

    public string Motive => motive;

    public string WeaponAndMethod =>
        weaponAndMethod;

    public RoomId CrimeRoom => crimeRoom;

    public string CrimeWindow => crimeWindow;

    public CaseFactDefinition[] CaseFacts =>
        caseFacts;

    public ClueDefinition[] RequiredEvidence =>
        requiredEvidence;

    public ClueDefinition[] AccusationGateEvidence =>
        accusationGateEvidence;

    public int MinimumAccusationEvidenceCount =>
        minimumAccusationEvidenceCount;

    public string CorrectResolutionSummary =>
        correctResolutionSummary;

    public string FailedResolutionSummary =>
        failedResolutionSummary;

    public RoomContentDefinition[] RoomContentDefinitions =>
        roomContentDefinitions;

    public InitialRoomAccessEntry[] InitialRoomAccess =>
        initialRoomAccess;

    public ScenarioNpcProfile[] NpcProfiles =>
        npcProfiles;

    public ProgressionRule[] ProgressionRules =>
        progressionRules;

    public ScenarioNpcProfile GetNpcProfile(
        string npcId)
    {
        if (string.IsNullOrWhiteSpace(npcId) ||
            npcProfiles == null)
        {
            return null;
        }

        foreach (
            ScenarioNpcProfile profile
            in npcProfiles)
        {
            if (profile == null ||
                profile.NpcDefinition == null)
            {
                continue;
            }

            if (profile.NpcDefinition.NpcId ==
                npcId)
            {
                return profile;
            }
        }

        return null;
    }

    private void OnValidate()
    {
        ValidateScenario(false);
    }

    [ContextMenu("Validate Scenario")]
    private void ValidateScenarioFromContextMenu()
    {
        ValidateScenario(true);
    }

    private void ValidateScenario(bool logSuccess)
    {
        bool hasErrors = false;

        if (string.IsNullOrWhiteSpace(scenarioId))
        {
            LogValidationError(
                "Scenario ID is empty.");

            hasErrors = true;
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            LogValidationError(
                "Scenario title is empty.");

            hasErrors = true;
        }

        if (culprit == null)
        {
            LogValidationError(
                "Culprit is not assigned.");

            hasErrors = true;
        }

        hasErrors |= ValidateNpcProfiles();
        hasErrors |= ValidateRequiredEvidence();
        hasErrors |= ValidateAccusationGate();
        hasErrors |= ValidateCaseResolution();
        hasErrors |= ValidateProgressionRules();
        hasErrors |= ValidateInitialRoomAccess();

        if (!hasErrors && logSuccess)
        {
            Debug.Log(
                $"Scenario \"{title}\" passed validation.",
                this);
        }
    }

    private bool ValidateNpcProfiles()
    {
        bool hasErrors = false;

        if (npcProfiles == null ||
            npcProfiles.Length == 0)
        {
            LogValidationError(
                "NPC Scenario Profiles is empty.");

            return true;
        }

        HashSet<NpcDefinition> usedNpcDefinitions =
            new HashSet<NpcDefinition>();

        HashSet<string> usedPlacements =
            new HashSet<string>();

        bool culpritFound = false;

        for (int i = 0;
             i < npcProfiles.Length;
             i++)
        {
            ScenarioNpcProfile profile =
                npcProfiles[i];

            if (profile == null)
            {
                LogValidationError(
                    $"NPC Scenario Profile at index {i} is null.");

                hasErrors = true;
                continue;
            }

            NpcDefinition npc =
                profile.NpcDefinition;

            if (npc == null)
            {
                LogValidationError(
                    $"NPC Scenario Profile at index {i} has no NPC Definition.");

                hasErrors = true;
                continue;
            }

            if (!usedNpcDefinitions.Add(npc))
            {
                LogValidationError(
                    $"NPC Definition \"{npc.name}\" appears more than once in NPC Scenario Profiles.");

                hasErrors = true;
            }

            if (npc == culprit)
            {
                culpritFound = true;
            }

            if (string.IsNullOrWhiteSpace(
                    profile.SpawnPointId))
            {
                LogValidationError(
                    $"NPC \"{npc.name}\" has an empty Spawn Point ID.");

                hasErrors = true;
            }
            else
            {
                string placementKey =
                    $"{profile.StartingRoom}:{profile.SpawnPointId}";

                if (!usedPlacements.Add(
                        placementKey))
                {
                    LogValidationError(
                        $"Duplicate NPC placement detected in room " +
                        $"\"{profile.StartingRoom}\" at spawn point " +
                        $"\"{profile.SpawnPointId}\".");

                    hasErrors = true;
                }
            }
        }

        if (culprit != null &&
            !culpritFound)
        {
            LogValidationError(
                $"Culprit \"{culprit.name}\" does not exist in NPC Scenario Profiles.");

            hasErrors = true;
        }

        return hasErrors;
    }

    private bool ValidateRequiredEvidence()
    {
        bool hasErrors = false;

        if (requiredEvidence == null ||
            requiredEvidence.Length == 0)
        {
            LogValidationError(
                "Required Evidence is empty.");

            return true;
        }

        HashSet<ClueDefinition> usedEvidence =
            new HashSet<ClueDefinition>();

        for (int i = 0;
             i < requiredEvidence.Length;
             i++)
        {
            ClueDefinition clue =
                requiredEvidence[i];

            if (clue == null)
            {
                LogValidationError(
                    $"Required Evidence at index {i} is null.");

                hasErrors = true;
                continue;
            }

            if (!usedEvidence.Add(clue))
            {
                LogValidationError(
                    $"Required Evidence contains duplicate clue \"{clue.name}\".");

                hasErrors = true;
            }
        }

        return hasErrors;
    }

    private bool ValidateAccusationGate()
    {
        bool hasErrors = false;

        if (accusationGateEvidence == null ||
            accusationGateEvidence.Length == 0)
        {
            LogValidationError(
                "Accusation Gate Evidence is empty.");

            return true;
        }

        if (minimumAccusationEvidenceCount <= 0)
        {
            LogValidationError(
                "Minimum Accusation Evidence Count must be greater than zero.");

            hasErrors = true;
        }

        HashSet<ClueDefinition> usedGateEvidence =
            new HashSet<ClueDefinition>();

        HashSet<ClueDefinition> requiredEvidenceSet =
            new HashSet<ClueDefinition>();

        if (requiredEvidence != null)
        {
            foreach (
                ClueDefinition clue
                in requiredEvidence)
            {
                if (clue != null)
                {
                    requiredEvidenceSet.Add(clue);
                }
            }
        }

        for (int i = 0;
             i < accusationGateEvidence.Length;
             i++)
        {
            ClueDefinition clue =
                accusationGateEvidence[i];

            if (clue == null)
            {
                LogValidationError(
                    $"Accusation Gate Evidence at index {i} is null.");

                hasErrors = true;
                continue;
            }

            if (!usedGateEvidence.Add(clue))
            {
                LogValidationError(
                    $"Accusation Gate Evidence contains duplicate clue \"{clue.name}\".");

                hasErrors = true;
            }

            if (!requiredEvidenceSet.Contains(clue))
            {
                LogValidationError(
                    $"Accusation Gate Evidence clue \"{clue.name}\" is not present in Required Evidence.");

                hasErrors = true;
            }
        }

        if (minimumAccusationEvidenceCount >
            usedGateEvidence.Count)
        {
            LogValidationError(
                "Minimum Accusation Evidence Count is greater than the number of unique Accusation Gate Evidence clues.");

            hasErrors = true;
        }

        return hasErrors;
    }

    private bool ValidateCaseResolution()
    {
        bool hasErrors = false;

        if (string.IsNullOrWhiteSpace(
                correctResolutionSummary))
        {
            LogValidationError(
                "Correct Resolution Summary is empty.");

            hasErrors = true;
        }

        if (string.IsNullOrWhiteSpace(
                failedResolutionSummary))
        {
            LogValidationError(
                "Failed Resolution Summary is empty.");

            hasErrors = true;
        }

        return hasErrors;
    }

    private bool ValidateProgressionRules()
    {
        bool hasErrors = false;

        if (progressionRules == null)
        {
            return false;
        }

        HashSet<string> usedRuleIds =
            new HashSet<string>();

        for (int i = 0;
             i < progressionRules.Length;
             i++)
        {
            ProgressionRule rule =
                progressionRules[i];

            if (rule == null)
            {
                LogValidationError(
                    $"Progression Rule at index {i} is null.");

                hasErrors = true;
                continue;
            }

            if (string.IsNullOrWhiteSpace(
                    rule.RuleId))
            {
                LogValidationError(
                    $"Progression Rule at index {i} has an empty Rule ID.");

                hasErrors = true;
            }
            else if (!usedRuleIds.Add(
                         rule.RuleId))
            {
                LogValidationError(
                    $"Duplicate Progression Rule ID \"{rule.RuleId}\".");

                hasErrors = true;
            }

            if (rule.TriggerNpc == null)
            {
                LogValidationError(
                    $"Progression Rule \"{rule.RuleId}\" has no Trigger NPC.");

                hasErrors = true;
            }

            if (!rule.UnlockRoom &&
                !rule.DiscoverClue)
            {
                LogValidationError(
                    $"Progression Rule \"{rule.RuleId}\" has no actions.");

                hasErrors = true;
            }

            if (rule.DiscoverClue &&
                rule.ClueToDiscover == null)
            {
                LogValidationError(
                    $"Progression Rule \"{rule.RuleId}\" is configured to discover a clue, but Clue To Discover is null.");

                hasErrors = true;
            }
        }

        return hasErrors;
    }

    private bool ValidateInitialRoomAccess()
    {
        bool hasErrors = false;

        if (initialRoomAccess == null ||
            initialRoomAccess.Length == 0)
        {
            LogValidationError(
                "Initial Room Access is empty.");

            return true;
        }

        HashSet<RoomId> usedRooms =
            new HashSet<RoomId>();

        for (int i = 0;
             i < initialRoomAccess.Length;
             i++)
        {
            InitialRoomAccessEntry entry =
                initialRoomAccess[i];

            if (entry == null)
            {
                LogValidationError(
                    $"Initial Room Access entry at index {i} is null.");

                hasErrors = true;
                continue;
            }

            if (!usedRooms.Add(
                    entry.RoomId))
            {
                LogValidationError(
                    $"Initial Room Access contains duplicate room \"{entry.RoomId}\".");

                hasErrors = true;
            }
        }

        return hasErrors;
    }

    private void LogValidationError(
        string message)
    {
        Debug.LogError(
            $"Scenario Validation [{name}]: {message}",
            this);
    }
}