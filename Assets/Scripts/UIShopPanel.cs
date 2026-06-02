using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIShopPanel : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI healthCostText;
    public TextMeshProUGUI healthLevelText;
    public TextMeshProUGUI staminaCostText;
    public TextMeshProUGUI staminaLevelText;
    public TextMeshProUGUI damageCostText;
    public TextMeshProUGUI damageLevelText;
    public TextMeshProUGUI coinsText;  // ← ДОБАВЬТЕ ЭТО поле для отображения монет

    public Button healthButton;
    public Button staminaButton;
    public Button damageButton;
    public Button closeButton;

    public GameObject shopPanel;

    private void Start()
    {
        // Назначаем обработчики
        if (healthButton != null)
            healthButton.onClick.AddListener(() => ShopManager.Instance?.BuyHealthUpgrade());

        if (staminaButton != null)
            staminaButton.onClick.AddListener(() => ShopManager.Instance?.BuyStaminaUpgrade());

        if (damageButton != null)
            damageButton.onClick.AddListener(() => ShopManager.Instance?.BuyDamageUpgrade());

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseShop);

        UpdateUI();

        if (shopPanel != null)
            shopPanel.SetActive(false);
    }

    private void OnEnable()
    {
        // Обновляем UI каждый раз когда панель становится активной
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (ShopManager.Instance == null) return;

        // Обновляем монеты
        if (coinsText != null && CoinManager.Instance != null)
        {
            coinsText.text = CoinManager.Instance.GetCurrentCoins().ToString();
            Debug.Log($"UIShopPanel: Обновление монет - {CoinManager.Instance.GetCurrentCoins()}");
        }

        // Обновляем стоимость и уровень улучшений
        if (healthCostText != null)
            healthCostText.text = ShopManager.Instance.GetHealthCost().ToString();

        if (healthLevelText != null)
            healthLevelText.text = $"{ShopManager.Instance.GetHealthLevel()}/{ShopManager.Instance.GetMaxHealthUpgrades()}";

        if (staminaCostText != null)
            staminaCostText.text = ShopManager.Instance.GetStaminaCost().ToString();

        if (staminaLevelText != null)
            staminaLevelText.text = $"{ShopManager.Instance.GetStaminaLevel()}/{ShopManager.Instance.GetMaxStaminaUpgrades()}";

        if (damageCostText != null)
            damageCostText.text = ShopManager.Instance.GetDamageCost().ToString();

        if (damageLevelText != null)
            damageLevelText.text = $"{ShopManager.Instance.GetDamageLevel()}/{ShopManager.Instance.GetMaxDamageUpgrades()}";

        // Блокируем кнопки если достигнут максимум
        if (healthButton != null)
            healthButton.interactable = ShopManager.Instance.GetHealthLevel() < ShopManager.Instance.GetMaxHealthUpgrades();

        if (staminaButton != null)
            staminaButton.interactable = ShopManager.Instance.GetStaminaLevel() < ShopManager.Instance.GetMaxStaminaUpgrades();

        if (damageButton != null)
            damageButton.interactable = ShopManager.Instance.GetDamageLevel() < ShopManager.Instance.GetMaxDamageUpgrades();
    }

    public void OpenShop()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        // Принудительно обновляем UI перед открытием
        UpdateUI();

        if (shopPanel != null)
            shopPanel.SetActive(true);
    }

    public void CloseShop()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        if (shopPanel != null)
            shopPanel.SetActive(false);
    }
}