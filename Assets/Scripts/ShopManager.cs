using UnityEngine;
using TMPro;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    [Header("Health Upgrade")]
    public int healthUpgradeCost = 50;
    public int healthIncreaseAmount = 40;

    [Header("Stamina Upgrade")]
    public int staminaUpgradeCost = 30;
    public int staminaIncreaseAmount = 1;

    [Header("Damage Upgrade")]
    public int damageUpgradeCost = 40;
    public int damageIncreaseAmount = 5;

    [Header("Limits")]
    public int maxHealthUpgrades = 4;
    public int maxStaminaUpgrades = 3;
    public int maxDamageUpgrades = 5;

    private int healthLevel = 0;
    private int staminaLevel = 0;
    private int damageLevel = 0;

    private void Awake()
    {
        // Синглтон - сохраняем между сценами
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("ShopManager создан на сцене Menu");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadProgress();
        Debug.Log($"ShopManager загружен: Здоровье ур.{healthLevel}, Стамина ур.{staminaLevel}, Урон ур.{damageLevel}");
    }

    public void BuyHealthUpgrade()
    {
        if (healthLevel >= maxHealthUpgrades)
        {
            Debug.Log("Максимальный уровень здоровья достигнут!");
            return;
        }

        if (CoinManager.Instance == null)
        {
            Debug.LogError("CoinManager.Instance = null!");
            return;
        }

        if (CoinManager.Instance.GetCurrentCoins() >= healthUpgradeCost)
        {
            CoinManager.Instance.SpendCoins(healthUpgradeCost);
            healthLevel++;
            healthUpgradeCost += 20;
            SaveProgress();

            // Обновляем UI на текущей сцене
            UpdateAllUI();

            Debug.Log($"Куплено улучшение здоровья! Уровень: {healthLevel}/{maxHealthUpgrades}");
        }
        else
        {
            Debug.Log($"Недостаточно монет! Нужно: {healthUpgradeCost}");
        }
    }

    public void BuyStaminaUpgrade()
    {
        if (staminaLevel >= maxStaminaUpgrades)
        {
            Debug.Log("Максимальный уровень стамины достигнут!");
            return;
        }

        if (CoinManager.Instance != null && CoinManager.Instance.GetCurrentCoins() >= staminaUpgradeCost)
        {
            CoinManager.Instance.SpendCoins(staminaUpgradeCost);
            staminaLevel++;
            staminaUpgradeCost += 15;
            SaveProgress();
            UpdateAllUI();

            Debug.Log($"Куплено улучшение стамины! Уровень: {staminaLevel}/{maxStaminaUpgrades}");
        }
        else
        {
            Debug.Log("Недостаточно монет для улучшения стамины!");
        }
    }

    public void BuyDamageUpgrade()
    {
        if (damageLevel >= maxDamageUpgrades)
        {
            Debug.Log("Максимальный уровень урона достигнут!");
            return;
        }

        if (CoinManager.Instance != null && CoinManager.Instance.GetCurrentCoins() >= damageUpgradeCost)
        {
            CoinManager.Instance.SpendCoins(damageUpgradeCost);
            damageLevel++;
            damageUpgradeCost += 25;
            SaveProgress();
            UpdateAllUI();

            Debug.Log($"Куплено улучшение урона! Уровень: {damageLevel}/{maxDamageUpgrades}");
        }
        else
        {
            Debug.Log("Недостаточно монет для улучшения урона!");
        }
    }

    private void SaveProgress()
    {
        PlayerPrefs.SetInt("HealthLevel", healthLevel);
        PlayerPrefs.SetInt("StaminaLevel", staminaLevel);
        PlayerPrefs.SetInt("DamageLevel", damageLevel);
        PlayerPrefs.SetInt("HealthCost", healthUpgradeCost);
        PlayerPrefs.SetInt("StaminaCost", staminaUpgradeCost);
        PlayerPrefs.SetInt("DamageCost", damageUpgradeCost);
        PlayerPrefs.Save();
    }

    private void LoadProgress()
    {
        healthLevel = PlayerPrefs.GetInt("HealthLevel", 0);
        staminaLevel = PlayerPrefs.GetInt("StaminaLevel", 0);
        damageLevel = PlayerPrefs.GetInt("DamageLevel", 0);
        healthUpgradeCost = PlayerPrefs.GetInt("HealthCost", 50);
        staminaUpgradeCost = PlayerPrefs.GetInt("StaminaCost", 30);
        damageUpgradeCost = PlayerPrefs.GetInt("DamageCost", 40);
    }

    // Методы для получения данных
    public int GetHealthLevel() => healthLevel;
    public int GetStaminaLevel() => staminaLevel;
    public int GetDamageLevel() => damageLevel;
    public int GetHealthCost() => healthUpgradeCost;
    public int GetStaminaCost() => staminaUpgradeCost;
    public int GetDamageCost() => damageUpgradeCost;
    public int GetMaxHealthUpgrades() => maxHealthUpgrades;
    public int GetMaxStaminaUpgrades() => maxStaminaUpgrades;
    public int GetMaxDamageUpgrades() => maxDamageUpgrades;

    // Обновление UI на текущей сцене
    private void UpdateAllUI()
    {
        UIShopPanel uiPanel = FindObjectOfType<UIShopPanel>();
        if (uiPanel != null)
        {
            uiPanel.UpdateUI();
        }
    }

    public void ReloadProgress()
    {
        LoadProgress();
        Debug.Log($"ShopManager перезагружен: Health={healthLevel}, Stamina={staminaLevel}, Damage={damageLevel}");
        UpdateAllUI();
    }
}