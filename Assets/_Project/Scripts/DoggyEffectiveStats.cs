using UnityEngine;

public class DoggyEffectiveStats
{
    public int rangeUp;
    public int rangeDown;
    public int rangeLeft;
    public int rangeRight;
    public float growthSpeedModifier;
    public bool autoHarvestEnabled;

    public static DoggyEffectiveStats Resolve(DoggyData doggy, AchievementManager achievementManager)
    {
        if (doggy == null) return null;

        DoggyEffectiveStats stats = new DoggyEffectiveStats
        {
            rangeUp = doggy.RangeUp,
            rangeDown = doggy.RangeDown,
            rangeLeft = doggy.RangeLeft,
            rangeRight = doggy.RangeRight,
            growthSpeedModifier = doggy.growthSpeedModifier,
            autoHarvestEnabled = doggy.autoHarvestEnabled
        };

        if (doggy.upgrades != null && achievementManager != null)
        {
            foreach (var upgrade in doggy.upgrades)
            {
                if (upgrade.requiredAchievement != null && achievementManager.IsCompleted(upgrade.requiredAchievement))
                {
                    switch (upgrade.statType)
                    {
                        case DoggyStatType.AoERange:
                            int rangeBonus = Mathf.RoundToInt(upgrade.value);
                            if ((upgrade.directions & DoggyDirectionFlags.Up) != 0)
                            {
                                stats.rangeUp += rangeBonus;
                            }
                            if ((upgrade.directions & DoggyDirectionFlags.Down) != 0)
                            {
                                stats.rangeDown += rangeBonus;
                            }
                            if ((upgrade.directions & DoggyDirectionFlags.Left) != 0)
                            {
                                stats.rangeLeft += rangeBonus;
                            }
                            if ((upgrade.directions & DoggyDirectionFlags.Right) != 0)
                            {
                                stats.rangeRight += rangeBonus;
                            }
                            break;

                        case DoggyStatType.GrowthSpeedModifier:
                            stats.growthSpeedModifier = upgrade.value;
                            break;

                        case DoggyStatType.AutoHarvest:
                            if (upgrade.value != 0f)
                            {
                                stats.autoHarvestEnabled = true;
                            }
                            break;
                    }
                }
            }
        }

        return stats;
    }
}
