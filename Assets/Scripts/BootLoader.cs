using UnityEngine;
using UnityEngine.SceneManagement;

public class BootLoader : MonoBehaviour
{
    private void Start()
    {
        // Загружаем главное меню
        SceneManager.LoadScene("Menu");
    }
}