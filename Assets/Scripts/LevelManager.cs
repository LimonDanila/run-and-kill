using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public Button[] levelButtons; // Назначьте кнопки 1,2,3,BOSS в инспекторе

    private void Start()
    {
        LoadLevelProgress();
        UpdateButtonsState();
    }

    private void LoadLevelProgress()
    {
        // Загружаем прогресс (по умолчанию первый уровень открыт)
        if (!PlayerPrefs.HasKey("Level1Unlocked"))
        {
            PlayerPrefs.SetInt("Level1Unlocked", 1);
            PlayerPrefs.SetInt("Level2Unlocked", 0);
            PlayerPrefs.SetInt("Level3Unlocked", 0);
            PlayerPrefs.SetInt("LevelBossUnlocked", 0);
            PlayerPrefs.Save();
        }
    }

    private void UpdateButtonsState()
    {
        // Обновляем состояние кнопок
        levelButtons[0].interactable = PlayerPrefs.GetInt("Level1Unlocked") == 1;
        levelButtons[1].interactable = PlayerPrefs.GetInt("Level2Unlocked") == 1;
        levelButtons[2].interactable = PlayerPrefs.GetInt("Level3Unlocked") == 1;
        levelButtons[3].interactable = PlayerPrefs.GetInt("LevelBossUnlocked") == 1;
    }

    public void LoadLevel(int levelIndex)
    {
        PlayButtonSound();
        string levelName = "";

        switch (levelIndex)
        {
            case 1:
                if (PlayerPrefs.GetInt("Level1Unlocked") == 1)
                    levelName = "Level1";
                break;
            case 2:
                if (PlayerPrefs.GetInt("Level2Unlocked") == 1)
                    levelName = "Level2";
                break;
            case 3:
                if (PlayerPrefs.GetInt("Level3Unlocked") == 1)
                    levelName = "Level3";
                break;
            case 4:
                if (PlayerPrefs.GetInt("LevelBossUnlocked") == 1)
                    levelName = "LevelBoss";
                break;
        }

        if (!string.IsNullOrEmpty(levelName))
        {
            SceneManager.LoadScene(levelName);
        }
        else
        {
            Debug.Log("Уровень заблокирован!");
        }
    }

    public void OpenShop()
    {
        PlayButtonSound();
        SceneManager.LoadScene("Shop");
    }

    public void BackToMenu()
    {
        PlayButtonSound();
        SceneManager.LoadScene("Menu");
    }

    private void PlayButtonSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }
}
