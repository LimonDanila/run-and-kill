using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelComplete : MonoBehaviour
{
    public int currentLevelNumber;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            UnlockNextLevel();
        }
    }

    private void UnlockNextLevel()
    {
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
        SceneManager.LoadScene("Levels");
    }

    public void BackToLevelsButton()
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
