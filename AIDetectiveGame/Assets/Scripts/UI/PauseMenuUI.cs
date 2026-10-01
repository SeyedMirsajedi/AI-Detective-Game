using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    [Header("Pause UI")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button resumeButton;

    [Header("UI That Blocks Pausing")]
    [SerializeField] private GameObject[] blockingPanels;

    [Header("Scene")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private PlayerMovement playerMovement;
    private PlayerInteraction playerInteraction;

    private bool isPaused;


    private void Start()
    {
        ResolvePlayerControls();

        Time.timeScale = 1f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        isPaused = false;
    }


    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (!Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            return;
        }

        if (settingsPanel != null &&
            settingsPanel.activeSelf)
        {
            CloseSettings();
            return;
        }

        if (isPaused)
        {
            ResumeGame();
            return;
        }

        if (IsAnotherPanelOpen())
        {
            return;
        }

        PauseGame();
    }


    public void PauseGame()
    {
        ResolvePlayerControls();

        isPaused = true;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        if (resumeButton != null &&
            EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(
                resumeButton.gameObject);
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        if (playerInteraction != null)
        {
            playerInteraction.SetInteractionEnabled(false);
        }

        Time.timeScale = 0f;
    }


    public void ResumeGame()
    {
        isPaused = false;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        Time.timeScale = 1f;

        ResolvePlayerControls();

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        if (playerInteraction != null)
        {
            playerInteraction.SetInteractionEnabled(true);
        }
    }


    public void OpenSettings()
    {
        if (!isPaused)
        {
            return;
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }


    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        if (resumeButton != null &&
            EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(
                resumeButton.gameObject);
        }
    }


    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(mainMenuSceneName);
    }


    private void ResolvePlayerControls()
    {
        if (playerMovement == null)
        {
            playerMovement =
                FindFirstObjectByType<PlayerMovement>();
        }

        if (playerInteraction == null)
        {
            playerInteraction =
                FindFirstObjectByType<PlayerInteraction>();
        }
    }


    private bool IsAnotherPanelOpen()
    {
        if (blockingPanels == null)
        {
            return false;
        }

        foreach (GameObject panel in blockingPanels)
        {
            if (panel != null &&
                panel.activeSelf)
            {
                return true;
            }
        }

        return false;
    }
}