using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public Button[] levelButtons;

    private void Start()
    {
        LoadLevelProgress();
        UpdateButtonsState();
    }

    private void LoadLevelProgress()
    {
        if (!PlayerPrefs.HasKey("Level1Unlocked"))
        {
            PlayerPrefs.SetInt("Level1Unlocked", 1);
            PlayerPrefs.SetInt("Level2Unlocked", 0);
            PlayerPrefs.SetInt("Level3Unlocked", 0);
            PlayerPrefs.SetInt("LevelBossUnlocked", 0);
            PlayerPrefs.Save();
        }
    }

    public void UpdateButtonsState()
    {
        // Проверяем, что массив кнопок не пустой и имеет нужное количество элементов
        if (levelButtons == null || levelButtons.Length < 4)
        {
            Debug.LogWarning("LevelButtons массив не полностью назначен!");
            return;
        }

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
                    levelName = "Boss";
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

    private void UnlockAllLevelsForTest()
    {
        // Проверяем, включен ли тестовый режим
        if (Debug.isDebugBuild)
        {
            PlayerPrefs.SetInt("Level1Unlocked", 1);
            PlayerPrefs.SetInt("Level2Unlocked", 1);
            PlayerPrefs.SetInt("Level3Unlocked", 1);
            PlayerPrefs.SetInt("LevelBossUnlocked", 1);
            PlayerPrefs.Save();
            Debug.Log("Тестовый режим: все уровни разблокированы!");
        }
    }
}
