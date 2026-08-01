using System.Collections.Generic;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    private int totalHarvested;
    private Dictionary<PlantData, int> perPlantHarvested = new Dictionary<PlantData, int>();
    private int totalCoinsEarned;

    public void RegisterHarvest(PlantData plant, int amount)
    {
        if (plant == null || amount <= 0) return;

        totalHarvested += amount;

        if (perPlantHarvested.ContainsKey(plant))
        {
            perPlantHarvested[plant] += amount;
        }
        else
        {
            perPlantHarvested[plant] = amount;
        }
    }

    public void RegisterCoinsEarned(int amount)
    {
        if (amount <= 0) return;
        totalCoinsEarned += amount;
    }

    public int GetProgress(AchievementData achievement)
    {
        if (achievement == null) return 0;

        switch (achievement.Type)
        {
            case AchievementType.HarvestTotal:
                return totalHarvested;

            case AchievementType.HarvestSpecificPlant:
                if (achievement.SpecificPlant != null && perPlantHarvested.TryGetValue(achievement.SpecificPlant, out int count))
                {
                    return count;
                }
                return 0;

            case AchievementType.EarnCoins:
                return totalCoinsEarned;

            default:
                return 0;
        }
    }
}