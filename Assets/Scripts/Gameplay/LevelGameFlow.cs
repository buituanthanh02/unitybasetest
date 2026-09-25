using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelGameFlow : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private CharacterController player;
    [SerializeField] private PlayerAnimation playerAnimation;

    [Header("Disable when the level ends")]
    [SerializeField] private Behaviour[] controlsToDisable;

    [Header("UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject winPanel;

    [Header("Scene Loading")]
    [SerializeField] private SceneLoader sceneLoader;

    public CharacterController Player => player;
    public bool HasEnded { get; private set; }

    private void Awake()
    {
        HasEnded = false;
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
    }

    public void Lose()
    {
        FinishLevel(false);
    }

    public void Win()
    {
        FinishLevel(true);
    }

    private void FinishLevel(bool won)
    {
        if (HasEnded)
        {
            return;
        }

        HasEnded = true;

        foreach (Behaviour control in controlsToDisable)
        {
            if (control != null)
            {
                control.enabled = false;
            }
        }

        if (playerAnimation != null)
        {
            if (won)
            {
                playerAnimation.PlayWin();
            }
            else
            {
                playerAnimation.PlayDeath();
            }
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(!won);
        }

        if (winPanel != null)
        {
            winPanel.SetActive(won);
        }
    }

    public void ResetLevel()
    {
        if (sceneLoader == null)
        {
            Debug.LogError(
                "LevelGameFlow chưa được gán Scene Loader.",
                this
            );
            return;
        }

        sceneLoader.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }

    public void ReturnToMainMenu()
    {
        if (sceneLoader == null)
        {
            Debug.LogError(
                "LevelGameFlow chưa được gán Scene Loader.",
                this
            );
            return;
        }

        sceneLoader.LoadScene("MainMenu");
    }
}