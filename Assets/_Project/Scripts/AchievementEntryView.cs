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

    private Color normalColor;
    private Color normalDescColor;

    private void Awake()
    {
        if (achievementManager != null)
        {
            achievementManager.OnAchievementCompleted += HandleAchievementCompleted;
        }
        if (unlockManager != null)
        {
            unlockManager.OnDoggyUnlocked += HandleDoggyUnlocked;
        }
    }

    private void OnDestroy()
    {
        if (achievementManager != null)
        {
            achievementManager.OnAchievementCompleted -= HandleAchievementCompleted;
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
    }

    private void HandleDoggyUnlocked(DoggyData doggy)
    {
        if (achievementManager == null || achievementData == null) return;
        if (!gameObject.activeSelf && achievementManager.IsUnlocked(achievementData))
        {
            Initialize();
        }
    }

    private void Start()
    {
        Initialize();
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

    private void Update()
    {
        if (achievementData == null || achievementManager == null) return;

        int progress = achievementManager.GetProgress(achievementData);
        int target = achievementData.TargetValue;

        if (progress >= target)
        {
            if (progressBar != null && progressBar.gameObject.activeSelf)
            {
                progressBar.gameObject.SetActive(false);
            }
            if (titleLabel != null)
            {
                titleLabel.text = "<s>" + achievementData.Title + "</s>";
                titleLabel.color = Color.gray;
            }
            if (descriptionLabel != null && descriptionLabel.gameObject.activeSelf)
            {
                descriptionLabel.text = "<s>" + achievementData.UnlockDescription + "</s>";
                descriptionLabel.color = Color.gray;
            }
        }
        else
        {
            if (progressBar != null)
            {
                if (!progressBar.gameObject.activeSelf)
                {
                    progressBar.gameObject.SetActive(true);
                }
                progressBar.value = (float)progress / target;
            }
            if (titleLabel != null)
            {
                titleLabel.text = achievementData.Title;
                titleLabel.color = normalColor;
            }
            if (descriptionLabel != null && descriptionLabel.gameObject.activeSelf)
            {
                descriptionLabel.text = achievementData.UnlockDescription;
                descriptionLabel.color = normalDescColor;
            }
        }
    }
}