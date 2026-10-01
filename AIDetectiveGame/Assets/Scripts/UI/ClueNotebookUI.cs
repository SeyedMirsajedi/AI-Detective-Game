using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClueNotebookUI : MonoBehaviour
{
    private const int CluesPerPage = 8;

    [Header("Notebook UI")]

    [SerializeField]
    private GameObject notebookPanel;

    [SerializeField]
    private TMP_Text[] clueRowTexts;

    [SerializeField]
    private GameObject[] selectionDots;

    [Header("Selected Clue Details")]

    [SerializeField]
    private TMP_Text selectedClueTitleText;

    [SerializeField]
    private TMP_Text selectedClueDescriptionText;

    [SerializeField]
    private TMP_Text pageText;

    [Header("Other UI")]

    [SerializeField]
    private GameObject inspectionPanel;

    [SerializeField]
    private ClueInspectionUI clueInspectionUI;

    [Header("Case State")]

    [SerializeField]
    private CaseState caseState;

    [Header("Player Controls")]

    [SerializeField]
    private PlayerMovement playerMovement;

    [SerializeField]
    private PlayerInteraction playerInteraction;

    private int selectedIndex;


    private void Start()
    {
        ResolvePersistentCaseState();
    }


    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (notebookPanel != null &&
            notebookPanel.activeSelf)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Close();
                return;
            }

            if (Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                MoveSelection(-1);
                return;
            }

            if (Keyboard.current.downArrowKey.wasPressedThisFrame)
            {
                MoveSelection(1);
                return;
            }

            if (Keyboard.current.enterKey.wasPressedThisFrame ||
                Keyboard.current.numpadEnterKey.wasPressedThisFrame)
            {
                InspectSelectedClue();
            }

            return;
        }

        if (Keyboard.current.nKey.wasPressedThisFrame)
        {
            Open();
        }
    }


    public void Open()
    {
        ResolvePersistentCaseState();

        if (notebookPanel == null ||
            caseState == null)
        {
            Debug.LogWarning(
                "ClueNotebookUI is missing one or more references.");

            return;
        }

        if (inspectionPanel != null &&
            inspectionPanel.activeSelf)
        {
            return;
        }

        notebookPanel.SetActive(true);

        RefreshNotebook();

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        if (playerInteraction != null)
        {
            playerInteraction.SetInteractionEnabled(false);
        }
    }


    public void Close()
    {
        if (notebookPanel != null)
        {
            notebookPanel.SetActive(false);
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        if (playerInteraction != null)
        {
            playerInteraction.SetInteractionEnabled(true);
        }
    }


    private void ResolvePersistentCaseState()
    {
        if (RoomNavigationState.Instance == null)
        {
            return;
        }

        CaseState persistentCaseState =
            RoomNavigationState.Instance.GetComponent<CaseState>();

        if (persistentCaseState != null)
        {
            caseState = persistentCaseState;
        }
    }


    private void MoveSelection(int direction)
    {
        if (caseState == null ||
            caseState.DiscoveredClues.Count == 0)
        {
            return;
        }

        selectedIndex += direction;

        if (selectedIndex < 0)
        {
            selectedIndex =
                caseState.DiscoveredClues.Count - 1;
        }
        else if (selectedIndex >=
                 caseState.DiscoveredClues.Count)
        {
            selectedIndex = 0;
        }

        Debug.Log("Notebook selected clue index: " + selectedIndex);

        RefreshNotebook();
    }


    private void InspectSelectedClue()
    {
        if (caseState == null ||
            caseState.DiscoveredClues.Count == 0)
        {
            return;
        }

        if (clueInspectionUI == null)
        {
            Debug.LogWarning(
                "ClueNotebookUI is missing its ClueInspectionUI reference.");

            return;
        }

        selectedIndex = Mathf.Clamp(
            selectedIndex,
            0,
            caseState.DiscoveredClues.Count - 1);

        ClueDefinition selectedClue =
            caseState.DiscoveredClues[selectedIndex];

        if (notebookPanel != null)
        {
            notebookPanel.SetActive(false);
        }

        clueInspectionUI.Show(selectedClue);
    }


    private void RefreshNotebook()
    {
        RefreshClueRows();
        UpdateSelectionDots();
        RefreshSelectedClueDetails();
        RefreshPageText();
    }


    private void RefreshClueRows()
    {
        if (clueRowTexts == null ||
            clueRowTexts.Length == 0)
        {
            return;
        }

        // Clear every visible notebook row first.
        for (int i = 0; i < clueRowTexts.Length; i++)
        {
            if (clueRowTexts[i] != null)
            {
                clueRowTexts[i].text = "";
            }
        }

        if (caseState == null ||
            caseState.DiscoveredClues.Count == 0)
        {
            if (clueRowTexts[0] != null)
            {
                clueRowTexts[0].text =
                    "No clues discovered yet.";
            }

            selectedIndex = 0;
            return;
        }

        selectedIndex = Mathf.Clamp(
            selectedIndex,
            0,
            caseState.DiscoveredClues.Count - 1);

        int currentPage =
            selectedIndex / CluesPerPage;

        int firstClueIndex =
            currentPage * CluesPerPage;

        for (int visibleRow = 0;
             visibleRow < CluesPerPage;
             visibleRow++)
        {
            if (visibleRow >= clueRowTexts.Length)
            {
                break;
            }

            int clueIndex =
                firstClueIndex + visibleRow;

            if (clueIndex >=
                caseState.DiscoveredClues.Count)
            {
                break;
            }

            ClueDefinition clue =
                caseState.DiscoveredClues[clueIndex];

            if (clueRowTexts[visibleRow] != null)
            {
                clueRowTexts[visibleRow].text =
                    clue.DisplayName;
            }
        }
    }

    private void RefreshPageText()
    {
        if (pageText == null)
        {
            return;
        }

        if (caseState == null)
        {
            pageText.text = "";
            return;
        }

        int clueCount = caseState.DiscoveredClues.Count;

        int totalPages = Mathf.Max(
            1,
            Mathf.CeilToInt(
                clueCount / (float)CluesPerPage));

        int currentPage = 1;

        if (clueCount > 0)
        {
            currentPage =
                (selectedIndex / CluesPerPage) + 1;
        }

        pageText.text =
            $"Page {currentPage} / {totalPages}";
    }


    private void UpdateSelectionDots()
    {
        if (selectionDots == null ||
            selectionDots.Length == 0)
        {
            return;
        }

        for (int i = 0; i < selectionDots.Length; i++)
        {
            if (selectionDots[i] != null)
            {
                selectionDots[i].SetActive(false);
            }
        }

        if (caseState == null ||
            caseState.DiscoveredClues.Count == 0)
        {
            return;
        }

        int visibleRow = selectedIndex % CluesPerPage;

        Debug.Log(
            "Selected Index: " + selectedIndex +
            " | Visible Row: " + visibleRow);

        if (visibleRow >= 0 &&
            visibleRow < selectionDots.Length &&
            selectionDots[visibleRow] != null)
        {
            selectionDots[visibleRow].SetActive(true);
        }
    }


    private void RefreshSelectedClueDetails()
    {
        if (selectedClueTitleText == null ||
            selectedClueDescriptionText == null)
        {
            return;
        }

        if (caseState == null ||
            caseState.DiscoveredClues.Count == 0)
        {
            selectedClueTitleText.text = "";
            selectedClueDescriptionText.text =
                "No clue selected.";

            return;
        }

        selectedIndex = Mathf.Clamp(
            selectedIndex,
            0,
            caseState.DiscoveredClues.Count - 1);

        ClueDefinition clue =
            caseState.DiscoveredClues[selectedIndex];

        selectedClueTitleText.text =
            clue.DisplayName;

        selectedClueDescriptionText.text =
            clue.Description;
    }
}