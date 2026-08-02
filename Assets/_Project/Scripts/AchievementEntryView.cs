using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementEntryView : MonoBehaviour
{
    public AchievementData achievementData;
    public TMP_Text titleLabel;
    public TMP_Text descriptionLabel;
    public Slider progressBar;
    public AchievementManager manager;

    private Color normalColor;
    private Color normalDescColor;

    private void Start()
    {
        if (titleLabel != null)
        {
            normalColor = titleLabel.color;
        }

        if (descriptionLabel != null)
        {
            if (achievementData != null && !string.IsNullOrWhiteSpace(achievementData.UnlockDescription))
            {
                descriptionLabel.text = achievementData.UnlockDescription;
                normalDescColor = descriptionLabel.color;
            }
            else
            {
                descriptionLabel.gameObject.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (achievementData == null || manager == null) return;

        int progress = manager.GetProgress(achievementData);
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