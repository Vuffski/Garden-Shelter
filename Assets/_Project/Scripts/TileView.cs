using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct HarvestResult
{
    public PlantData Plant;
    public int Amount;
}

public class TileView : MonoBehaviour
{
    public int X;
    public int Y;
    public SpriteRenderer plantIconRenderer;
    public float tileWorldSize = 1f;

    public SpriteRenderer fillRenderer;
    private Color baseFillColor;
    public bool IsOccupied { get; private set; }
    public bool IsOccupiedByDoggy => currentDoggy != null;

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

    [HideInInspector] public DoggyFieldManager doggyFieldManager;
    [HideInInspector] public GridManager gridManager;
    public GameObject aoeOverlayPrefab;
    private Dictionary<DoggyData, SpriteRenderer> aoeOverlays = new Dictionary<DoggyData, SpriteRenderer>();
    private List<TileView> influencedTiles = new List<TileView>();

    private DoggyData currentDoggy;
    private Coroutine doggyCoroutine;
    private Coroutine duplicateFlashCoroutine;

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

    public void AddAoeOverlay(DoggyData doggy)
    {
        if (doggy == null) return;
        if (aoeOverlays.ContainsKey(doggy)) return;

        if (aoeOverlayPrefab != null)
        {
            GameObject spawnedOverlay = Instantiate(aoeOverlayPrefab, transform);
            spawnedOverlay.transform.localPosition = Vector3.zero;
            
            // Set layer to match the tile's layer (Default) to prevent Naninovel's UI camera from rendering it
            spawnedOverlay.layer = gameObject.layer;

            SpriteRenderer sr = spawnedOverlay.GetComponent<SpriteRenderer>();
            
            AoeOverlayFlash flasher = spawnedOverlay.GetComponent<AoeOverlayFlash>();
            if (flasher != null)
            {
                flasher.Initialize(doggy.AoeColor, doggy.FlashFrequency);
            }
            else if (sr != null)
            {
                sr.color = doggy.AoeColor;
            }

            aoeOverlays[doggy] = sr;
        }
    }

    public void RemoveAoeOverlay(DoggyData doggy)
    {
        if (doggy == null) return;
        if (aoeOverlays.TryGetValue(doggy, out SpriteRenderer sr))
        {
            if (sr != null && sr.gameObject != null)
            {
                Destroy(sr.gameObject);
            }
            aoeOverlays.Remove(doggy);
        }
    }

    private void ApplyInfluence(DoggyData doggy)
    {
        if (doggy == null) return;
        influencedTiles.Clear();

        int rUp = doggy.RangeUp;
        int rDown = doggy.RangeDown;
        int rLeft = doggy.RangeLeft;
        int rRight = doggy.RangeRight;

        if (AchievementManager.Instance != null)
        {
            DoggyEffectiveStats stats = DoggyEffectiveStats.Resolve(doggy, AchievementManager.Instance);
            if (stats != null)
            {
                rUp = stats.rangeUp;
                rDown = stats.rangeDown;
                rLeft = stats.rangeLeft;
                rRight = stats.rangeRight;
            }
        }

        ApplyDirection(doggy, 0, 1, rUp);
        ApplyDirection(doggy, 1, 1, doggy.RangeUpRight);
        ApplyDirection(doggy, 1, 0, rRight);
        ApplyDirection(doggy, 1, -1, doggy.RangeDownRight);
        ApplyDirection(doggy, 0, -1, rDown);
        ApplyDirection(doggy, -1, -1, doggy.RangeDownLeft);
        ApplyDirection(doggy, -1, 0, rLeft);
        ApplyDirection(doggy, -1, 1, doggy.RangeUpLeft);
    }

    private void ApplyDirection(DoggyData doggy, int dx, int dy, int range)
    {
        if (gridManager == null) return;
        for (int step = 1; step <= range; step++)
        {
            TileView targetTile = gridManager.GetTile(X + dx * step, Y + dy * step);
            if (targetTile == null) continue;

            targetTile.AddAoeOverlay(doggy);
            influencedTiles.Add(targetTile);
        }
    }

    private void ClearInfluence(DoggyData doggy)
    {
        if (doggy == null) return;
        foreach (TileView tile in influencedTiles)
        {
            if (tile != null)
            {
                tile.RemoveAoeOverlay(doggy);
            }
        }
        influencedTiles.Clear();
    }

    private void ClearCurrentOccupant()
    {
        if (growthCoroutine != null)
        {
            StopCoroutine(growthCoroutine);
            growthCoroutine = null;
        }
        if (doggyCoroutine != null)
        {
            StopCoroutine(doggyCoroutine);
            doggyCoroutine = null;
        }
        if (glowCoroutine != null)
        {
            StopCoroutine(glowCoroutine);
            glowCoroutine = null;
        }

        if (currentDoggy != null)
        {
            ClearInfluence(currentDoggy);
            if (doggyFieldManager != null)
            {
                doggyFieldManager.UnregisterDoggy(currentDoggy);
            }
        }

        currentDoggy = null;
        currentPlant = null;
        IsReadyToHarvest = false;
    }

