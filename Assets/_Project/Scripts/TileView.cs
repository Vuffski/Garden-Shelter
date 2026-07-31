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

    private DoggyData currentDoggy;
    private Coroutine doggyCoroutine;

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

    private void ApplyIconScale(Sprite icon)
    {
        if (icon == null) return;
        float targetSize = tileWorldSize * 0.95f;
        Vector3 spriteSize = icon.bounds.size;
        float scaleX = targetSize / spriteSize.x;
        float scaleY = targetSize / spriteSize.y;
        if (plantIconRenderer != null)
        {
            plantIconRenderer.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }
    }

    private void UpdateGrowthBar(float t)
    {
        if (growthBarFillRenderer != null)
        {
            Vector3 scale = growthBarFillRenderer.transform.localScale;
            scale.x = initialFillWidth * t;
            growthBarFillRenderer.transform.localScale = scale;

            Vector3 pos = growthBarFillRenderer.transform.localPosition;
            pos.x = (initialFillWidth * (t - 1f)) / 2f;
            growthBarFillRenderer.transform.localPosition = pos;
        }
    }

    public void SetPlant(PlantData plant)
    {
        if (growthBarFillRenderer != null)
        {
            growthBarFillRenderer.color = Color.green;
        }

        currentPlant = plant;
        if (plantIconRenderer != null)
        {
            plantIconRenderer.sprite = plant.Icon;
            plantIconRenderer.gameObject.SetActive(true);
        }
        IsOccupied = true;
        IsReadyToHarvest = false;

        ApplyIconScale(plant.Icon);

        if (growthBar != null)
        {
            growthBar.SetActive(true);
        }

        UpdateGrowthBar(0f);

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
            UpdateGrowthBar(t);
            yield return null;
        }

        if (growthBar != null)
        {
            growthBar.SetActive(false);
        }
        IsReadyToHarvest = true;
        glowCoroutine = StartCoroutine(GlowRoutine());
    }

    public void PlaceDoggy(DoggyData doggy)
    {
        currentDoggy = doggy;
        if (plantIconRenderer != null)
        {
            plantIconRenderer.sprite = doggy.Icon;
            plantIconRenderer.gameObject.SetActive(true);
        }
        ApplyIconScale(doggy.Icon);
        IsOccupied = true;

        if (growthBar != null)
        {
            growthBar.SetActive(true);
        }

        if (growthBarFillRenderer != null)
        {
            growthBarFillRenderer.color = new Color(1f, 0.55f, 0f);
        }

        UpdateGrowthBar(1f);

        if (doggyCoroutine != null)
        {
            StopCoroutine(doggyCoroutine);
        }
        doggyCoroutine = StartCoroutine(DoggyCountdownRoutine(doggy.Duration));
    }

    private IEnumerator DoggyCountdownRoutine(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            yield return null;
            elapsed += Time.deltaTime;
            float t = 1f - Mathf.Clamp01(elapsed / duration);
            UpdateGrowthBar(t);
        }
        UpdateGrowthBar(0f);
        RemoveDoggy();
    }

    private void RemoveDoggy()
    {
        if (plantIconRenderer != null)
        {
            plantIconRenderer.gameObject.SetActive(false);
        }
        if (growthBar != null)
        {
            growthBar.SetActive(false);
        }
        IsOccupied = false;
        currentDoggy = null;
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