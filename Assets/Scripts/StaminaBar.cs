using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StaminaBar : MonoBehaviour
{
    [Header("UI References")]
    public Image fillImage;           // Сюда перетащите StaminaFill (Image)
    public TextMeshProUGUI staminaText; // Сюда перетащите StaminaText

    [Header("Colors")]
    public Color fullStaminaColor = new Color(0.3f, 0.8f, 1f);   // Голубой
    public Color mediumStaminaColor = new Color(1f, 0.8f, 0.2f);  // Желтый
    public Color lowStaminaColor = new Color(1f, 0.3f, 0.2f);     // Красный

    private HeroMove heroMove;
    private int maxStamina;

    void Start()
    {
        heroMove = FindObjectOfType<HeroMove>();

        if (heroMove != null)
        {
            maxStamina = heroMove.GetMaxStamina();
            UpdateStaminaUI(heroMove.GetCurrentStamina(), maxStamina);

            // Подписываемся на событие изменения стамины
            heroMove.OnStaminaChanged += UpdateStaminaUI;
        }
        else
        {
            Debug.LogError("HeroMove не найден!");
        }
    }

    void UpdateStaminaUI(int currentStamina, int maxStaminaValue)
    {
        maxStamina = maxStaminaValue;
        float percent = (float)currentStamina / maxStamina;

        // Обновляем полоску
        if (fillImage != null)
        {
            fillImage.fillAmount = percent;
        }

        // Обновляем текст
        if (staminaText != null)
        {
            staminaText.text = $"{currentStamina}/{maxStamina}";
        }

        // Меняем цвет в зависимости от количества стамины
        UpdateColor(percent);
    }

    void UpdateColor(float percent)
    {
        if (fillImage == null) return;

        if (percent > 0.5f)
        {
            fillImage.color = fullStaminaColor;
        }
        else if (percent > 0.25f)
        {
            fillImage.color = mediumStaminaColor;
        }
        else
        {
            fillImage.color = lowStaminaColor;
        }
    }
}