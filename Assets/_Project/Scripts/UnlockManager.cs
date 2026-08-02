using System;
using System.Collections.Generic;
using UnityEngine;

public class UnlockManager : MonoBehaviour
{
    [SerializeField] private List<PlantData> defaultUnlockedPlants;
    [SerializeField] private List<DoggyData> defaultUnlockedDoggies;
    [SerializeField] private AchievementManager achievementManager;

    private HashSet<PlantData> unlockedPlants = new HashSet<PlantData>();
    private HashSet<DoggyData> unlockedDoggies = new HashSet<DoggyData>();

    public event Action<PlantData> OnPlantUnlocked;
    public event Action<DoggyData> OnDoggyUnlocked;

    private void Awake()
    {
        if (defaultUnlockedPlants != null)
        {
            foreach (var plant in defaultUnlockedPlants)
            {
                if (plant != null)
                {
                    unlockedPlants.Add(plant);
                }
            }
        }

        if (defaultUnlockedDoggies != null)
        {
            foreach (var doggy in defaultUnlockedDoggies)
            {
                if (doggy != null)
                {
                    unlockedDoggies.Add(doggy);
                }
            }
        }
    }

    private void Start()
    {
        if (achievementManager != null)
        {
            achievementManager.OnAchievementCompleted += HandleAchievementCompleted;
        }
        else
        {
            Debug.LogWarning("AchievementManager reference is missing in UnlockManager!", this);
        }
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
        if (achievement == null) return;

        if (achievement.PlantsToUnlock != null)
        {
            foreach (var plant in achievement.PlantsToUnlock)
            {
                if (plant == null) continue;
                if (!unlockedPlants.Contains(plant))
                {
                    unlockedPlants.Add(plant);
                    OnPlantUnlocked?.Invoke(plant);
                }
            }
        }

        if (achievement.DoggiesToUnlock != null)
        {
            foreach (var doggy in achievement.DoggiesToUnlock)
            {
                if (doggy == null) continue;
                if (!unlockedDoggies.Contains(doggy))
                {
                    unlockedDoggies.Add(doggy);
                    OnDoggyUnlocked?.Invoke(doggy);
                }
            }
        }
    }

    public bool IsPlantUnlocked(PlantData plant)
    {
        if (plant == null) return false;
        return unlockedPlants.Contains(plant);
    }

    public bool IsDoggyUnlocked(DoggyData doggy)
    {
        if (doggy == null) return false;
        return unlockedDoggies.Contains(doggy);
    }
}
