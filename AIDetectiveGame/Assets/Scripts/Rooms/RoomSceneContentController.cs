using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class RoomSceneContentController : MonoBehaviour
{
    [Header("Room Data")]

    [SerializeField]
    private RoomContentDefinition roomContentDefinition;

    [SerializeField]
    private RoomSceneController roomSceneController;

    [Header("Scene Content")]

    [SerializeField]
    private Transform roomContentRoot;

    [SerializeField]
    private Transform runtimeSpawnedContent;

    [Header("Runtime References")]

    [SerializeField]
    private CaseState caseState;

    [SerializeField]
    private ContainerSearchUI containerSearchUI;

    [SerializeField]
    private ClueInspectionUI clueInspectionUI;

    [SerializeField]
    private DialogueUI dialogueUI;

    private readonly Dictionary<string, ClueSpawnPoint>
        clueSpawnPoints =
            new Dictionary<string, ClueSpawnPoint>();

    private readonly Dictionary<string, ContainerSpawnPoint>
        containerSpawnPoints =
            new Dictionary<string, ContainerSpawnPoint>();

    private readonly Dictionary<string, NpcSpawnPoint>
        npcSpawnPoints =
            new Dictionary<string, NpcSpawnPoint>();

    private readonly Dictionary<string, EnvironmentVariantGroup>
        environmentVariantGroups =
            new Dictionary<string, EnvironmentVariantGroup>();

    private readonly HashSet<string> duplicateClueSpawnIds =
        new HashSet<string>();

    private readonly HashSet<string> duplicateContainerSpawnIds =
        new HashSet<string>();

    private readonly HashSet<string> duplicateNpcSpawnIds =
        new HashSet<string>();

    private readonly HashSet<string> duplicateVariantGroupIds =
        new HashSet<string>();

    private void Start()
    {
        ResolvePersistentCaseState();

        if (!ValidateMainReferences())
        {
            return;
        }

        RegisterSceneContent();

        if (!ValidateRoomMatch())
        {
            return;
        }

        ApplyEnvironmentVariants();
        SpawnContainers();
        SpawnClues();
        SpawnNpcs();
    }

    private void ResolvePersistentCaseState()
    {
        if (RoomNavigationState.Instance == null)
        {
            return;
        }

        CaseState persistentCaseState =
            RoomNavigationState.Instance
                .GetComponent<CaseState>();

        if (persistentCaseState != null)
        {
            caseState =
                persistentCaseState;
        }
    }

    private bool ValidateMainReferences()
    {
        bool valid = true;

        if (roomContentDefinition == null)
        {
            Debug.LogError(
                $"RoomSceneContentController on \"{gameObject.name}\" " +
                "has no RoomContentDefinition assigned.");

            valid = false;
        }

        if (roomSceneController == null)
        {
            Debug.LogError(
                $"RoomSceneContentController on \"{gameObject.name}\" " +
                "has no RoomSceneController assigned.");

            valid = false;
        }

        if (roomContentRoot == null)
        {
            Debug.LogError(
                $"RoomSceneContentController on \"{gameObject.name}\" " +
                "has no Room Content Root assigned.");

            valid = false;
        }

        if (runtimeSpawnedContent == null)
        {
            Debug.LogError(
                $"RoomSceneContentController on \"{gameObject.name}\" " +
                "has no Runtime Spawned Content parent assigned.");

            valid = false;
        }

        if (caseState == null)
        {
            Debug.LogError(
                $"RoomSceneContentController on \"{gameObject.name}\" " +
                "could not find the persistent CaseState.");

            valid = false;
        }

        return valid;
    }

    private bool ValidateRoomMatch()
    {
        if (roomContentDefinition.RoomId ==
            roomSceneController.RoomId)
        {
            return true;
        }

        Debug.LogError(
            $"Room content mismatch: scene is " +
            $"\"{roomSceneController.RoomId}\" but assigned definition " +
            $"\"{roomContentDefinition.name}\" is for " +
            $"\"{roomContentDefinition.RoomId}\".");

        return false;
    }

    private void RegisterSceneContent()
    {
        RegisterClueSpawnPoints();
        RegisterContainerSpawnPoints();
        RegisterNpcSpawnPoints();
        RegisterEnvironmentVariantGroups();
    }

    private void RegisterClueSpawnPoints()
    {
        clueSpawnPoints.Clear();
        duplicateClueSpawnIds.Clear();

        ClueSpawnPoint[] points =
            roomContentRoot
                .GetComponentsInChildren<ClueSpawnPoint>(
                    true);

        foreach (ClueSpawnPoint point in points)
        {
            string id =
                point.SpawnPointId;

            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogError(
                    $"ClueSpawnPoint on \"{point.gameObject.name}\" " +
                    "has an empty Spawn Point ID.");

                continue;
            }

            if (clueSpawnPoints.ContainsKey(id) ||
                duplicateClueSpawnIds.Contains(id))
            {
                Debug.LogError(
                    $"Duplicate ClueSpawnPoint ID \"{id}\" found in " +
                    $"room \"{roomSceneController.RoomId}\". " +
                    "Spawn-point IDs must be unique.");

                clueSpawnPoints.Remove(id);
                duplicateClueSpawnIds.Add(id);

                continue;
            }

            clueSpawnPoints.Add(
                id,
                point);
        }
    }

    private void RegisterContainerSpawnPoints()
    {
        containerSpawnPoints.Clear();
        duplicateContainerSpawnIds.Clear();

        ContainerSpawnPoint[] points =
            roomContentRoot
                .GetComponentsInChildren<ContainerSpawnPoint>(
                    true);

        foreach (
            ContainerSpawnPoint point
            in points)
        {
            string id =
                point.SpawnPointId;

            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogError(
                    $"ContainerSpawnPoint on \"{point.gameObject.name}\" " +
                    "has an empty Spawn Point ID.");

                continue;
            }

            if (containerSpawnPoints.ContainsKey(id) ||
                duplicateContainerSpawnIds.Contains(id))
            {
                Debug.LogError(
                    $"Duplicate ContainerSpawnPoint ID \"{id}\" found in " +
                    $"room \"{roomSceneController.RoomId}\". " +
                    "Spawn-point IDs must be unique.");

                containerSpawnPoints.Remove(id);
                duplicateContainerSpawnIds.Add(id);

                continue;
            }

            containerSpawnPoints.Add(
                id,
                point);
        }
    }

    private void RegisterNpcSpawnPoints()
    {
        npcSpawnPoints.Clear();
        duplicateNpcSpawnIds.Clear();

        NpcSpawnPoint[] points =
            roomContentRoot
                .GetComponentsInChildren<NpcSpawnPoint>(
                    true);

        foreach (
            NpcSpawnPoint point
            in points)
        {
            string id =
                point.SpawnPointId;

            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogError(
                    $"NpcSpawnPoint on \"{point.gameObject.name}\" " +
                    "has an empty Spawn Point ID.");

                continue;
            }

            if (npcSpawnPoints.ContainsKey(id) ||
                duplicateNpcSpawnIds.Contains(id))
            {
                Debug.LogError(
                    $"Duplicate NpcSpawnPoint ID \"{id}\" found in " +
                    $"room \"{roomSceneController.RoomId}\". " +
                    "Spawn-point IDs must be unique.");

                npcSpawnPoints.Remove(id);
                duplicateNpcSpawnIds.Add(id);

                continue;
            }

            npcSpawnPoints.Add(
                id,
                point);
        }
    }

    private void RegisterEnvironmentVariantGroups()
    {
        environmentVariantGroups.Clear();
        duplicateVariantGroupIds.Clear();

        EnvironmentVariantGroup[] groups =
            roomContentRoot
                .GetComponentsInChildren<EnvironmentVariantGroup>(
                    true);

        foreach (
            EnvironmentVariantGroup group
            in groups)
        {
            string id =
                group.GroupId;

            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogError(
                    $"EnvironmentVariantGroup on " +
                    $"\"{group.gameObject.name}\" has an empty Group ID.");

                continue;
            }

            if (environmentVariantGroups.ContainsKey(id) ||
                duplicateVariantGroupIds.Contains(id))
            {
                Debug.LogError(
                    $"Duplicate EnvironmentVariantGroup ID \"{id}\" " +
                    $"found in room \"{roomSceneController.RoomId}\".");

                environmentVariantGroups.Remove(id);
                duplicateVariantGroupIds.Add(id);

                continue;
            }

            environmentVariantGroups.Add(
                id,
                group);
        }
    }

    private void SpawnContainers()
    {
        RoomContentDefinition.ContainerPlacement[] placements =
            roomContentDefinition.ContainerPlacements;

        if (placements == null)
        {
            return;
        }

        foreach (
            RoomContentDefinition.ContainerPlacement placement
            in placements)
        {
            if (placement == null)
            {
                continue;
            }

            string spawnId =
                placement.SpawnPointId;

            if (string.IsNullOrWhiteSpace(spawnId))
            {
                Debug.LogError(
                    $"{roomContentDefinition.name} contains a container " +
                    "placement with an empty Spawn Point ID.");

                continue;
            }

            if (placement.ContainerPrefab == null)
            {
                Debug.LogError(
                    $"{roomContentDefinition.name} references container " +
                    $"spawn point \"{spawnId}\", but its Container Prefab " +
                    "is null.");

                continue;
            }

            if (duplicateContainerSpawnIds.Contains(
                    spawnId))
            {
                Debug.LogError(
                    $"{roomContentDefinition.name} references container " +
                    $"spawn point \"{spawnId}\", but that ID is duplicated " +
                    "in the scene.");

                continue;
            }

            if (!containerSpawnPoints.TryGetValue(
                    spawnId,
                    out ContainerSpawnPoint spawnPoint))
            {
                Debug.LogError(
                    $"{roomContentDefinition.name} references container " +
                    $"spawn point \"{spawnId}\", but no " +
                    "ContainerSpawnPoint with that ID exists.");

                continue;
            }

            if (containerSearchUI == null)
            {
                Debug.LogError(
                    $"Cannot spawn container \"{placement.ContainerPrefab.name}\" " +
                    "because RoomSceneContentController has no " +
                    "ContainerSearchUI assigned.");

                continue;
            }

            GameObject instance =
                Instantiate(
                    placement.ContainerPrefab,
                    spawnPoint.transform.position,
                    spawnPoint.transform.rotation,
                    runtimeSpawnedContent);

            instance.transform.localScale =
                Vector3.Scale(
                    instance.transform.localScale,
                    spawnPoint.transform.localScale);

            SearchableContainer container =
                instance
                    .GetComponentInChildren<SearchableContainer>(
                        true);

            if (container == null)
            {
                Debug.LogError(
                    $"Spawned container prefab \"{placement.ContainerPrefab.name}\" " +
                    "does not contain a SearchableContainer component.");

                Destroy(instance);
                continue;
            }

            container.InitializeRuntime(
                caseState,
                containerSearchUI);
        }
    }

    private void SpawnClues()
    {
        RoomContentDefinition.CluePlacement[] placements =
            roomContentDefinition.CluePlacements;

        if (placements == null)
        {
            return;
        }

        foreach (
            RoomContentDefinition.CluePlacement placement
            in placements)
        {
            if (placement == null)
            {
                continue;
            }

            string spawnId =
                placement.SpawnPointId;

            if (string.IsNullOrWhiteSpace(spawnId))
            {
                Debug.LogError(
                    $"{roomContentDefinition.name} contains a clue " +
                    "placement with an empty Spawn Point ID.");

                continue;
            }

            if (placement.WorldCluePrefab == null)
            {
                Debug.LogError(
                    $"{roomContentDefinition.name} references clue spawn " +
                    $"point \"{spawnId}\", but its World Clue Prefab is null.");

                continue;
            }

            if (duplicateClueSpawnIds.Contains(
                    spawnId))
            {
                Debug.LogError(
                    $"{roomContentDefinition.name} references clue spawn " +
                    $"point \"{spawnId}\", but that ID is duplicated " +
                    "in the scene.");

                continue;
            }

            if (!clueSpawnPoints.TryGetValue(
                    spawnId,
                    out ClueSpawnPoint spawnPoint))
            {
                Debug.LogError(
                    $"{roomContentDefinition.name} references clue spawn " +
                    $"point \"{spawnId}\", but no ClueSpawnPoint " +
                    "with that ID exists.");

                continue;
            }

            if (clueInspectionUI == null)
            {
                Debug.LogError(
                    $"Cannot spawn clue \"{placement.WorldCluePrefab.name}\" " +
                    "because RoomSceneContentController has no " +
                    "ClueInspectionUI assigned.");

                continue;
            }

            GameObject instance =
                Instantiate(
                    placement.WorldCluePrefab,
                    spawnPoint.transform.position,
                    spawnPoint.transform.rotation,
                    runtimeSpawnedContent);

            instance.transform.localScale =
                Vector3.Scale(
                    instance.transform.localScale,
                    spawnPoint.transform.localScale);

            ClueInteractable clue =
                instance
                    .GetComponentInChildren<ClueInteractable>(
                        true);

            if (clue == null)
            {
                Debug.LogError(
                    $"Spawned clue prefab \"{placement.WorldCluePrefab.name}\" " +
                    "does not contain a ClueInteractable component.");

                Destroy(instance);
                continue;
            }

            clue.InitializeRuntime(
                caseState,
                clueInspectionUI);
        }
    }

    private void SpawnNpcs()
    {
        ScenarioDefinition scenario =
            caseState.ActiveScenario;

        if (scenario == null)
        {
            Debug.LogError(
                "Cannot spawn NPCs because CaseState has no active scenario.");

            return;
        }

        ScenarioDefinition.ScenarioNpcProfile[] profiles =
            scenario.NpcProfiles;

        if (profiles == null)
        {
            return;
        }

        foreach (
            ScenarioDefinition.ScenarioNpcProfile profile
            in profiles)
        {
            if (profile == null)
            {
                continue;
            }

            if (profile.StartingRoom !=
                roomSceneController.RoomId)
            {
                continue;
            }

            NpcDefinition npcDefinition =
                profile.NpcDefinition;

            if (npcDefinition == null)
            {
                Debug.LogError(
                    $"Scenario \"{scenario.name}\" contains an NPC profile " +
                    $"for room \"{roomSceneController.RoomId}\" " +
                    "with no NpcDefinition assigned.");

                continue;
            }

            string spawnId =
                profile.SpawnPointId;

            if (string.IsNullOrWhiteSpace(spawnId))
            {
                Debug.LogError(
                    $"NPC \"{npcDefinition.DisplayName}\" has an empty " +
                    "Spawn Point ID.");

                continue;
            }

            if (npcDefinition.WorldPrefab == null)
            {
                Debug.LogError(
                    $"NPC \"{npcDefinition.DisplayName}\" has no World Prefab.");

                continue;
            }

            if (duplicateNpcSpawnIds.Contains(
                    spawnId))
            {
                Debug.LogError(
                    $"NPC \"{npcDefinition.DisplayName}\" references " +
                    $"spawn point \"{spawnId}\", but that ID is duplicated " +
                    "in the scene.");

                continue;
            }

            if (!npcSpawnPoints.TryGetValue(
                    spawnId,
                    out NpcSpawnPoint spawnPoint))
            {
                Debug.LogError(
                    $"NPC \"{npcDefinition.DisplayName}\" references " +
                    $"spawn point \"{spawnId}\", but no matching " +
                    "NpcSpawnPoint exists in room " +
                    $"\"{roomSceneController.RoomId}\".");

                continue;
            }

            if (dialogueUI == null)
            {
                Debug.LogError(
                    $"Cannot spawn NPC \"{npcDefinition.DisplayName}\" " +
                    "because RoomSceneContentController has no " +
                    "DialogueUI assigned.");

                continue;
            }

            GameObject instance =
                Instantiate(
                    npcDefinition.WorldPrefab,
                    spawnPoint.transform.position,
                    spawnPoint.transform.rotation,
                    runtimeSpawnedContent);

            instance.transform.localScale =
                Vector3.Scale(
                    instance.transform.localScale,
                    spawnPoint.transform.localScale);

            NpcDialogue npcDialogue =
                instance.GetComponent<NpcDialogue>();

            if (npcDialogue == null)
            {
                Debug.LogError(
                    $"Spawned NPC prefab \"{npcDefinition.WorldPrefab.name}\" " +
                    "does not contain NpcDialogue on its root.");

                Destroy(instance);
                continue;
            }

            NpcInteractable npcInteractable =
                instance.GetComponent<NpcInteractable>();

            if (npcInteractable == null)
            {
                Debug.LogError(
                    $"Spawned NPC prefab \"{npcDefinition.WorldPrefab.name}\" " +
                    "does not contain NpcInteractable on its root.");

                Destroy(instance);
                continue;
            }

            npcDialogue.InitializeRuntime(
                npcDefinition,
                profile.TemporaryHardCodedResponse,
                caseState);

            npcInteractable.InitializeRuntime(
                dialogueUI);

            instance.name =
                npcDefinition.DisplayName;
        }
    }

    private void ApplyEnvironmentVariants()
    {
        RoomContentDefinition.EnvironmentVariantSelection[]
            selections =
                roomContentDefinition
                    .EnvironmentVariantSelections;

        if (selections == null)
        {
            return;
        }

        foreach (
            RoomContentDefinition.EnvironmentVariantSelection selection
            in selections)
        {
            if (selection == null)
            {
                continue;
            }

            string groupId =
                selection.VariantGroupId;

            if (string.IsNullOrWhiteSpace(
                    groupId))
            {
                Debug.LogError(
                    $"{roomContentDefinition.name} contains an environment " +
                    "variant selection with an empty Variant Group ID.");

                continue;
            }

            if (duplicateVariantGroupIds.Contains(
                    groupId))
            {
                Debug.LogError(
                    $"{roomContentDefinition.name} references environment " +
                    $"variant group \"{groupId}\", but that ID is duplicated.");

                continue;
            }

            if (!environmentVariantGroups.TryGetValue(
                    groupId,
                    out EnvironmentVariantGroup group))
            {
                Debug.LogError(
                    $"{roomContentDefinition.name} references environment " +
                    $"variant group \"{groupId}\", but no matching " +
                    "EnvironmentVariantGroup exists.");

                continue;
            }

            if (!selection.Enabled)
            {
                group.SetGroupEnabled(
                    false);

                continue;
            }

            if (string.IsNullOrWhiteSpace(
                    selection.VariantId))
            {
                Debug.LogError(
                    $"{roomContentDefinition.name} enables environment " +
                    $"variant group \"{groupId}\" but provides no Variant ID.");

                continue;
            }

            group.SetVariant(
                selection.VariantId);
        }
    }
}