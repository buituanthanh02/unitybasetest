using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    [Header("Loading UI")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text progressText;

    [Header("Controls")]
    [SerializeField] private Behaviour[] controlsToDisable =
        new Behaviour[0];

    [Header("Timing")]
    [SerializeField, Min(0f)]
    private float minimumDisplayTime = 0.35f;

    private bool isLoading;

    private void Awake()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }

        SetProgress(0f);
    }

    public void LoadScene(string sceneName)
    {
        if (isLoading)
        {
            return;
        }

        if (loadingPanel == null)
        {
            Debug.LogError(
                "SceneLoader chưa được gán Loading Panel.",
                this
            );
            return;
        }

        if (string.IsNullOrWhiteSpace(sceneName) ||
            !Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                $"Không thể tải scene '{sceneName}'. " +
                "Kiểm tra tên và danh sách scene trong Build Settings.",
                this
            );
            return;
        }

        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        isLoading = true;

        AsyncOperation operation = SceneManager.LoadSceneAsync(
            sceneName,
            LoadSceneMode.Single
        );

        if (operation == null)
        {
            isLoading = false;
            Debug.LogError("Không thể bắt đầu tải scene.", this);
            yield break;
        }

        operation.allowSceneActivation = false;

        SetProgress(0f);
        loadingPanel.SetActive(true);

        foreach (Behaviour control in controlsToDisable)
        {
            if (control != null)
            {
                control.enabled = false;
            }
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;

        float elapsed = 0f;

        while (operation.progress < 0.9f ||
               elapsed < minimumDisplayTime)
        {
            elapsed += Time.unscaledDeltaTime;

            float loadingProgress =
                Mathf.Clamp01(operation.progress / 0.9f);

            float displayProgress = minimumDisplayTime > 0f
                ? Mathf.Clamp01(elapsed / minimumDisplayTime)
                : 1f;

            SetProgress(
                Mathf.Min(loadingProgress, displayProgress)
            );

            yield return null;
        }

        SetProgress(1f);

        // Cho UI có một frame hiển thị 100%.
        yield return null;

        Time.timeScale = 1f;
        operation.allowSceneActivation = true;
    }

    private void SetProgress(float progress)
    {
        if (progressBar != null)
        {
            progressBar.normalizedValue = progress;
        }

        if (progressText != null)
        {
            progressText.text =
                $"Đang tải... {Mathf.RoundToInt(progress * 100f)}%";
        }
    }

    public void QuitGame()
    {
        if (isLoading) 
        {
            return;
        }

#if UNITY_EDITOR
        Debug.Log(
            "Đã bấm Thoát. Trong bản build, ứng dụng sẽ đóng.",
            this
        );
#else
        Application.Quit();
#endif
    }
}