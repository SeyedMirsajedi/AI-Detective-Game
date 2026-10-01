using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClueInspectionUI : MonoBehaviour
{
    [Header("Inspection UI")]

    [SerializeField]
    private GameObject inspectionPanel;

    [SerializeField]
    private TMP_Text titleText;

    [SerializeField]
    private TMP_Text descriptionText;

    [Header("Player Controls")]

    [SerializeField]
    private PlayerMovement playerMovement;

    [SerializeField]
    private PlayerInteraction playerInteraction;

    private int openedFrame = -1;

    private void Update()
    {
        if (inspectionPanel == null ||
            !inspectionPanel.activeSelf ||
            Keyboard.current == null)
        {
            return;
        }

        // Ignore the same key press that opened the panel.
        if (Time.frameCount == openedFrame)
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

    public void Show(ClueDefinition clue)
    {
        if (clue == null)
        {
            Debug.LogWarning(
                "Cannot show inspection UI for a null clue.");

            return;
        }

        if (inspectionPanel == null ||
            titleText == null ||
            descriptionText == null)
        {
            Debug.LogWarning(
                "ClueInspectionUI is missing one or more UI references.");

            return;
        }

        titleText.text = clue.DisplayName;
        descriptionText.text = clue.Description;

        openedFrame = Time.frameCount;

        inspectionPanel.SetActive(true);

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
        if (inspectionPanel != null)
        {
            inspectionPanel.SetActive(false);
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
}