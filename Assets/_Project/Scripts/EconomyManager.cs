using UnityEngine;
using TMPro;

public class EconomyManager : MonoBehaviour
{
    public int startingGold = 100;
    private int currentGold;
    public TMP_Text goldLabel;
    public AchievementManager achievementManager;

    private Color originalGoldColor;
    private bool isGoldColorCached;

    public int MaxGold
    {
        get
        {
            int max = 10;
            if (achievementManager != null)
            {
                foreach (var ach in achievementManager.AllAchievements)
                {
                    if (achievementManager.IsCompleted(ach))
                    {
                        max += ach.MaxMoneyIncrease;
                    }
                }
            }
            return max;
        }
    }

    private void Awake()
    {
        currentGold = Mathf.Min(startingGold, MaxGold);
    }

    private void Start()
    {
        if (achievementManager != null)
        {
            achievementManager.OnAchievementCompleted += HandleAchievementCompleted;
        }
        UpdateLabel();
    }

    private void OnDestroy()
    {
        if (achievementManager != null)
        {
            achievementManager.OnAchievementCompleted -= HandleAchievementCompleted;
        }
    }

    private void HandleAchievementCompleted(AchievementData achievement)
    {
        UpdateLabel();
    }

    public bool CanAfford(int cost)
    {
        return currentGold >= cost;
    }

    public void Spend(int cost)
    {
        currentGold -= cost;
        if (currentGold < 0) currentGold = 0;
        UpdateLabel();
    }

    public void Earn(int amount)
    {
        if (amount <= 0) return;
        currentGold += amount;
        int max = MaxGold;
        if (currentGold > max)
        {
            currentGold = max;
        }
        if (achievementManager != null)
        {
            achievementManager.RegisterCoinsEarned(amount);
        }
        UpdateLabel();
    }

    private void UpdateLabel()
    {
        if (goldLabel != null)
        {
            if (!isGoldColorCached)
            {
                originalGoldColor = goldLabel.color;
                isGoldColorCached = true;
            }

            int max = MaxGold;
            goldLabel.text = $"${currentGold}<size=50%><color=#aaaaaa>/{max}</color></size>";

            if (currentGold >= max)
            {
                goldLabel.fontStyle = FontStyles.Bold;
                goldLabel.color = Color.red;
            }
            else
            {
                goldLabel.fontStyle = FontStyles.Normal;
                goldLabel.color = originalGoldColor;
            }
        }
    }
}
