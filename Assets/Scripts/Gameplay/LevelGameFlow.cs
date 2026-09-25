using System.Collections;
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

    [Header("Result Panel")]
    [SerializeField, Min(0f)] private float resultPanelDelay = 0.35f;

    public CharacterController Player => player;
    public bool HasEnded { get; private set; }

    private void Awake()
    {
        HasEnded = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
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
        if (HasEnded || (sceneLoader != null && sceneLoader.IsLoading)) return;
        HasEnded = true;

        if (controlsToDisable != null)
        {
            foreach (Behaviour control in controlsToDisable)
                if (control != null) control.enabled = false;
        }
        if (playerAnimation != null)
        {
            if (won) playerAnimation.PlayWin();
            else playerAnimation.PlayDeath();
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        StartCoroutine(ShowResult(won));
    }

    private IEnumerator ShowResult(bool won)
    {
        if (resultPanelDelay > 0f)
            yield return new WaitForSecondsRealtime(resultPanelDelay);
        if (sceneLoader != null && sceneLoader.IsLoading) yield break;

        GameObject panel = won ? winPanel : gameOverPanel;
        if (panel == null) yield break;
        PanelFader fader = panel.GetComponent<PanelFader>();
        if (fader != null) fader.Show();
        else panel.SetActive(true);
    }

    public void ResetLevel()
    {
        if (sceneLoader == null)
        {
            Debug.LogError("LevelGameFlow chưa được gán Scene Loader.", this);
            return;
        }
        sceneLoader.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainMenu()
    {
        if (sceneLoader == null)
        {
            Debug.LogError("LevelGameFlow chưa được gán Scene Loader.", this);
            return;
        }
        sceneLoader.LoadScene("MainMenu");
    }
}