    public void SetPlant(PlantData plant)
    {
        ClearCurrentOccupant();

        if (growthBarFillRenderer != null)
        {
            growthBarFillRenderer.color = Color.green;
        }

        currentPlant = plant;
        if (plantIconRenderer != null)
        {
            plantIconRenderer.sprite = plant.Icon;
            plantIconRenderer.color = plant.IconColor;
            plantIconRenderer.gameObject.SetActive(true);
        }
        IsOccupied = true;

        ApplyIconScale(plant.Icon);

        if (growthBar != null)
        {
            growthBar.SetActive(true);
        }

        UpdateGrowthBar(0f);

        growthCoroutine = StartCoroutine(GrowthRoutine(plant.GrowthTime));
    }

    private float GetActiveGrowthMultiplier()
    {
        float multiplier = 1f;
        foreach (DoggyData doggy in aoeOverlays.Keys)
        {
            if (doggy != null && doggy.UseGrowthSpeed)
            {
                float mod = doggy.GrowthMultiplier;
                if (AchievementManager.Instance != null)
                {
                    DoggyEffectiveStats stats = DoggyEffectiveStats.Resolve(doggy, AchievementManager.Instance);
                    if (stats != null)
                    {
                        mod = stats.growthSpeedModifier;
                    }
                }
                multiplier *= mod;
            }
        }
        return multiplier;
    }

    private IEnumerator GrowthRoutine(float growthTime)
    {
        float progress = 0f;
        while (progress < growthTime)
        {
            float multiplier = GetActiveGrowthMultiplier();
            progress += Time.deltaTime / Mathf.Max(multiplier, 0.0001f);
            float t = Mathf.Clamp01(progress / growthTime);
            UpdateGrowthBar(t);
            yield return null;
        }

        if (growthBar != null)
        {
            growthBar.SetActive(false);
        }
        IsReadyToHarvest = true;

        bool autoHarvested = false;
        if (AchievementManager.Instance != null)
        {
            foreach (DoggyData doggy in aoeOverlays.Keys)
            {
                if (doggy != null)
                {
                    DoggyEffectiveStats stats = DoggyEffectiveStats.Resolve(doggy, AchievementManager.Instance);
                    if (stats != null && stats.autoHarvestEnabled)
                    {
                        autoHarvested = true;
                        break;
                    }
                }
            }
        }

        if (autoHarvested)
        {
            HarvestResult result = Harvest();
            if (result.Plant != null)
            {
                if (HarvestInventory.Instance != null)
                {
                    HarvestInventory.Instance.AddHarvest(result.Plant, result.Amount);
                }
                if (AchievementManager.Instance != null)
                {
                    AchievementManager.Instance.RegisterHarvest(result.Plant, result.Amount);
                }
            }
        }
        else
        {
            glowCoroutine = StartCoroutine(GlowRoutine());
        }
    }

    public void PlaceDoggy(DoggyData doggy)
    {
        ClearCurrentOccupant();

        currentDoggy = doggy;
        if (doggyFieldManager != null && currentDoggy != null)
        {
            doggyFieldManager.RegisterDoggy(currentDoggy, this);
        }

        if (plantIconRenderer != null)
        {
            plantIconRenderer.sprite = doggy.Icon;
            plantIconRenderer.color = doggy.IconColor;
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

        doggyCoroutine = StartCoroutine(DoggyCountdownRoutine(doggy.Duration));
        ApplyInfluence(doggy);
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
        if (currentDoggy != null)
        {
            ClearInfluence(currentDoggy);
        }
        if (doggyFieldManager != null && currentDoggy != null)
        {
            doggyFieldManager.UnregisterDoggy(currentDoggy);
        }
        currentDoggy = null;
    }

    public void FlashDuplicateWarning()
    {
        if (duplicateFlashCoroutine != null)
        {
            StopCoroutine(duplicateFlashCoroutine);
        }
        duplicateFlashCoroutine = StartCoroutine(DuplicateFlashRoutine());
    }

    private IEnumerator DuplicateFlashRoutine()
    {
        if (fillRenderer != null)
        {
            fillRenderer.color = Color.red;
        }
        yield return new WaitForSeconds(0.3f);
        if (fillRenderer != null)
        {
            fillRenderer.color = baseFillColor;
        }
        duplicateFlashCoroutine = null;
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

    public HarvestResult Harvest()
    {
        if (!IsReadyToHarvest)
        {
            return new HarvestResult { Plant = null, Amount = 0 };
        }

        float totalBonus = 0f;
        foreach (DoggyData doggy in aoeOverlays.Keys)
        {
            if (doggy != null && doggy.UseYield)
            {
                totalBonus += doggy.YieldChance;
            }
        }

        int guaranteedExtra = Mathf.FloorToInt(totalBonus);
        float remainder = totalBonus - guaranteedExtra;
        int amount = 1 + guaranteedExtra;
        if (Random.value < remainder)
        {
            amount += 1;
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
        return new HarvestResult { Plant = harvestedPlant, Amount = amount };
    }

    public bool HandleOverwriteClick(System.Action onConfirm)
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
            onConfirm?.Invoke();
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