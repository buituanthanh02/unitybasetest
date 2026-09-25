using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class PanelFader : MonoBehaviour
{
    [SerializeField, Min(0f)] private float duration = 0.2f;

    private CanvasGroup group;
    private Coroutine fadeRoutine;
    private bool targetVisible;

    public bool IsVisible => gameObject.activeSelf;
    public bool IsAnimating { get; private set; }

    private void Awake()
    {
        group = GetComponent<CanvasGroup>();
    }

    public void Show()
    {
        if (IsVisible && targetVisible) return;
        Prepare();
        if (!gameObject.activeSelf) group.alpha = 0f;
        gameObject.SetActive(true);
        targetVisible = true;
        fadeRoutine = StartCoroutine(FadeRoutine(true));
    }

    public void Hide()
    {
        if (!IsVisible || (!targetVisible && IsAnimating)) return;
        Prepare();
        targetVisible = false;
        fadeRoutine = StartCoroutine(FadeRoutine(false));
    }

    public void HideImmediate()
    {
        Prepare();
        targetVisible = false;
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    private void Prepare()
    {
        if (group == null) group = GetComponent<CanvasGroup>();
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = null;
        IsAnimating = false;
    }

    private IEnumerator FadeRoutine(bool show)
    {
        IsAnimating = true;
        group.interactable = false;
        // Vẫn chặn click xuống bên dưới trong lúc panel đang biến mất.
        group.blocksRaycasts = true;

        float startAlpha = group.alpha;
        float targetAlpha = show ? 1f : 0f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(startAlpha, targetAlpha,
                Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        group.alpha = targetAlpha;
        group.interactable = show;
        group.blocksRaycasts = show;
        IsAnimating = false;
        fadeRoutine = null;
        if (!show) gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = null;
        IsAnimating = false;
        targetVisible = false;
    }
}
