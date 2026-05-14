using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SettingsMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider musicSlider;
    public Slider sfxSlider;
    public GameObject settingsPanel;

    [Header("Animation Settings")]
    public float animationDuration = 0.3f;
    public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Vector3 originalScale;
    private bool isOpen = false;
    private bool isAnimating = false;
    private CanvasGroup canvasGroup;

    private void Start()
    {
        // Добавляем CanvasGroup для плавного появления
        if (settingsPanel != null)
        {
            canvasGroup = settingsPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = settingsPanel.AddComponent<CanvasGroup>();
            }

            originalScale = settingsPanel.transform.localScale;
            settingsPanel.SetActive(false);
        }

        // Загружаем сохраненные значения
        LoadSettings();

        // Назначаем обработчики
        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
    }

    private void LoadSettings()
    {
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.7f);

        if (musicSlider != null)
            musicSlider.value = musicVolume;

        if (sfxSlider != null)
            sfxSlider.value = sfxVolume;
    }

    private void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }
    }

    public void OpenSettings()
    {
        if (settingsPanel == null || isAnimating) return;

        // Воспроизводим звук кнопки
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        settingsPanel.SetActive(true);
        StartCoroutine(AnimateOpen());
    }

    public void CloseSettings()
    {
        if (settingsPanel == null || isAnimating || !isOpen) return;

        // Воспроизводим звук кнопки
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        // Просто закрываем без анимации
        settingsPanel.SetActive(false);
        isOpen = false;
    }

    private IEnumerator AnimateOpen()
    {
        isAnimating = true;
        isOpen = true;

        // Начальные значения
        settingsPanel.transform.localScale = Vector3.zero;
        if (canvasGroup != null) canvasGroup.alpha = 0;

        float elapsed = 0;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            float easedT = animationCurve.Evaluate(t);

            // Анимация масштаба
            settingsPanel.transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, easedT);

            // Плавное появление
            if (canvasGroup != null)
                canvasGroup.alpha = easedT;

            yield return null;
        }

        // Финальные значения
        settingsPanel.transform.localScale = originalScale;
        if (canvasGroup != null) canvasGroup.alpha = 1;

        isAnimating = false;
    }

    // Метод для принудительного закрытия (без звука)
    public void ForceClose()
    {
        if (settingsPanel != null)
        {
            StopAllCoroutines();
            settingsPanel.SetActive(false);
            isOpen = false;
            isAnimating = false;
        }
    }

    private void Update()
    {
        // Закрытие по Escape
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseSettings();
        }
    }
}