using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("UI References")]
    public Image fillImage;      // Перетащите сюда Fill (Image)
    public TextMeshProUGUI healthText;       // Перетащите сюда HealthText

    [Header("Colors")]
    public Color healthyColor = Color.green;
    public Color damagedColor = Color.yellow;
    public Color criticalColor = Color.red;

    private HeroMove heroMove;
    private int maxHealth;

    void Start()
    {
        heroMove = FindObjectOfType<HeroMove>();

        if (heroMove != null)
        {
            maxHealth = heroMove.GetMaxHealth();
            UpdateHealthUI(heroMove.GetCurrentHealth(), maxHealth);
            heroMove.OnHealthChanged += UpdateHealthUI;
        }
    }

    void UpdateHealthUI(int currentHealth, int maxHealthValue)
    {
        maxHealth = maxHealthValue;
        float percent = (float)currentHealth / maxHealth;

        if (fillImage != null)
        {
            fillImage.fillAmount = percent;
        }

        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }

        // Меняем цвет
        if (percent > 0.5f)
            fillImage.color = healthyColor;
        else if (percent > 0.25f)
            fillImage.color = damagedColor;
        else
            fillImage.color = criticalColor;
    }
}