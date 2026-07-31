using UnityEngine;

public class AoeOverlayFlash : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Color baseColor;
    private float frequency;

    public void Initialize(Color color, float frequency)
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseColor = color;
        this.frequency = Mathf.Max(0.01f, frequency);
    }

    private void Update()
    {
        if (spriteRenderer == null) return;

        float fadeInDuration = 0.2f;
        float cycleLength = fadeInDuration + frequency;
        float t = Time.time % cycleLength;
        float alpha;

        if (t < fadeInDuration)
        {
            alpha = Mathf.Lerp(0.2f, 1f, t / fadeInDuration);
        }
        else
        {
            alpha = Mathf.Lerp(1f, 0.2f, (t - fadeInDuration) / frequency);
        }

        Color color = baseColor;
        color.a = alpha;
        spriteRenderer.color = color;
    }
}
