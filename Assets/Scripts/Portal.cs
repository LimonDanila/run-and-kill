using UnityEngine;

public class Portal : MonoBehaviour
{
    [Header("Portal Settings")]
    public int levelNumber = 1;              // Номер текущего уровня
    public bool completeOnTouch = true;      // Завершать уровень при касании

    [Header("Effects")]
    public GameObject portalEffect;          // Эффект портала (опционально)

    private bool isActivated = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isActivated) return;

        if (other.CompareTag("Player"))
        {
            isActivated = true;

            // Эффекты
            if (portalEffect != null)
                Instantiate(portalEffect, transform.position, Quaternion.identity);

            // Мгновенное завершение уровня
            CompleteLevel();
        }
    }

    void CompleteLevel()
    {
        Debug.Log($"Portal: уровень {levelNumber} завершён!");

        // Сохраняем прогресс
        UnlockNextLevel();

        // Загружаем сцену выбора уровней
        LevelComplete levelComplete = GetComponent<LevelComplete>();
        if (levelComplete != null)
        {
            levelComplete.BackToLevels();
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Levels");
        }
    }

    void UnlockNextLevel()
    {
        switch (levelNumber)
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
    }
}