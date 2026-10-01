using System.Collections.Generic;
using UnityEngine;

public class SearchableContainer : MonoBehaviour, IInteractable
{
    [Header("Container Identity")]

    [SerializeField]
    private string containerId;

    [SerializeField]
    private string displayName = "Container";

    [Header("Interaction")]

    [SerializeField]
    private InteractionMode interactionMode = InteractionMode.Hold;

    [SerializeField]
    [Min(0.1f)]
    private float holdDuration = 1f;

    [Header("Container State")]

    [SerializeField]
    private bool initiallyLocked;

    [Header("Contents")]

    [SerializeField]
    private ClueDefinition[] clueContents;

    [Header("Visuals")]

    [SerializeField]
    private SpriteRenderer visualRenderer;

    [SerializeField]
    private Sprite closedSprite;

    [SerializeField]
    private Sprite openSprite;

    [Header("References")]

    [SerializeField]
    private CaseState caseState;

    [SerializeField]
    private ContainerSearchUI containerSearchUI;

    private bool isLocked;
    private bool isSearched;

    public InteractionMode Mode => interactionMode;

    public float HoldDuration => holdDuration;

    public string InteractionPrompt
    {
        get
        {
            if (isSearched)
            {
                return "check " + GetDisplayName();
            }

            return "search " + GetDisplayName();
        }
    }

    public string ContainerId => containerId;

    public bool IsLocked => isLocked;

    public bool IsSearched => isSearched;

    private void Awake()
    {
        isLocked = initiallyLocked;
        isSearched = false;

        ApplyVisualState();
    }

    public void InitializeRuntime(
        CaseState runtimeCaseState,
        ContainerSearchUI runtimeContainerSearchUI)
    {
        caseState = runtimeCaseState;
        containerSearchUI = runtimeContainerSearchUI;

        RestorePersistentState();
    }

    public void Interact()
    {
        if (isLocked)
        {
            if (containerSearchUI != null)
            {
                containerSearchUI.ShowLocked(GetDisplayName());
            }
            else
            {
                Debug.LogWarning(
                    "SearchableContainer is missing its ContainerSearchUI reference.");
            }

            return;
        }

        if (isSearched)
        {
            if (containerSearchUI != null)
            {
                containerSearchUI.ShowAlreadySearched(GetDisplayName());
            }
            else
            {
                Debug.LogWarning(
                    "SearchableContainer is missing its ContainerSearchUI reference.");
            }

            return;
        }

        if (HasValidClueContents() && caseState == null)
        {
            Debug.LogWarning(
                "SearchableContainer '" + GetDisplayName() +
                "' contains clues but has no CaseState reference.");

            return;
        }

        isSearched = true;

        if (caseState != null)
        {
            caseState.MarkContainerSearched(
                containerId,
                initiallyLocked);
        }

        ApplyVisualState();

        List<ClueDefinition> validContents =
            new List<ClueDefinition>();

        if (clueContents != null)
        {
            foreach (ClueDefinition clue in clueContents)
            {
                if (clue == null)
                {
                    continue;
                }

                validContents.Add(clue);

                if (caseState != null)
                {
                    caseState.TryDiscoverClue(clue);
                }
            }
        }

        if (containerSearchUI == null)
        {
            Debug.LogWarning(
                "SearchableContainer is missing its ContainerSearchUI reference.");

            return;
        }

        if (validContents.Count == 0)
        {
            containerSearchUI.ShowEmpty(GetDisplayName());
            return;
        }

        containerSearchUI.ShowFound(
            GetDisplayName(),
            validContents);
    }

    public void Unlock()
    {
        if (!isLocked)
        {
            return;
        }

        isLocked = false;

        if (caseState != null)
        {
            caseState.UnlockContainer(
                containerId,
                initiallyLocked);
        }

        Debug.Log(
            "Container unlocked: " + GetDisplayName());
    }

    private void RestorePersistentState()
    {
        if (caseState == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(containerId))
        {
            Debug.LogWarning(
                "SearchableContainer '" + GetDisplayName() +
                "' has an empty Container ID. Its state cannot persist.",
                this);

            return;
        }

        isLocked =
            caseState.IsContainerLocked(
                containerId,
                initiallyLocked);

        isSearched =
            caseState.IsContainerSearched(
                containerId,
                initiallyLocked);

        ApplyVisualState();
    }

    private bool HasValidClueContents()
    {
        if (clueContents == null)
        {
            return false;
        }

        foreach (ClueDefinition clue in clueContents)
        {
            if (clue != null)
            {
                return true;
            }
        }

        return false;
    }

    private void ApplyVisualState()
    {
        if (visualRenderer == null)
        {
            return;
        }

        Sprite targetSprite =
            isSearched ? openSprite : closedSprite;

        if (targetSprite != null)
        {
            visualRenderer.sprite = targetSprite;
        }
    }

    private string GetDisplayName()
    {
        if (!string.IsNullOrWhiteSpace(displayName))
        {
            return displayName;
        }

        return gameObject.name;
    }
}