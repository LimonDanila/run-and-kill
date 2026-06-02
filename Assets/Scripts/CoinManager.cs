using UnityEngine;
using TMPro;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    [Header("UI References")]
    public TextMeshProUGUI coinText;

    [Header("Settings")]
    public int startCoins = 0;

    private int currentCoins;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("CoinManager создан");
        }
        else
        {
            Debug.Log("CoinManager уже существует, уничтожаем дубликат");
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        LoadCoins();

        // Ищем UI текст на текущей сцене
        FindAndAttachCoinText();

        UpdateCoinUI();
    }

    // Этот метод вызывается при загрузке новой сцены
    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Когда загружается новая сцена - ищем UI текст заново
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        Debug.Log($"Сцена загружена: {scene.name}");

        // Ищем текст монет на новой сцене
        FindAndAttachCoinText();

        // Обновляем отображение
        UpdateCoinUI();
    }

    // Поиск текста монет на текущей сцене
    private void FindAndAttachCoinText()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // На сцене Menu не ищем монеты (там они не нужны)
        if (sceneName == "Menu")
        {
            coinText = null;
            Debug.Log("Сцена Menu - отключаем отображение монет");
            return;
        }

        // Ищем объект с именем "CoinText"
        GameObject textObject = GameObject.Find("CoinText");

        if (textObject != null)
        {
            coinText = textObject.GetComponent<TextMeshProUGUI>();
            if (coinText != null)
            {
                Debug.Log($"CoinText найден на сцене {sceneName}");
            }
        }
        else
        {
            Debug.LogWarning($"CoinText не найден на сцене {sceneName}");
        }
    }

    public void AddCoins(int amount)
    {
        currentCoins += amount + 100;
        UpdateCoinUI();
        SaveCoins();
        Debug.Log($"+{amount} монет! Всего: {currentCoins}");
    }

    public bool SpendCoins(int amount)
    {
        if (currentCoins >= amount)
        {
            currentCoins -= amount;
            UpdateCoinUI();
            SaveCoins();
            Debug.Log($"Потрачено {amount} монет. Осталось: {currentCoins}");
            return true;
        }
        else
        {
            Debug.Log($"Недостаточно монет! Нужно: {amount}, есть: {currentCoins}");
            return false;
        }
    }

    public int GetCurrentCoins()
    {
        return currentCoins;
    }

    private void UpdateCoinUI()
    {
        if (coinText != null)
        {
            coinText.text = currentCoins.ToString();
            Debug.Log($"UI обновлён: {currentCoins} монет");
        }
        else
        {
            // Если текста нет, пробуем найти его снова
            FindAndAttachCoinText();
            if (coinText != null)
            {
                coinText.text = currentCoins.ToString();
            }
            else
            {
                Debug.LogWarning($"CoinText не назначен! Монет: {currentCoins}");
            }
        }
    }

    private void SaveCoins()
    {
        PlayerPrefs.SetInt("PlayerCoins", currentCoins);
        PlayerPrefs.Save();
        Debug.Log($"Монеты сохранены: {currentCoins}");
    }

    private void LoadCoins()
    {
        if (PlayerPrefs.HasKey("PlayerCoins"))
        {
            currentCoins = PlayerPrefs.GetInt("PlayerCoins");
            Debug.Log($"Монеты загружены из сохранения: {currentCoins}");
        }
        else
        {
            currentCoins = startCoins;
            Debug.Log($"Нет сохранения, установлено начальное значение: {currentCoins}");
        }
    }

    public void ResetCoins()
    {
        currentCoins = startCoins;
        UpdateCoinUI();
        SaveCoins();
        Debug.Log("Монеты сброшены!");
    }

    public void ReloadCoins()
    {
        LoadCoins();
        UpdateCoinUI();
        Debug.Log($"CoinManager перезагружен: монет = {currentCoins}");
    }
}