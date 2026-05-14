using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelComplete : MonoBehaviour
{
    public int currentLevelNumber; // 1, 2, 3 или 4 для босса

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            UnlockNextLevel();
        }
    }

    private void UnlockNextLevel()
    {
        // Открываем следующий уровень
        switch (currentLevelNumber)
        {
            case 1:
                PlayerPrefs.SetInt("Level2Unlocked", 1);
                break;
            case 2:
                PlayerPrefs.SetInt("Level3Unlocked", 1);
                break;
            case 3:
                PlayerPrefs.SetInt("LevelBossUnlocked", 1);
                break;
            case 4:
                Debug.Log("Игра пройдена!");
                break;
        }

        PlayerPrefs.Save();

        Invoke("BackToLevels", 1f);
        //SceneManager.LoadScene("Levels");
    }

    public void BackToLevels()
    {
        PlayButtonSound();
        SceneManager.LoadScene("Levels");
    }

    private void PlayButtonSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }
}
