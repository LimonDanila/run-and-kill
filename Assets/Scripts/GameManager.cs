using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject panelSettings;

    private void Start()
    {
        if (panelSettings != null)
            panelSettings.SetActive(false);
    }

    private void ResetProgressButton()
    {
        ResetProgress();
    }

    private void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt("Level1Unlocked", 1);
        PlayerPrefs.Save();
        Debug.Log("Прогресс сброшен при запуске (только для тестирования)");
    }

    public void Play()
    {
        PlayButtonSound();
        SceneManager.LoadScene("Levels");
    }

    public void Back()
    {
        PlayButtonSound();
        SceneManager.LoadScene("Menu");
    }

    public void Exit()
    {
        PlayButtonSound();
        Application.Quit();
    }

    public void OpenSettings()
    {
        PlayButtonSound();
        if (panelSettings != null)
            panelSettings.SetActive(true);
    }

    public void CloseSettings()
    {
        PlayButtonSound();
        if (panelSettings != null)
            panelSettings.SetActive(false);
    }

    public void ResetFullProgress()
    {
        PlayButtonSound();

        // 1. Сбрасываем прогресс уровней
        PlayerPrefs.SetInt("Level1Unlocked", 1);
        PlayerPrefs.SetInt("Level2Unlocked", 0);
        PlayerPrefs.SetInt("Level3Unlocked", 0);
        PlayerPrefs.SetInt("LevelBossUnlocked", 0);

        // 2. Сбрасываем улучшения магазина
        PlayerPrefs.SetInt("HealthLevel", 0);
        PlayerPrefs.SetInt("StaminaLevel", 0);
        PlayerPrefs.SetInt("DamageLevel", 0);
        PlayerPrefs.SetInt("HealthCost", 50);
        PlayerPrefs.SetInt("StaminaCost", 30);
        PlayerPrefs.SetInt("DamageCost", 40);

        // 3. Сбрасываем монеты
        PlayerPrefs.SetInt("PlayerCoins", 0);

        // 4. Сбрасываем сохранённые характеристики (если есть)
        PlayerPrefs.SetInt("SavedMaxHealth", 100);
        PlayerPrefs.SetInt("SavedMaxStamina", 4);
        PlayerPrefs.SetInt("SavedLightDamage", 10);
        PlayerPrefs.SetInt("SavedHeavyDamage", 25);

        PlayerPrefs.Save();

        Debug.Log("=== ВЕСЬ ПРОГРЕСС ПОЛНОСТЬЮ СБРОШЕН ===");
        Debug.Log("Уровни: открыт только уровень 1");
        Debug.Log("Улучшения: все сброшены");
        Debug.Log("Монеты: 0");

        // Обновляем UI если находимся на сцене Levels
        UpdateAllUIs();
    }

    private void UpdateAllUIs()
    {
        // Обновляем магазин если открыт
        if (ShopManager.Instance != null)
        {
            // Принудительно перезагружаем прогресс в ShopManager
            ShopManager.Instance.ReloadProgress();
        }

        // Обновляем LevelManager если на сцене Levels
        LevelManager levelManager = FindObjectOfType<LevelManager>();
        if (levelManager != null)
        {
            levelManager.UpdateButtonsState();
        }

        // Обновляем отображение монет
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.ReloadCoins();
        }
    }

    private void PlayButtonSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }
}
