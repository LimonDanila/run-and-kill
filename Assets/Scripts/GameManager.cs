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
        SceneManager.LoadScene("Levels");
    }

    public void Back()
    {
        SceneManager.LoadScene("Menu");
    }

    public void Exit()
    {
        Application.Quit();
    }
}
