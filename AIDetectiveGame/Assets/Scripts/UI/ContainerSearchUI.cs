using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ContainerSearchUI : MonoBehaviour
{
    [Header("Container Search UI")]

    [SerializeField]
    private GameObject containerSearchPanel;

    [SerializeField]
    private TMP_Text containerTitleText;

    [SerializeField]
    private TMP_Text resultText;

    [Header("Player Controls")]

    [SerializeField]
    private PlayerMovement playerMovement;

    [SerializeField]
    private PlayerInteraction playerInteraction;

    [Header("Other UI")]

    [SerializeField]
    private ClueNotebookUI clueNotebookUI;

    private bool notebookWasEnabled;

    private void Awake()
    {
        if (containerSearchPanel != null)
        {
            containerSearchPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (containerSearchPanel == null ||
            !containerSearchPanel.activeSelf ||
            Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame ||
            Keyboard.current.enterKey.wasPressedThisFrame ||
            Keyboard.current.numpadEnterKey.wasPressedThisFrame)
        {
            Close();
        }
    }

    public void ShowFound(
        string containerName,
        IReadOnlyList<ClueDefinition> clues)
    {
        if (clues == null || clues.Count == 0)
        {
            ShowEmpty(containerName);
            return;
        }

        StringBuilder builder = new StringBuilder();

        builder.AppendLine("Found:");

        int validClueCount = 0;

        foreach (ClueDefinition clue in clues)
        {
            if (clue == null)
            {
                continue;
            }

            builder.AppendLine("• " + clue.DisplayName);
            validClueCount++;
        }

        if (validClueCount == 0)
        {
            ShowEmpty(containerName);
            return;
        }

        ShowResult(
            containerName,
            builder.ToString().TrimEnd());
    }

    public void ShowEmpty(string containerName)
    {
        ShowResult(
            containerName,
            "Nothing useful was found.");
    }

    public void ShowLocked(string containerName)
    {
        ShowResult(
            containerName,
            "It is locked.");
    }

    public void ShowAlreadySearched(string containerName)
    {
        ShowResult(
            containerName,
            "You've already searched this.");
    }

    public void Close()
    {
        if (containerSearchPanel != null)
        {
            containerSearchPanel.SetActive(false);
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        if (playerInteraction != null)
        {
            playerInteraction.SetInteractionEnabled(true);
        }

        if (clueNotebookUI != null)
        {
            clueNotebookUI.enabled = notebookWasEnabled;
        }
    }

    private void ShowResult(
        string containerName,
        string message)
    {
        if (containerSearchPanel == null ||
            containerTitleText == null ||
            resultText == null)
        {
            Debug.LogWarning(
                "ContainerSearchUI is missing one or more UI references.");

            return;
        }

        containerTitleText.text = containerName;
        resultText.text = message;

        containerSearchPanel.SetActive(true);

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        if (playerInteraction != null)
        {
            playerInteraction.SetInteractionEnabled(false);
        }

        if (clueNotebookUI != null)
        {
            notebookWasEnabled = clueNotebookUI.enabled;
            clueNotebookUI.enabled = false;
        }
    }
}