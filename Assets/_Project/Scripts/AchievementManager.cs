using System;
using System.Collections.Generic;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    [SerializeField] private List<AchievementData> allAchievements;
    [SerializeField] private UnlockManager unlockManager;
    [SerializeField] private TreatManager treatManager;
    [SerializeField] private DoggyFieldManager doggyFieldManager;
    [SerializeField] private GoldenTileManager goldenTileManager;

    public IReadOnlyList<AchievementData> AllAchievements => allAchievements;

    private int totalHarvested;
    private Dictionary<PlantData, int> perPlantHarvested = new Dictionary<PlantData, int>();
    private int totalCoinsEarned;

    private HashSet<AchievementData> completedAchievements = new HashSet<AchievementData>();
    private HashSet<AchievementData> readyToCollectAchievements = new HashSet<AchievementData>();

    public event Action<AchievementData> OnAchievementCompleted;
    public event Action<AchievementData> OnAchievementReadyToCollect;

    public bool IsReadyToCollect(AchievementData achievement) => readyToCollectAchievements.Contains(achievement);

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

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
            if (readyToCollectAchievements.Contains(achievement)) continue;

            if (achievement.TargetValue > 0 && (float)GetProgress(achievement) / achievement.TargetValue >= 1f)
            {
                readyToCollectAchievements.Add(achievement);
                OnAchievementReadyToCollect?.Invoke(achievement);
            }
        }
    }

    public async void ClaimAchievement(AchievementData achievement)
    {
        if (achievement == null || !readyToCollectAchievements.Contains(achievement) || completedAchievements.Contains(achievement))
        {
            return;
        }

        readyToCollectAchievements.Remove(achievement);
        completedAchievements.Add(achievement);
        OnAchievementCompleted?.Invoke(achievement);

        if (treatManager != null && achievement.TreatReward > 0)
        {
            treatManager.AddTreats(achievement.TreatReward);
        }

        if (doggyFieldManager != null && achievement.MaxDoggySlotIncrease > 0)
        {
            doggyFieldManager.IncreaseMaxActiveDoggies(achievement.MaxDoggySlotIncrease);
        }

        if (goldenTileManager != null && achievement.MaxGoldenTilesIncrease > 0)
        {
            goldenTileManager.IncreaseMaxGoldenTiles(achievement.MaxGoldenTilesIncrease);
        }

        if (!string.IsNullOrEmpty(achievement.vnScriptName))
        {
            var scriptPlayer = Naninovel.Engine.GetService<Naninovel.IScriptPlayer>();
            if (scriptPlayer != null)
            {
                await scriptPlayer.PreloadAndPlayAsync(achievement.vnScriptName);
            }
        }
    }

    public bool IsCompleted(AchievementData achievement)
    {
        return completedAchievements.Contains(achievement);
    }

    public bool IsUnlocked(AchievementData achievement)
    {
        if (achievement == null) return false;

        if (achievement.RequiredAchievement != null && !IsCompleted(achievement.RequiredAchievement))
        {
            return false;
        }

        if (achievement.RequiredDoggy != null && (unlockManager == null || !unlockManager.IsDoggyUnlocked(achievement.RequiredDoggy)))
        {
            return false;
        }

        return true;
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

public static class ScriptPlayerExtensions
{
    public static async UnityEngine.Awaitable PreloadAndPlayAsync(this Naninovel.IScriptPlayer player, string scriptName)
    {
        await Naninovel.ScriptTrackExtensions.LoadAndPlay(player.MainTrack, scriptName);
    }
}