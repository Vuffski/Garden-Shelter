using UnityEngine;
using System.Collections;

public class TileView : MonoBehaviour
{
    public int X;
    public int Y;
    public SpriteRenderer plantIconRenderer;
    public float tileWorldSize = 1f;

    public SpriteRenderer fillRenderer;
    private Color baseFillColor;
    public bool IsOccupied { get; private set; }

    public GameObject growthBar;
    public SpriteRenderer growthBarFillRenderer;
    private float initialFillWidth;
    private PlantData currentPlant;
    private Coroutine growthCoroutine;
    private Coroutine glowCoroutine;
    public bool IsReadyToHarvest { get; private set; }

    private int clickCount = 0;
    private Coroutine resetCoroutine;
    private Coroutine shakeCoroutine;
    private Vector3 originalLocalPosition;

    private void Start()
    {
        originalLocalPosition = this.transform.localPosition;
        if (growthBarFillRenderer != null)
        {
            initialFillWidth = growthBarFillRenderer.transform.localScale.x;
        }
    }

    public string GetLabel()
    {
        char letter = (char)('A' + X);
        return $"{letter}{Y + 1}";
    }

    public void SetFillColor(Color color)
    {
        if (fillRenderer != null)
        {
            fillRenderer.color = color;
        }
        baseFillColor = color;
    }

    public void SetPlant(PlantData plant)
    {
        currentPlant = plant;
        plantIconRenderer.sprite = plant.Icon;
        plantIconRenderer.gameObject.SetActive(true);
        IsOccupied = true;
        IsReadyToHarvest = false;

        float targetSize = tileWorldSize * 0.95f;
        Vector3 spriteSize = plant.Icon.bounds.size;
        float scaleX = targetSize / spriteSize.x;
        float scaleY = targetSize / spriteSize.y;
        plantIconRenderer.transform.localScale = new Vector3(scaleX, scaleY, 1f);

        if (growthBar != null)
        {
            growthBar.SetActive(true);
        }

        if (growthBarFillRenderer != null)
        {
            Vector3 scale = growthBarFillRenderer.transform.localScale;
            scale.x = 0f;
            growthBarFillRenderer.transform.localScale = scale;

            Vector3 pos = growthBarFillRenderer.transform.localPosition;
            pos.x = -initialFillWidth / 2f;
            growthBarFillRenderer.transform.localPosition = pos;
        }

        if (growthCoroutine != null)
        {
            StopCoroutine(growthCoroutine);
        }
        growthCoroutine = StartCoroutine(GrowthRoutine(plant.GrowthTime));
    }

    private IEnumerator GrowthRoutine(float growthTime)
    {
        float elapsed = 0f;
        while (elapsed < growthTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / growthTime);
            if (growthBarFillRenderer != null)
            {
                Vector3 scale = growthBarFillRenderer.transform.localScale;
                scale.x = initialFillWidth * t;
                growthBarFillRenderer.transform.localScale = scale;

                Vector3 pos = growthBarFillRenderer.transform.localPosition;
                pos.x = (initialFillWidth * (t - 1f)) / 2f;
                growthBarFillRenderer.transform.localPosition = pos;
            }
            yield return null;
        }

        if (growthBar != null)
        {
            growthBar.SetActive(false);
        }
        IsReadyToHarvest = true;
        glowCoroutine = StartCoroutine(GlowRoutine());
    }

    private IEnumerator GlowRoutine()
    {
        while (IsReadyToHarvest)
        {
            float pulse = (Mathf.Sin(Time.time * 5f) + 1f) / 2f;
            if (fillRenderer != null)
            {
                fillRenderer.color = Color.Lerp(baseFillColor, Color.white, pulse);
            }
            yield return null;
        }
        if (fillRenderer != null)
        {
            fillRenderer.color = baseFillColor;
        }
    }

    public PlantData Harvest()
    {
        if (!IsReadyToHarvest)
        {
            return null;
        }

        PlantData harvestedPlant = currentPlant;
        IsReadyToHarvest = false;
        IsOccupied = false;

        if (glowCoroutine != null)
        {
            StopCoroutine(glowCoroutine);
            glowCoroutine = null;
        }

        if (fillRenderer != null)
        {
            fillRenderer.color = baseFillColor;
        }

        if (plantIconRenderer != null)
        {
            plantIconRenderer.gameObject.SetActive(false);
        }

        if (growthBar != null)
        {
            growthBar.SetActive(false);
        }

        currentPlant = null;
        return harvestedPlant;
    }

    public bool HandleOverwriteClick(PlantData newPlant)
    {
        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
        }

        clickCount++;

        if (clickCount == 1)
        {
            if (fillRenderer != null)
            {
                fillRenderer.color = new Color(baseFillColor.r * 0.5f, baseFillColor.g * 0.5f, baseFillColor.b * 0.5f, baseFillColor.a);
            }
        }
        else if (clickCount == 2)
        {
            if (shakeCoroutine == null)
            {
                shakeCoroutine = StartCoroutine(ShakeRoutine());
            }
        }
        else if (clickCount >= 3)
        {
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
                shakeCoroutine = null;
            }
            transform.localPosition = originalLocalPosition;
            if (fillRenderer != null)
            {
                fillRenderer.color = baseFillColor;
            }
            SetPlant(newPlant);
            clickCount = 0;
            return true;
        }

        resetCoroutine = StartCoroutine(ResetAfterDelay());
        return false;
    }

    private IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        clickCount = 0;
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }
        transform.localPosition = originalLocalPosition;
        if (fillRenderer != null)
        {
            fillRenderer.color = baseFillColor;
        }
    }

    private IEnumerator ShakeRoutine()
    {
        while (true)
        {
            float rx = Random.Range(-0.05f, 0.05f);
            float ry = Random.Range(-0.05f, 0.05f);
            transform.localPosition = originalLocalPosition + new Vector3(rx, ry, 0f);
            yield return new WaitForSeconds(0.03f);
        }
    }
}