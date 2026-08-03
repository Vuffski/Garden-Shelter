using UnityEngine;
using TMPro;

public class MiniAchievementEntryView : MonoBehaviour
{
    public TMPro.TMP_Text titleLabel;
    public UnityEngine.UI.Slider progressSlider;
    [SerializeField] private TMPro.TMP_Text descriptionLabel;
    [SerializeField] private UnityEngine.UI.Image doggyIconImage;

    [SerializeField] private float flashBrightenMultiplier = 1.4f;
    [SerializeField] private float flashDarkenMultiplier = 0.85f;

    private UnityEngine.UI.Button entryButton;
    private UnityEngine.UI.Image fillImage;
    private Color baseFillColor;
    private bool hasStoredBaseColor = false;
    private Coroutine flashCoroutine;

    private Color normalColor;
    private Color normalDescColor;
    private AchievementData achievementData;

    private void Awake()
    {
        entryButton = GetComponent<UnityEngine.UI.Button>();
        if (entryButton == null)
        {
            entryButton = gameObject.AddComponent<UnityEngine.UI.Button>();
        }

        entryButton.onClick.AddListener(() =>
        {
            if (achievementData != null && AchievementManager.Instance != null)
            {
                if (AchievementManager.Instance.IsReadyToCollect(achievementData))
                {
                    AchievementManager.Instance.ClaimAchievement(achievementData);
                }
            }
        });

        if (progressSlider != null && progressSlider.fillRect != null)
        {
            fillImage = progressSlider.fillRect.GetComponent<UnityEngine.UI.Image>();
            if (fillImage != null)
            {
                baseFillColor = fillImage.color;
                hasStoredBaseColor = true;
            }
        }

        if (titleLabel != null)
        {
            normalColor = titleLabel.color;
        }

        if (descriptionLabel != null)
        {
            normalDescColor = descriptionLabel.color;
        }
    }

    private void OnEnable()
    {
        if (achievementData != null && AchievementManager.Instance != null && AchievementManager.Instance.IsReadyToCollect(achievementData))
        {
            StartFlashRoutine();
        }
    }

    private void OnDisable()
    {
        StopFlashRoutine();
    }

    private void OnDestroy()
    {
        StopFlashRoutine();
    }

    public void SetData(AchievementData achievement, float progress)
    {
        gameObject.SetActive(true);
        achievementData = achievement;
        if (achievement != null)
        {
            if (!hasStoredBaseColor && progressSlider != null && progressSlider.fillRect != null)
            {
                fillImage = progressSlider.fillRect.GetComponent<UnityEngine.UI.Image>();
                if (fillImage != null)
                {
                    baseFillColor = fillImage.color;
                    hasStoredBaseColor = true;
                }
            }

            bool isReady = AchievementManager.Instance != null && AchievementManager.Instance.IsReadyToCollect(achievement);

            if (titleLabel != null)
            {
                titleLabel.text = achievement.Title;
                titleLabel.fontStyle = isReady ? TMPro.FontStyles.Bold : TMPro.FontStyles.Normal;
                if (normalColor != default)
                {
                    titleLabel.color = normalColor;
                }
            }

            if (progressSlider != null)
            {
                progressSlider.value = isReady ? 1f : progress;
            }

            if (descriptionLabel != null)
            {
                if (string.IsNullOrWhiteSpace(achievement.UnlockDescription))
                {
                    descriptionLabel.gameObject.SetActive(false);
                }
                else
                {
                    descriptionLabel.gameObject.SetActive(true);
                    descriptionLabel.text = achievement.UnlockDescription;
                    descriptionLabel.fontStyle = TMPro.FontStyles.Normal;
                    if (normalDescColor != default)
                    {
                        descriptionLabel.color = normalDescColor;
                    }
                }
            }

            if (doggyIconImage != null)
            {
                doggyIconImage.gameObject.SetActive(achievement.RequiredDoggy != null);
                if (achievement.RequiredDoggy != null)
                {
                    doggyIconImage.sprite = achievement.RequiredDoggy.Icon;
                }
            }

            if (entryButton != null)
            {
                entryButton.interactable = isReady;
            }

            if (isReady)
            {
                StartFlashRoutine();
            }
            else
            {
                StopFlashRoutine();
                if (fillImage != null && hasStoredBaseColor)
                {
                    fillImage.color = baseFillColor;
                }
            }
        }
    }

    private void StartFlashRoutine()
    {
        if (gameObject.activeInHierarchy && flashCoroutine == null)
        {
            flashCoroutine = StartCoroutine(FlashRoutine());
        }
    }

    private void StopFlashRoutine()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }
    }

    private System.Collections.IEnumerator FlashRoutine()
    {
        if (fillImage == null || !hasStoredBaseColor) yield break;

        Color brightColor = GetHSVMultipliedColor(baseFillColor, flashBrightenMultiplier);
        Color darkColor = GetHSVMultipliedColor(baseFillColor, flashDarkenMultiplier);

        while (true)
        {
            float elapsed = 0f;
            float duration = 0.2f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                fillImage.color = Color.Lerp(baseFillColor, brightColor, t);
                yield return null;
            }
            fillImage.color = brightColor;

            elapsed = 0f;
            duration = 1.0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                fillImage.color = Color.Lerp(brightColor, darkColor, t);
                yield return null;
            }
            fillImage.color = darkColor;
        }
    }

    private Color GetHSVMultipliedColor(Color baseColor, float multiplier)
    {
        float h, s, v;
        Color.RGBToHSV(baseColor, out h, out s, out v);
        v = Mathf.Clamp01(v * multiplier);
        Color result = Color.HSVToRGB(h, s, v);
        result.a = baseColor.a;
        return result;
    }

    public void Clear()
    {
        achievementData = null;
        StopFlashRoutine();
        if (fillImage != null && hasStoredBaseColor)
        {
            fillImage.color = baseFillColor;
        }
        if (descriptionLabel != null)
        {
            descriptionLabel.gameObject.SetActive(false);
        }
        if (doggyIconImage != null)
        {
            doggyIconImage.gameObject.SetActive(false);
        }
        gameObject.SetActive(false);
    }
}
