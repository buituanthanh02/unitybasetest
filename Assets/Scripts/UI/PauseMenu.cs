using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    [Header("Existing gameplay objects")]
    [SerializeField] private LevelGameFlow levelGameFlow;
    [SerializeField] private SceneLoader sceneLoader;
    [SerializeField] private PlayerInteractor playerInteractor;
    [SerializeField] private CanvasGroup gameplayUI;

    [Header("Menus")]
    [SerializeField] private PanelFader pausePanel;
    [SerializeField] private AudioSettingsUI audioSettings;

    [Header("Movement and camera only - NOT PlayerInteractor")]
    [SerializeField] private Behaviour[] controlsToDisable = new Behaviour[0];

    private bool[] previousEnabled;
    private float previousTimeScale;
    private CursorLockMode previousCursorLock;
    private bool previousCursorVisible;
    private bool previousUIInteractable;
    private bool previousUIBlocksRaycasts;
    private Coroutine resumeRoutine;

    public bool IsPaused { get; private set; }

    private bool IsBlocked => levelGameFlow == null || sceneLoader == null ||
        levelGameFlow.HasEnded || sceneLoader.IsLoading;

    private void Awake()
    {
        if (pausePanel != null) pausePanel.HideImmediate();
    }

    private void Start()
    {
        if (levelGameFlow == null || sceneLoader == null || playerInteractor == null || pausePanel == null)
            Debug.LogError("PauseMenu: kiểm tra Level Game Flow, Scene Loader, " + "Player Interactor và Pause Panel.", this);
    }

    private void Update()
    {
        if (IsBlocked)
        {
            if (IsPaused) 
                CloseForEndOrLoading();
            return;
        }
        if (!Input.GetKeyDown(KeyCode.Escape)) 
            return;

        if (audioSettings != null && audioSettings.IsOpen)
        {
            audioSettings.Close();
            return;
        }
        if (pausePanel == null || pausePanel.IsAnimating) 
            return;
        if (IsPaused) 
            Resume();
        else Pause();
    }

    public void Pause()
    {
        if (IsPaused || IsBlocked || pausePanel == null || playerInteractor == null)
            return;

        IsPaused = true;
        previousTimeScale = Time.timeScale;
        previousCursorLock = Cursor.lockState;
        previousCursorVisible = Cursor.visible;

        previousEnabled = new bool[controlsToDisable.Length];
        for (int i = 0; i < controlsToDisable.Length; i++)
        {
            Behaviour control = controlsToDisable[i];
            // Interactor dùng khoá riêng để không mất mục tiêu đang đứng gần.
            if (control == null || control == this || control == playerInteractor)
                continue;
            previousEnabled[i] = control.enabled;
            control.enabled = false;
        }
        playerInteractor.SetPaused(true);

        if (gameplayUI != null)
        {
            previousUIInteractable = gameplayUI.interactable;
            previousUIBlocksRaycasts = gameplayUI.blocksRaycasts;
            gameplayUI.interactable = false;
            gameplayUI.blocksRaycasts = false;
        }

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        ClearSelection();
        pausePanel.Show();
    }

    public void Resume()
    {
        if (!IsPaused || IsBlocked || resumeRoutine != null) 
            return;
        if (audioSettings != null && audioSettings.IsOpen)
        {
            audioSettings.Close();
            return;
        }
        resumeRoutine = StartCoroutine(ResumeRoutine());
    }

    private IEnumerator ResumeRoutine()
    {
        pausePanel.Hide();
        do { yield return null; }
        while (pausePanel.IsVisible || Input.GetMouseButton(0));

        if (IsBlocked)
        {
            CloseForEndOrLoading();
            yield break;
        }

        for (int i = 0; i < controlsToDisable.Length; i++)
        {
            Behaviour control = controlsToDisable[i];
            if (control != null && control != this && control != playerInteractor)
                control.enabled = previousEnabled[i];
        }
        playerInteractor.SetPaused(false);
        RestoreGameplayUI();
        Time.timeScale = previousTimeScale;
        Cursor.lockState = previousCursorLock;
        Cursor.visible = previousCursorVisible;
        IsPaused = false;
        resumeRoutine = null;
        ClearSelection();
    }

    public void OpenSettings()
    {
        if (!IsPaused || IsBlocked || resumeRoutine != null) return;
        if (audioSettings != null) audioSettings.Open();
    }

    private void CloseForEndOrLoading()
    {
        if (resumeRoutine != null) StopCoroutine(resumeRoutine);
        resumeRoutine = null;
        if (audioSettings != null) audioSettings.CloseImmediate();
        if (pausePanel != null) pausePanel.HideImmediate();
        if (playerInteractor != null) playerInteractor.SetPaused(false);
        RestoreGameplayUI();
        // Không bật lại điều khiển: Loading / LevelGameFlow đang sở hữu khoá.
        if (sceneLoader == null || !sceneLoader.IsLoading)
            Time.timeScale = previousTimeScale;
        IsPaused = false;
        ClearSelection();
    }

    private void RestoreGameplayUI()
    {
        if (gameplayUI == null) return;
        gameplayUI.interactable = previousUIInteractable;
        gameplayUI.blocksRaycasts = previousUIBlocksRaycasts;
    }

    private static void ClearSelection()
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }
}
