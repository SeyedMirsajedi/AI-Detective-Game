using UnityEngine;

public class ClueInteractable : MonoBehaviour, IInteractable
{
    [Header("Clue")]

    [SerializeField]
    private ClueDefinition clueDefinition;

    [SerializeField]
    private CaseState caseState;

    [SerializeField]
    private ClueInspectionUI clueInspectionUI;

    public InteractionMode Mode => InteractionMode.Press;

    public float HoldDuration => 0f;

    public string InteractionPrompt
    {
        get
        {
            if (clueDefinition == null ||
                string.IsNullOrWhiteSpace(clueDefinition.DisplayName))
            {
                return "inspect clue";
            }

            return "inspect " +
                   clueDefinition.DisplayName.ToLowerInvariant();
        }
    }

    public void InitializeRuntime(
        CaseState runtimeCaseState,
        ClueInspectionUI runtimeClueInspectionUI)
    {
        caseState = runtimeCaseState;
        clueInspectionUI = runtimeClueInspectionUI;
    }

    public void Interact()
    {
        if (clueDefinition == null)
        {
            Debug.LogWarning(
                gameObject.name +
                " cannot be inspected because no ClueDefinition is assigned.");

            return;
        }

        if (caseState == null)
        {
            Debug.LogWarning(
                gameObject.name +
                " cannot discover its clue because no CaseState is assigned.");

            return;
        }

        caseState.TryDiscoverClue(clueDefinition);

        if (clueInspectionUI == null)
        {
            Debug.LogWarning(
                gameObject.name +
                " cannot open its inspection UI because no ClueInspectionUI is assigned.");

            return;
        }

        clueInspectionUI.Show(clueDefinition);
    }
}