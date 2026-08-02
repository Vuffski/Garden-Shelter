using System;
using System.Collections.Generic;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    [SerializeField] private List<AchievementData> allAchievements;

    private int totalHarvested;
    private Dictionary<PlantData, int> perPlantHarvested = new Dictionary<PlantData, int>();
    private int totalCoinsEarned;

    private HashSet<AchievementData> completedAchievements = new HashSet<AchievementData>();

    public event Action<AchievementData> OnAchievementCompleted;

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

        CheckAllAchievements();
    }

    public void RegisterCoinsEarned(int amount)
    {
        if (amount <= 0) return;
        totalCoinsEarned += amount;

        CheckAllAchievements();
    }

    private void CheckAllAchievements()
    {
        if (allAchievements == null) return;

        foreach (var achievement in allAchievements)
        {
            if (achievement == null) continue;
            if (completedAchievements.Contains(achievement)) continue;

            if (achievement.TargetValue > 0 && (float)GetProgress(achievement) / achievement.TargetValue >= 1f)
            {
                completedAchievements.Add(achievement);
                OnAchievementCompleted?.Invoke(achievement);
            }
        }
    }

    public bool IsCompleted(AchievementData achievement)
    {
        return completedAchievements.Contains(achievement);
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