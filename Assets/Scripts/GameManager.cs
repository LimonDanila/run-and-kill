using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject panelSettings;

    private void Start()
    {
        if (panelSettings != null)
            panelSettings.SetActive(false);
        ResetProgressOnStart();
    }

    private void ResetProgressOnStart()
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

    private void PlayButtonSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }
}
