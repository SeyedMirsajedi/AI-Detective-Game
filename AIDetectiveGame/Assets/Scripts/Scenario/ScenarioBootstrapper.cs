using UnityEngine;

[DisallowMultipleComponent]
public class ScenarioBootstrapper : MonoBehaviour
{
    [Header("Starting Scenario")]

    [SerializeField]
    private ScenarioDefinition startingScenario;

    [Header("Runtime References")]

    [SerializeField]
    private CaseState caseState;

    [SerializeField]
    private RoomNavigationState roomNavigationState;

    private void Reset()
    {
        caseState =
            GetComponent<CaseState>();

        roomNavigationState =
            GetComponent<RoomNavigationState>();
    }

    private void Start()
    {
        if (!ValidateReferences())
        {
            return;
        }

        // If this GameSession is a duplicate that is
        // already scheduled for destruction, do nothing.
        if (RoomNavigationState.Instance !=
            roomNavigationState)
        {
            return;
        }

        if (!caseState.IsInitialized)
        {
            bool initialized =
                caseState.InitializeScenario(
                    startingScenario);

            if (!initialized)
            {
                return;
            }

            roomNavigationState
                .InitializeRoomAccess(
                    startingScenario);
        }
    }

    private bool ValidateReferences()
    {
        bool valid = true;

        if (startingScenario == null)
        {
            Debug.LogError(
                "ScenarioBootstrapper has no Starting Scenario assigned.",
                this);

            valid = false;
        }

        if (caseState == null)
        {
            Debug.LogError(
                "ScenarioBootstrapper has no CaseState assigned.",
                this);

            valid = false;
        }

        if (roomNavigationState == null)
        {
            Debug.LogError(
                "ScenarioBootstrapper has no RoomNavigationState assigned.",
                this);

            valid = false;
        }

        return valid;
    }
}