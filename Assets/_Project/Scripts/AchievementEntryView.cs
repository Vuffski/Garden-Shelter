using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementEntryView : MonoBehaviour
{
    public AchievementData achievementData;
    public TMP_Text titleLabel;
    public TMP_Text descriptionLabel;
    public UnityEngine.UI.Slider progressBar;

    [UnityEngine.Serialization.FormerlySerializedAs("manager")]
    public AchievementManager achievementManager;
    public UnlockManager unlockManager;

    [SerializeField] private UnityEngine.UI.Image doggyIconImage;
    [SerializeField] private float flashBrightenMultiplier = 1.4f;
    [SerializeField] private float flashDarkenMultiplier = 0.85f;

    private Color normalColor;
    private Color normalDescColor;

    private UnityEngine.UI.Button entryButton;
    private UnityEngine.UI.Image fillImage;
    private Color baseFillColor;
    private bool hasStoredBaseColor = false;
    private Coroutine flashCoroutine;

    private enum AchievementVisualState
    {
        InProgress,
        ReadyToCollect,
        Completed
    }
    private AchievementVisualState currentVisualState;

    private void Awake()
    {
        entryButton = GetComponent<UnityEngine.UI.Button>();
        if (entryButton == null)
        {
            entryButton = gameObject.AddComponent<UnityEngine.UI.Button>();
        }

        entryButton.onClick.AddListener(() =>
        {
            if (achievementData != null && achievementManager != null)
            {
                if (achievementManager.IsReadyToCollect(achievementData))
                {
                    achievementManager.ClaimAchievement(achievementData);
                }
            }
        });

        if (progressBar != null && progressBar.fillRect != null)
        {
            fillImage = progressBar.fillRect.GetComponent<UnityEngine.UI.Image>();
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

        if (achievementManager != null)
        {
            achievementManager.OnAchievementCompleted += HandleAchievementCompleted;
            achievementManager.OnAchievementReadyToCollect += HandleAchievementReadyToCollect;
        }
        if (unlockManager != null)
        {
            unlockManager.OnDoggyUnlocked += HandleDoggyUnlocked;
        }
    }

    private void OnEnable()
    {
        if (hasStoredBaseColor)
        {
            RefreshVisualState();
        }
        if (currentVisualState == AchievementVisualState.ReadyToCollect && flashCoroutine == null)
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
        if (achievementManager != null)
        {
            achievementManager.OnAchievementCompleted -= HandleAchievementCompleted;
            achievementManager.OnAchievementReadyToCollect -= HandleAchievementReadyToCollect;
        }
        if (unlockManager != null)
        {
            unlockManager.OnDoggyUnlocked -= HandleDoggyUnlocked;
        }
    }

    private void HandleAchievementCompleted(AchievementData achievement)
    {
        if (achievementManager == null || achievementData == null) return;
        if (!gameObject.activeSelf && achievementManager.IsUnlocked(achievementData))
        {
            Initialize();
        }
        if (achievement == achievementData)
        {
            RefreshVisualState();
        }
    }

    private void HandleAchievementReadyToCollect(AchievementData achievement)
    {
        if (achievementManager == null || achievementData == null) return;
        if (!gameObject.activeSelf && achievementManager.IsUnlocked(achievementData))
        {
            Initialize();
        }
        if (achievement == achievementData)
        {
            RefreshVisualState();
        }
    }

    private void HandleDoggyUnlocked(DoggyData doggy)
    {
        if (achievementManager == null || achievementData == null) return;
        if (!gameObject.activeSelf && achievementManager.IsUnlocked(achievementData))
        {
            Initialize();
            RefreshVisualState();
        }
    }

    private void Start()
    {
        Initialize();
        RefreshVisualState();
    }

    public void Initialize()
    {
        if (achievementManager == null || achievementData == null) return;

        if (!achievementManager.IsUnlocked(achievementData))
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (titleLabel != null)
        {
            normalColor = titleLabel.color;
        }

        if (descriptionLabel != null)
        {
            if (!string.IsNullOrWhiteSpace(achievementData.UnlockDescription))
            {
                descriptionLabel.text = achievementData.UnlockDescription;
                normalDescColor = descriptionLabel.color;
            }
            else
            {
                descriptionLabel.gameObject.SetActive(false);
            }
        }

        if (doggyIconImage != null)
        {
            doggyIconImage.gameObject.SetActive(achievementData.RequiredDoggy != null);
            if (achievementData.RequiredDoggy != null)
            {
                doggyIconImage.sprite = achievementData.RequiredDoggy.Icon;
            }
        }
    }

    public void RefreshVisualState()
    {
        if (achievementData == null || achievementManager == null) return;

        if (achievementManager.IsCompleted(achievementData))
        {
            currentVisualState = AchievementVisualState.Completed;
            StopFlashRoutine();
            if (fillImage != null && hasStoredBaseColor)
            {
                fillImage.color = baseFillColor;
            }
            if (entryButton != null)
            {
                entryButton.interactable = false;
            }

            if (progressBar != null && progressBar.gameObject.activeSelf)
            {
                progressBar.gameObject.SetActive(false);
            }
            if (titleLabel != null)
            {
                titleLabel.text = "<s>" + achievementData.Title + "</s>";
                titleLabel.color = Color.gray;
                titleLabel.fontStyle = TMPro.FontStyles.Normal;
            }
            if (descriptionLabel != null && descriptionLabel.gameObject.activeSelf)
            {
                descriptionLabel.text = "<s>" + achievementData.UnlockDescription + "</s>";
                descriptionLabel.color = Color.gray;
                descriptionLabel.fontStyle = TMPro.FontStyles.Normal;
            }
        }
        else if (achievementManager.IsReadyToCollect(achievementData))
        {
            currentVisualState = AchievementVisualState.ReadyToCollect;
            if (titleLabel != null)
            {
                titleLabel.text = achievementData.Title;
                titleLabel.color = normalColor;
                titleLabel.fontStyle = TMPro.FontStyles.Bold;
            }
            if (descriptionLabel != null && descriptionLabel.gameObject.activeSelf)
            {
                descriptionLabel.text = achievementData.UnlockDescription;
                descriptionLabel.color = normalDescColor;
                descriptionLabel.fontStyle = TMPro.FontStyles.Normal;
            }
            if (progressBar != null)
            {
                if (!progressBar.gameObject.activeSelf)
                {
                    progressBar.gameObject.SetActive(true);
                }
                progressBar.value = 1f;
            }

            if (entryButton != null)
            {
                entryButton.interactable = true;
            }

            StartFlashRoutine();
        }
        else
        {
            currentVisualState = AchievementVisualState.InProgress;
            StopFlashRoutine();
            if (fillImage != null && hasStoredBaseColor)
            {
                fillImage.color = baseFillColor;
            }
            if (entryButton != null)
            {
                entryButton.interactable = false;
            }

            if (titleLabel != null)
            {
                titleLabel.text = achievementData.Title;
                titleLabel.color = normalColor;
                titleLabel.fontStyle = TMPro.FontStyles.Normal;
            }
            if (descriptionLabel != null && descriptionLabel.gameObject.activeSelf)
            {
                descriptionLabel.text = achievementData.UnlockDescription;
                descriptionLabel.color = normalDescColor;
                descriptionLabel.fontStyle = TMPro.FontStyles.Normal;
            }
            if (progressBar != null)
            {
                if (!progressBar.gameObject.activeSelf)
                {
                    progressBar.gameObject.SetActive(true);
                }
                int progress = achievementManager.GetProgress(achievementData);
                int target = achievementData.TargetValue;
                progressBar.value = target > 0 ? (float)progress / target : 0f;
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

    private void Update()
    {
        if (achievementData == null || achievementManager == null) return;

        if (achievementManager.IsCompleted(achievementData))
        {
            if (progressBar != null && progressBar.gameObject.activeSelf)
            {
                progressBar.gameObject.SetActive(false);
            }
            if (titleLabel != null)
            {
                titleLabel.text = "<s>" + achievementData.Title + "</s>";
                titleLabel.color = Color.gray;
                titleLabel.fontStyle = TMPro.FontStyles.Normal;
            }
            if (descriptionLabel != null && descriptionLabel.gameObject.activeSelf)
            {
                descriptionLabel.text = "<s>" + achievementData.UnlockDescription + "</s>";
                descriptionLabel.color = Color.gray;
                descriptionLabel.fontStyle = TMPro.FontStyles.Normal;
            }
        }
        else if (achievementManager.IsReadyToCollect(achievementData))
        {
            if (progressBar != null)
            {
                if (!progressBar.gameObject.activeSelf)
                {
                    progressBar.gameObject.SetActive(true);
                }
                progressBar.value = 1f;
            }
        }
        else
        {
            int progress = achievementManager.GetProgress(achievementData);
            int target = achievementData.TargetValue;

            if (progressBar != null)
            {
                if (!progressBar.gameObject.activeSelf)
                {
                    progressBar.gameObject.SetActive(true);
                }
                progressBar.value = target > 0 ? (float)progress / target : 0f;
            }
            if (titleLabel != null)
            {
                titleLabel.text = achievementData.Title;
                titleLabel.color = normalColor;
                titleLabel.fontStyle = TMPro.FontStyles.Normal;
            }
            if (descriptionLabel != null && descriptionLabel.gameObject.activeSelf)
            {
                descriptionLabel.text = achievementData.UnlockDescription;
                descriptionLabel.color = normalDescColor;
                descriptionLabel.fontStyle = TMPro.FontStyles.Normal;
            }
        }
    }
}