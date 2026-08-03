using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDoggyData", menuName = "Garden/Doggy Data")]
public class DoggyData : ScriptableObject
{
    public string DoggyName;
    public Sprite Icon;
    public Color IconColor = Color.white;
    public float Duration;
    public List<PlantCost> Costs;

    [Header("Ranges")]
    public int RangeUp = 0;
    public int RangeUpRight = 0;
    public int RangeRight = 0;
    public int RangeDownRight = 0;
    public int RangeDown = 0;
    public int RangeDownLeft = 0;
    public int RangeLeft = 0;
    public int RangeUpLeft = 0;

    public Color AoeColor = Color.white;
    public float FlashFrequency = 1f;

    [Header("Growth and Yield Settings")]
    public bool UseGrowthSpeed;
    public float GrowthMultiplier = 1f;
    public bool UseYield;
    public float YieldChance = 0f;

    [Header("Baseline Settings")]
    public float growthSpeedModifier = 1f;
    public bool autoHarvestEnabled = false;

    [System.Serializable]
    public class DoggyUpgrade
    {
        public AchievementData requiredAchievement;
        public DoggyStatType statType;
        public DoggyDirectionFlags directions; // only used when statType == AoERange
        public float value;
    }

    [Header("Upgrades")]
    public List<DoggyUpgrade> upgrades;
}
