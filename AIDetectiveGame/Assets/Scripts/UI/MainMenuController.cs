using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class MainMenuController : MonoBehaviour
{
    [Header("Buttons")]

    [SerializeField]
    private Button newGameButton;

    [SerializeField]
    private Button quitButton;

    [Header("Scene Configuration")]

    [SerializeField]
    private string investigationStartScene =
        "LivingRoom";

    private void Start()
    {
        if (newGameButton != null)
        {
            newGameButton.onClick.AddListener(
                StartNewGame);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(
                QuitGame);
        }
    }

    private void OnDestroy()
    {
        if (newGameButton != null)
        {
            newGameButton.onClick.RemoveListener(
                StartNewGame);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(
                QuitGame);
        }
    }

    private void StartNewGame()
    {
        if (string.IsNullOrWhiteSpace(
                investigationStartScene))
        {
            Debug.LogError(
                "MainMenuController has no investigation " +
                "start scene configured.",
                this);

            return;
        }

        Debug.Log(
            "Starting new investigation: " +
            investigationStartScene,
            this);

        SceneManager.LoadScene(
            investigationStartScene);
    }

    private void QuitGame()
    {
        Debug.Log(
            "Quit requested.",
            this);

        Application.Quit();
    }
}