using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private PanelFader settingsPanel;
    [SerializeField] private CanvasGroup backgroundUI;
    [SerializeField] private bool closeWithEscape = true;

    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider uiSlider;

    private const string MusicKey = "3D-PV.Volume.Music";
    private const string SfxKey = "3D-PV.Volume.SFX";
    private const string UiKey = "3D-PV.Volume.UI";

    private bool previousInteractable;
    private bool previousBlocksRaycasts;
    private bool backgroundLocked;
    private bool dirty;
    private Coroutine closeRoutine;

    // Giữ true cả lúc fade-out, để Esc không vô tình Resume ngay.
    public bool IsOpen => settingsPanel != null && settingsPanel.IsVisible;

    private void Awake()
    {
        if (settingsPanel != null) settingsPanel.HideImmediate();
    }

    private void Start()
    {
        InitializeSlider(musicSlider, MusicKey, "MusicVolume");
        InitializeSlider(sfxSlider, SfxKey, "SFXVolume");
        InitializeSlider(uiSlider, UiKey, "UIVolume");

        // Tự nối slider bằng code: không cần thêm On Value Changed ở Inspector.
        if (musicSlider != null) musicSlider.onValueChanged.AddListener(SetMusic);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(SetSfx);
        if (uiSlider != null) uiSlider.onValueChanged.AddListener(SetUi);
    }

    private void Update()
    {
        if (closeWithEscape && IsOpen && Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    private void InitializeSlider(Slider slider, string key, string parameter)
    {
        float value = Mathf.Clamp01(PlayerPrefs.GetFloat(key, 1f));
        if (slider != null) slider.SetValueWithoutNotify(value);
        ApplyVolume(parameter, value);
    }

    private void SetMusic(float value)
    {
        ChangeVolume(MusicKey, "MusicVolume", value);
    }

    private void SetSfx(float value)
    {
        ChangeVolume(SfxKey, "SFXVolume", value);
    }

    private void SetUi(float value)
    {
        ChangeVolume(UiKey, "UIVolume", value);
    }

    private void ChangeVolume(string key, string parameter, float value)
    {
        value = Mathf.Clamp01(value);
        ApplyVolume(parameter, value);
        PlayerPrefs.SetFloat(key, value);
        dirty = true;
    }

    private void ApplyVolume(string parameter, float value)
    {
        if (audioMixer == null)
        {
            Debug.LogError("AudioSettingsUI chưa được gán Audio Mixer.", this);
            return;
        }

        float decibels = 20f * Mathf.Log10(Mathf.Max(0.0001f, value));
        if (!audioMixer.SetFloat(parameter, decibels))
            Debug.LogError("Kiểm tra Exposed Parameter: " + parameter, this);
    }

    public void Open()
    {
        if (settingsPanel == null || IsOpen) return;
        if (backgroundUI != null)
        {
            previousInteractable = backgroundUI.interactable;
            previousBlocksRaycasts = backgroundUI.blocksRaycasts;
            backgroundLocked = true;
            backgroundUI.interactable = false;
            backgroundUI.blocksRaycasts = false;
        }
        ClearSelection();
        settingsPanel.Show();
    }

    public void Close()
    {
        if (!IsOpen || closeRoutine != null) return;
        SaveIfChanged();
        closeRoutine = StartCoroutine(CloseRoutine());
    }

    private IEnumerator CloseRoutine()
    {
        settingsPanel.Hide();
        // Có ít nhất một frame chờ, kể cả khi Duration = 0.
        do { yield return null; } while (settingsPanel.IsVisible);
        RestoreBackground();
        ClearSelection();
        closeRoutine = null;
    }

    public void CloseImmediate()
    {
        if (closeRoutine != null) StopCoroutine(closeRoutine);
        closeRoutine = null;
        if (settingsPanel != null) settingsPanel.HideImmediate();
        RestoreBackground();
        SaveIfChanged();
    }

    private void RestoreBackground()
    {
        if (!backgroundLocked) return;
        if (backgroundUI != null)
        {
            backgroundUI.interactable = previousInteractable;
            backgroundUI.blocksRaycasts = previousBlocksRaycasts;
        }
        backgroundLocked = false;
    }

    private void SaveIfChanged()
    {
        if (!dirty) return;
        PlayerPrefs.Save();
        dirty = false;
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused) SaveIfChanged();
    }

    private void OnDestroy()
    {
        if (musicSlider != null) musicSlider.onValueChanged.RemoveListener(SetMusic);
        if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(SetSfx);
        if (uiSlider != null) uiSlider.onValueChanged.RemoveListener(SetUi);
        SaveIfChanged();
    }

    private static void ClearSelection()
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }
}
