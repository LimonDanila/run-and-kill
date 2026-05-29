using UnityEngine;

public class PortalVisual : MonoBehaviour
{
    [Header("Visual Settings")]
    public float radius = 1.2f;
    public float thickness = 0.15f;
    public Color color = new Color(0.8f, 0.3f, 1f, 1f);
    public float pulseSpeed = 1.5f;
    public float pulseAmount = 0.15f;

    private SpriteRenderer spriteRenderer;
    private Texture2D circleTexture;
    private float time;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

        CreateCircleSprite();
    }

    void Update()
    {
        time += Time.deltaTime * pulseSpeed;

        // Пульсация
        float pulse = 1f + Mathf.Sin(time) * pulseAmount;
        float currentRadius = radius * pulse;

        // Пересоздаём спрайт с новым размером
        CreateCircleSprite(currentRadius);

        // Пульсация цвета
        float alpha = 0.7f + Mathf.Sin(time * 2f) * 0.2f;
        spriteRenderer.color = new Color(color.r, color.g, color.b, alpha);
    }

    void CreateCircleSprite(float currentRadius = -1)
    {
        if (currentRadius < 0) currentRadius = radius;

        int size = 256;
        circleTexture = new Texture2D(size, size);

        Vector2 center = new Vector2(size / 2, size / 2);
        float outerRadiusPx = currentRadius * (size / 3f);
        float innerRadiusPx = outerRadiusPx - thickness * (size / 3f);

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist >= innerRadiusPx && dist <= outerRadiusPx)
                {
                    // Градиент от центра
                    float gradient = 1f - (dist - innerRadiusPx) / (outerRadiusPx - innerRadiusPx);
                    circleTexture.SetPixel(x, y, new Color(color.r, color.g, color.b, gradient));
                }
                else
                {
                    circleTexture.SetPixel(x, y, Color.clear);
                }
            }
        }

        circleTexture.Apply();

        Sprite circleSprite = Sprite.Create(circleTexture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100);
        spriteRenderer.sprite = circleSprite;
    }
}