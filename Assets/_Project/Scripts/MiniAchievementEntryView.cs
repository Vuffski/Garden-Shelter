using UnityEngine;
using TMPro;

public class MiniAchievementEntryView : MonoBehaviour
{
    public TMPro.TMP_Text titleLabel;
    public UnityEngine.UI.Slider progressSlider;
    [SerializeField] private TMPro.TMP_Text descriptionLabel;

    public void SetData(AchievementData achievement, float progress)
    {
        gameObject.SetActive(true);
        if (achievement != null)
        {
            if (titleLabel != null)
            {
                titleLabel.text = achievement.Title;
            }
            if (progressSlider != null)
            {
                progressSlider.value = progress;
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
                }
            }
        }
    }

    public void Clear()
    {
        if (descriptionLabel != null)
        {
            descriptionLabel.gameObject.SetActive(false);
        }
        gameObject.SetActive(false);
    }
}
