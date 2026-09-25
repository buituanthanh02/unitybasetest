using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    [Header("Loading UI")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text progressText;

    [Header("Controls")]
    [SerializeField] private Behaviour[] controlsToDisable = new Behaviour[0];

    [Header("Timing")]
    [SerializeField, Min(0f)] private float minimumDisplayTime = 0.35f;

    [Header("Screen Fade")]
    [SerializeField] private CanvasGroup fadePanel;
    [SerializeField, Min(0f)] private float fadeDuration = 0.35f;

    // Bao gồm cả fade-in khi vừa vào scene.
    private bool isLoading = true;
    public bool IsLoading => isLoading;

    private void Awake()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (HasValidLoadingPanel()) loadingPanel.SetActive(false);
        SetProgress(0f);
        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            fadePanel.alpha = 1f;
            fadePanel.interactable = false;
            fadePanel.blocksRaycasts = true;
        }
    }

    private IEnumerator Start()
    {
        // Start chạy sau Awake của LevelGameFlow (nơi cũng đặt timeScale).
        Time.timeScale = 0f;
        // Cho camera một LateUpdate để tới đúng vị trí khi màn hình còn đen.
        yield return null;
        bool[] previousEnabled = DisableControls(true);
        yield return FadeScreen(0f);
        RestoreControls(previousEnabled);
        Time.timeScale = 1f;
        if (fadePanel != null) fadePanel.blocksRaycasts = false;
        isLoading = false;
    }

    public void LoadScene(string sceneName)
    {
        if (isLoading || !isActiveAndEnabled) return;
        if (!HasValidLoadingPanel()) return;
        if (string.IsNullOrWhiteSpace(sceneName) ||
            !Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"Không thể tải scene '{sceneName}'. " +
                "Kiểm tra tên và danh sách scene trong Build Settings.", this);
            return;
        }

        AsyncOperation operation = SceneManager.LoadSceneAsync(
            sceneName, LoadSceneMode.Single);
        if (operation == null)
        {
            Debug.LogError("Không thể bắt đầu tải scene.", this);
            return;
        }
        operation.allowSceneActivation = false;
        isLoading = true;
        StartCoroutine(LoadSceneRoutine(operation));
    }

    private IEnumerator LoadSceneRoutine(AsyncOperation operation)
    {
        DisableControls();
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
        if (fadePanel != null) fadePanel.blocksRaycasts = true;

        yield return FadeScreen(1f);
        SetProgress(0f);
        loadingPanel.SetActive(true);
        // LoadingPanel có nền đen giống FadePanel, nên không lộ scene phía dưới.
        if (fadePanel != null) fadePanel.alpha = 0f;

        float elapsed = 0f;
        while (operation.progress < 0.9f || elapsed < minimumDisplayTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float loadingProgress = Mathf.Clamp01(operation.progress / 0.9f);
            float displayProgress = minimumDisplayTime > 0f
                ? Mathf.Clamp01(elapsed / minimumDisplayTime) : 1f;
            SetProgress(Mathf.Min(loadingProgress, displayProgress));
            yield return null;
        }
        SetProgress(1f);
        yield return null;
        yield return FadeScreen(1f);
        Time.timeScale = 1f;
        operation.allowSceneActivation = true;
        // Scene mới có SceneLoader riêng tự fade từ đen sang sáng.
    }

    private IEnumerator FadeScreen(float target)
    {
        if (fadePanel == null) yield break;
        float start = fadePanel.alpha;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadePanel.alpha = Mathf.Lerp(start, target,
                Mathf.Clamp01(elapsed / fadeDuration));
            yield return null;
        }
        fadePanel.alpha = target;
    }

    private bool[] DisableControls(bool enteringScene = false)
    {
        bool[] previous = new bool[controlsToDisable.Length];
        for (int i = 0; i < controlsToDisable.Length; i++)
        {
            Behaviour control = controlsToDisable[i];
            if (control == null || control == this) continue;
            previous[i] = control.enabled;
            // Fade vào không xoá target nếu Player đã xuất hiện trong một zone.
            if (enteringScene && control is PlayerInteractor interactor)
                interactor.SetPaused(true);
            else
                control.enabled = false;
        }
        return previous;
    }

    private void RestoreControls(bool[] previous)
    {
        for (int i = 0; i < controlsToDisable.Length; i++)
        {
            Behaviour control = controlsToDisable[i];
            if (control == null || control == this) continue;
            if (control is PlayerInteractor interactor) interactor.SetPaused(false);
            control.enabled = previous[i];
        }
    }

    private bool HasValidLoadingPanel()
    {
        if (loadingPanel == null || loadingPanel == gameObject ||
            transform.IsChildOf(loadingPanel.transform))
        {
            Debug.LogError("Loading Panel phải là object con LoadingPanel, " +
                "không phải LoadingUI chứa SceneLoader.", this);
            return false;
        }
        return true;
    }

    private void SetProgress(float progress)
    {
        if (progressBar != null) progressBar.normalizedValue = progress;
        if (progressText != null)
            progressText.text = $"Đang tải... {Mathf.RoundToInt(progress * 100f)}%";
    }

    public void QuitGame()
    {
        if (isLoading) return;
        #if UNITY_EDITOR
            Debug.Log("Đã bấm Thoát. Trong bản build, ứng dụng sẽ đóng.", this);
        #else
            Application.Quit();
        #endif
    } 
}
