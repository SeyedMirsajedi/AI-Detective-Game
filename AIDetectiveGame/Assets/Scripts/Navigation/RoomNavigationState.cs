using System.Collections.Generic;
using UnityEngine;

public class RoomNavigationState : MonoBehaviour
{
    public static RoomNavigationState Instance
    {
        get;
        private set;
    }

    [Header("Starting Navigation State")]

    [SerializeField]
    private RoomId startingRoom =
        RoomId.LivingRoom;

    public RoomId CurrentRoom
    {
        get;
        private set;
    }

    private readonly HashSet<RoomId>
        visitedRooms =
            new HashSet<RoomId>();

    private readonly HashSet<RoomId>
        unlockedRooms =
            new HashSet<RoomId>();

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(
            gameObject);

        CurrentRoom = startingRoom;

        unlockedRooms.Clear();
    }

    public void InitializeRoomAccess(
        ScenarioDefinition scenario)
    {
        if (scenario == null)
        {
            Debug.LogError(
                "RoomNavigationState cannot initialize room access " +
                "from a null ScenarioDefinition.");

            return;
        }

        unlockedRooms.Clear();

        ScenarioDefinition.InitialRoomAccessEntry[]
            entries =
                scenario.InitialRoomAccess;

        if (entries == null)
        {
            Debug.LogWarning(
                "Scenario \"" +
                scenario.name +
                "\" has no initial room access entries.");

            return;
        }

        HashSet<RoomId> configuredRooms =
            new HashSet<RoomId>();

        foreach (
            ScenarioDefinition.InitialRoomAccessEntry
                entry
            in entries)
        {
            if (entry == null)
            {
                Debug.LogWarning(
                    "Scenario \"" +
                    scenario.name +
                    "\" contains a null initial room access entry.");

                continue;
            }

            if (!configuredRooms.Add(
                    entry.RoomId))
            {
                Debug.LogWarning(
                    "Scenario \"" +
                    scenario.name +
                    "\" contains duplicate initial access " +
                    "configuration for room \"" +
                    entry.RoomId +
                    "\".");

                continue;
            }

            if (entry.InitiallyUnlocked)
            {
                unlockedRooms.Add(
                    entry.RoomId);
            }
        }

        Debug.Log(
            "Room access initialized from scenario: " +
            scenario.Title);
    }

    public void SetCurrentRoom(
        RoomId room)
    {
        CurrentRoom = room;

        MarkRoomVisited(room);
    }

    public void MarkRoomVisited(
        RoomId room)
    {
        visitedRooms.Add(room);
    }

    public bool IsRoomVisited(
        RoomId room)
    {
        return visitedRooms.Contains(room);
    }

    public bool IsRoomUnlocked(
        RoomId room)
    {
        return unlockedRooms.Contains(room);
    }

    public void UnlockRoom(
        RoomId room)
    {
        unlockedRooms.Add(room);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}