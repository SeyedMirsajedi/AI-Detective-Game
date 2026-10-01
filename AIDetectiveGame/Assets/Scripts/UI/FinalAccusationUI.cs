using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class FinalAccusationUI : MonoBehaviour
{
    [Header("HUD")]

    [SerializeField]
    private Button concludeCaseButton;

    [SerializeField]
    private TMP_Text accusationProgressText;

    [Header("Final Accusation Panel")]

    [SerializeField]
    private GameObject finalAccusationPanel;

    [SerializeField]
    private Button[] suspectButtons;

    [SerializeField]
    private Button cancelButton;

    [Header("Confirmation Panel")]

    [SerializeField]
    private GameObject accusationConfirmPanel;

    [SerializeField]
    private TMP_Text confirmMessageText;

    [SerializeField]
    private Button confirmButton;

    [SerializeField]
    private Button backButton;

    [Header("Case Result Panel")]

    [SerializeField]
    private GameObject caseResultPanel;

    [SerializeField]
    private TMP_Text resultTitleText;

    [SerializeField]
    private Image resultStampImage;

    [SerializeField]
    private Sprite solvedStampSprite;

    [SerializeField]
    private Sprite failedStampSprite;

    [SerializeField]
    private TMP_Text resultBodyText;

    [SerializeField]
    private Button returnToMainMenuButton;

    [Header("Scene Configuration")]

    [SerializeField]
    private string mainMenuSceneName =
    "MainMenu";

    [Header("Runtime References")]

    [SerializeField]
    private CaseResolutionController
        caseResolutionController;

    private readonly List<NpcDefinition>
        suspectDefinitions =
            new List<NpcDefinition>();

    private int selectedSuspectIndex;

    private NpcDefinition selectedSuspect;

    private bool finalAccusationCommitted;

    private float confirmationInputUnlockTime;

    private CaseResolutionController.AccusationResult
        committedResult;

    public NpcDefinition SelectedSuspect =>
        selectedSuspect;

    public bool FinalAccusationCommitted =>
        finalAccusationCommitted;

    public CaseResolutionController.AccusationResult
        CommittedResult =>
            committedResult;

    public bool IsAccusationPanelOpen =>
        finalAccusationPanel != null &&
        finalAccusationPanel.activeSelf;

    public bool IsConfirmationPanelOpen =>
        accusationConfirmPanel != null &&
        accusationConfirmPanel.activeSelf;

    private void Start()
    {
        FindRuntimeReferences();

        if (finalAccusationPanel != null)
        {
            finalAccusationPanel.SetActive(false);
        }

        if (accusationConfirmPanel != null)
        {
            accusationConfirmPanel.SetActive(false);
        }

        if (caseResultPanel != null)
        {
            caseResultPanel.SetActive(false);
        }

        if (concludeCaseButton != null)
        {
            concludeCaseButton.onClick.AddListener(
                TryOpenAccusation);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(
                CloseAccusationPanel);
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(
                ConfirmFinalAccusation);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(
                ReturnToSuspectSelection);
        }

        if (returnToMainMenuButton != null)
        {
            returnToMainMenuButton.onClick.AddListener(
                ReturnToMainMenu);
        }

        RegisterSuspectButtonListeners();

        RefreshHud();
    }

    private void Update()
    {
        if (caseResolutionController == null)
        {
            FindRuntimeReferences();
        }

        RefreshHud();

        if (Keyboard.current == null)
        {
            return;
        }

        if (finalAccusationCommitted)
        {
            return;
        }

        if (IsConfirmationPanelOpen)
        {
            HandleConfirmationKeyboardInput();
            return;
        }

        if (IsAccusationPanelOpen)
        {
            HandleAccusationKeyboardInput();
            return;
        }

        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            TryOpenAccusation();
        }
    }

    private void OnDestroy()
    {
        if (concludeCaseButton != null)
        {
            concludeCaseButton.onClick.RemoveListener(
                TryOpenAccusation);
        }

        if (returnToMainMenuButton != null)
        {
            returnToMainMenuButton.onClick.RemoveListener(
                ReturnToMainMenu);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveListener(
                CloseAccusationPanel);
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(
                ConfirmFinalAccusation);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(
                ReturnToSuspectSelection);
        }

        UnregisterSuspectButtonListeners();
    }

    private void FindRuntimeReferences()
    {
        if (caseResolutionController != null)
        {
            return;
        }

        caseResolutionController =
            FindFirstObjectByType<
                CaseResolutionController>();
    }

    private void RefreshHud()
    {
        if (caseResolutionController == null)
        {
            if (concludeCaseButton != null)
            {
                concludeCaseButton.interactable =
                    false;
            }

            if (accusationProgressText != null)
            {
                accusationProgressText.text =
                    "Interviews: 0 / 0\n" +
                    "Important Evidence: 0 / 0";
            }

            return;
        }

        int interviewCount =
            caseResolutionController
                .GetMeaningfulConversationCount();

        int requiredInterviewCount =
            caseResolutionController
                .GetRequiredConversationCount();

        int evidenceCount =
            caseResolutionController
                .GetImportantEvidenceCount();

        int requiredEvidenceCount =
            caseResolutionController
                .GetRequiredImportantEvidenceCount();

        int displayedEvidenceCount =
            Mathf.Min(
                evidenceCount,
                requiredEvidenceCount);

        if (accusationProgressText != null)
        {
            accusationProgressText.text =
                "Interviews: " +
                interviewCount +
                " / " +
                requiredInterviewCount +
                "\nImportant Evidence: " +
                displayedEvidenceCount +
                " / " +
                requiredEvidenceCount;
        }

        if (concludeCaseButton != null)
        {
            concludeCaseButton.interactable =
                !finalAccusationCommitted &&
                caseResolutionController
                    .IsAccusationAvailable();
        }
    }

    private void TryOpenAccusation()
    {
        if (finalAccusationCommitted)
        {
            return;
        }

        if (caseResolutionController == null)
        {
            return;
        }

        if (!caseResolutionController
            .IsAccusationAvailable())
        {
            return;
        }

        if (IsAccusationPanelOpen ||
            IsConfirmationPanelOpen)
        {
            return;
        }

        if (!ConfigureSuspectsFromScenario())
        {
            return;
        }

        selectedSuspect = null;
        selectedSuspectIndex = 0;

        finalAccusationPanel.SetActive(true);

        SetNormalGameplayEnabled(false);

        RefreshSuspectSelectionVisual();
    }

    private bool ConfigureSuspectsFromScenario()
    {
        suspectDefinitions.Clear();

        ScenarioDefinition scenario =
            caseResolutionController.ActiveScenario;

        if (scenario == null ||
            scenario.NpcProfiles == null)
        {
            Debug.LogError(
                "FinalAccusationUI cannot build suspect " +
                "list because the active scenario has no " +
                "NPC profiles.",
                this);

            return false;
        }

        if (suspectButtons == null ||
            suspectButtons.Length == 0)
        {
            Debug.LogError(
                "FinalAccusationUI has no suspect buttons assigned.",
                this);

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

            suspectDefinitions.Add(
                profile.NpcDefinition);
        }

        if (suspectDefinitions.Count !=
            suspectButtons.Length)
        {
            Debug.LogError(
                "FinalAccusationUI suspect button count does " +
                "not match the active scenario NPC count. " +
                "Scenario NPCs: " +
                suspectDefinitions.Count +
                ", Buttons: " +
                suspectButtons.Length,
                this);

            return false;
        }

        for (int i = 0;
             i < suspectButtons.Length;
             i++)
        {
            Button button =
                suspectButtons[i];

            NpcDefinition npc =
                suspectDefinitions[i];

            if (button == null ||
                npc == null)
            {
                Debug.LogError(
                    "FinalAccusationUI contains a missing " +
                    "suspect button or NPC definition.",
                    this);

                return false;
            }

            TMP_Text buttonText =
                button.GetComponentInChildren<TMP_Text>();

            if (buttonText != null)
            {
                buttonText.text =
                    npc.DisplayName;
            }
        }

        return true;
    }

    private void HandleAccusationKeyboardInput()
    {
        if (Keyboard.current.escapeKey
            .wasPressedThisFrame)
        {
            CloseAccusationPanel();
            return;
        }

        if (Keyboard.current.upArrowKey
                .wasPressedThisFrame ||
            Keyboard.current.wKey
                .wasPressedThisFrame)
        {
            MoveSuspectSelection(-1);
            return;
        }

        if (Keyboard.current.downArrowKey
                .wasPressedThisFrame ||
            Keyboard.current.sKey
                .wasPressedThisFrame)
        {
            MoveSuspectSelection(1);
            return;
        }

        if (Keyboard.current.enterKey
                .wasPressedThisFrame ||
            Keyboard.current.numpadEnterKey
                .wasPressedThisFrame)
        {
            ChooseCurrentSuspect();
        }
    }

    private void HandleConfirmationKeyboardInput()
    {
        if (Keyboard.current.escapeKey
            .wasPressedThisFrame)
        {
            ReturnToSuspectSelection();
            return;
        }

        if (Keyboard.current.enterKey
                .wasPressedThisFrame ||
            Keyboard.current.numpadEnterKey
                .wasPressedThisFrame)
        {
            ConfirmFinalAccusation();
        }
    }

    private void MoveSuspectSelection(
        int direction)
    {
        if (suspectDefinitions.Count == 0)
        {
            return;
        }

        selectedSuspectIndex += direction;

        if (selectedSuspectIndex < 0)
        {
            selectedSuspectIndex =
                suspectDefinitions.Count - 1;
        }
        else if (selectedSuspectIndex >=
                 suspectDefinitions.Count)
        {
            selectedSuspectIndex = 0;
        }

        RefreshSuspectSelectionVisual();
    }

    private void ChooseCurrentSuspect()
    {
        if (selectedSuspectIndex < 0 ||
            selectedSuspectIndex >=
            suspectDefinitions.Count)
        {
            return;
        }

        selectedSuspect =
            suspectDefinitions[
                selectedSuspectIndex];

        OpenConfirmationPanel();
    }

    private void ChooseSuspectByIndex(
        int index)
    {
        if (index < 0 ||
            index >= suspectDefinitions.Count)
        {
            return;
        }

        selectedSuspectIndex = index;

        RefreshSuspectSelectionVisual();

        ChooseCurrentSuspect();
    }

    private void OpenConfirmationPanel()
    {
        if (selectedSuspect == null)
        {
            return;
        }

        if (confirmMessageText != null)
        {
            confirmMessageText.text =
                "You are about to formally accuse\n" +
                selectedSuspect.DisplayName +
                "\nof Edmund Bellamy's murder.\n\n" +
                "Once the accusation is made,\n" +
                "the investigation will end.";
        }

        if (finalAccusationPanel != null)
        {
            finalAccusationPanel.SetActive(false);
        }

        if (accusationConfirmPanel != null)
        {
            accusationConfirmPanel.SetActive(true);
        }

        confirmationInputUnlockTime =
            Time.unscaledTime + 0.15f;

        if (confirmButton != null)
        {
            confirmButton.interactable = true;
            confirmButton.Select();
        }

        if (backButton != null)
        {
            backButton.interactable = true;
        }
    }

    private void ReturnToSuspectSelection()
    {
        if (finalAccusationCommitted)
        {
            return;
        }

        if (!IsConfirmationPanelOpen)
        {
            return;
        }

        accusationConfirmPanel.SetActive(false);

        finalAccusationPanel.SetActive(true);

        RefreshSuspectSelectionVisual();
    }

    private void ConfirmFinalAccusation()
    {
        if (finalAccusationCommitted)
        {
            return;
        }

        if (Time.unscaledTime <
            confirmationInputUnlockTime)
        {
            return;
        }

        if (caseResolutionController == null ||
            selectedSuspect == null)
        {
            return;
        }

        CaseResolutionController.AccusationResult
            result =
                caseResolutionController
                    .EvaluateAccusation(
                        selectedSuspect);

        if (result ==
                CaseResolutionController
                    .AccusationResult.Unavailable ||
            result ==
                CaseResolutionController
                    .AccusationResult.InvalidSuspect)
        {
            Debug.LogError(
                "Final accusation could not be committed. " +
                "Result: " +
                result,
                this);

            return;
        }

        committedResult = result;
        finalAccusationCommitted = true;

        if (confirmButton != null)
        {
            confirmButton.interactable = false;
        }

        if (backButton != null)
        {
            backButton.interactable = false;
        }

        Debug.Log(
            "Final accusation committed.\n" +
            "Suspect: " +
            selectedSuspect.DisplayName +
            " (" +
            selectedSuspect.NpcId +
            ")\nResult: " +
            committedResult,
            this);

        if (committedResult ==
            CaseResolutionController.AccusationResult.Correct)
        {
            ShowCorrectResult();
        }
        else
        {
            ShowIncorrectResult();
        }

        // Normal gameplay intentionally remains disabled.
        // There is no return to investigation after confirmation.
    }

    private void ShowCorrectResult()
    {
        if (caseResolutionController == null)
        {
            return;
        }

        ScenarioDefinition scenario =
            caseResolutionController.ActiveScenario;

        if (scenario == null)
        {
            Debug.LogError(
                "Cannot display the solved-case result because " +
                "there is no active scenario.",
                this);

            return;
        }

        if (accusationConfirmPanel != null)
        {
            accusationConfirmPanel.SetActive(false);
        }

        if (finalAccusationPanel != null)
        {
            finalAccusationPanel.SetActive(false);
        }

        if (resultTitleText != null)
        {
            resultTitleText.text =
                "CASE SOLVED";
        }

        if (resultBodyText != null)
        {
            resultBodyText.text =
                scenario.CorrectResolutionSummary;
        }

        if (resultStampImage != null)
        {
            resultStampImage.sprite = solvedStampSprite;
            resultStampImage.gameObject.SetActive(true);
        }

        if (caseResultPanel != null)
        {
            caseResultPanel.SetActive(true);
        }
    }

    private void ShowIncorrectResult()
    {
        if (caseResolutionController == null)
        {
            return;
        }

        ScenarioDefinition scenario =
            caseResolutionController.ActiveScenario;

        if (scenario == null)
        {
            Debug.LogError(
                "Cannot display the failed-case result because " +
                "there is no active scenario.",
                this);

            return;
        }

        if (accusationConfirmPanel != null)
        {
            accusationConfirmPanel.SetActive(false);
        }

        if (finalAccusationPanel != null)
        {
            finalAccusationPanel.SetActive(false);
        }

        if (resultTitleText != null)
        {
            resultTitleText.text =
                "CASE FAILED";
        }

        if (resultBodyText != null)
        {
            resultBodyText.text =
                scenario.FailedResolutionSummary;
        }

        if (resultStampImage != null)
        {
            resultStampImage.sprite =
                failedStampSprite;

            resultStampImage.gameObject.SetActive(true);
        }

        if (caseResultPanel != null)
        {
            caseResultPanel.SetActive(true);
        }
    }

    private void RefreshSuspectSelectionVisual()
    {
        if (suspectButtons == null)
        {
            return;
        }

        for (int i = 0;
             i < suspectButtons.Length;
             i++)
        {
            Button button =
                suspectButtons[i];

            if (button == null)
            {
                continue;
            }

            TMP_Text buttonText =
                button.GetComponentInChildren<TMP_Text>();

            if (buttonText == null)
            {
                continue;
            }

            string displayName =
                i < suspectDefinitions.Count &&
                suspectDefinitions[i] != null
                    ? suspectDefinitions[i].DisplayName
                    : "Unknown";

            if (i == selectedSuspectIndex)
            {
                buttonText.text =
                    "> " + displayName;
            }
            else
            {
                buttonText.text =
                    "  " + displayName;
            }
        }

        if (selectedSuspectIndex >= 0 &&
            selectedSuspectIndex <
            suspectButtons.Length &&
            suspectButtons[
                selectedSuspectIndex] != null)
        {
            suspectButtons[
                selectedSuspectIndex].Select();
        }
    }

    private void CloseAccusationPanel()
    {
        if (finalAccusationCommitted)
        {
            return;
        }

        if (!IsAccusationPanelOpen)
        {
            return;
        }

        finalAccusationPanel.SetActive(false);

        selectedSuspect = null;

        SetNormalGameplayEnabled(true);
    }

    private void RegisterSuspectButtonListeners()
    {
        if (suspectButtons == null)
        {
            return;
        }

        for (int i = 0;
             i < suspectButtons.Length;
             i++)
        {
            Button button =
                suspectButtons[i];

            if (button == null)
            {
                continue;
            }

            int buttonIndex = i;

            button.onClick.AddListener(
                () =>
                    ChooseSuspectByIndex(
                        buttonIndex));
        }
    }

    private void UnregisterSuspectButtonListeners()
    {
        if (suspectButtons == null)
        {
            return;
        }

        for (int i = 0;
             i < suspectButtons.Length;
             i++)
        {
            Button button =
                suspectButtons[i];

            if (button == null)
            {
                continue;
            }

            button.onClick.RemoveAllListeners();
        }
    }

    private void SetNormalGameplayEnabled(
        bool enabled)
    {
        PlayerMovement playerMovement =
            FindFirstObjectByType<PlayerMovement>();

        if (playerMovement != null)
        {
            playerMovement.enabled = enabled;
        }

        PlayerInteraction playerInteraction =
            FindFirstObjectByType<PlayerInteraction>();

        if (playerInteraction != null)
        {
            playerInteraction.SetInteractionEnabled(
                enabled);
        }
    }

    private void ReturnToMainMenu()
    {
        if (!finalAccusationCommitted)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(
                mainMenuSceneName))
        {
            Debug.LogError(
                "FinalAccusationUI has no Main Menu " +
                "scene configured.",
                this);

            return;
        }

        if (returnToMainMenuButton != null)
        {
            returnToMainMenuButton.interactable =
                false;
        }

        StartCoroutine(
            ReturnToMainMenuRoutine());
    }

    private IEnumerator ReturnToMainMenuRoutine()
    {
        GameObject oldGameSession = null;

        if (RoomNavigationState.Instance != null)
        {
            oldGameSession =
                RoomNavigationState.Instance.gameObject;
        }

        if (oldGameSession != null)
        {
            Debug.Log(
                "Ending current investigation and " +
                "destroying the persistent GameSession.",
                this);

            Destroy(oldGameSession);

            // Destroy() completes at the end of the frame.
            // Wait one frame before loading the menu so the
            // old authoritative session is fully gone.
            yield return null;
        }

        SceneManager.LoadScene(
            mainMenuSceneName);
    }
}