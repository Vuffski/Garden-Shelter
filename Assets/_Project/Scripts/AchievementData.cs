using System.Collections.Generic;
using UnityEngine;

public enum AchievementType
{
    HarvestTotal,
    HarvestSpecificPlant,
    EarnCoins
}

[CreateAssetMenu(fileName = "NewAchievement", menuName = "Garden/Achievement Data")]
public class AchievementData : ScriptableObject
{
    public string Title;
    public AchievementType Type;
    public PlantData SpecificPlant;
    public int TargetValue;
    public string vnScriptName;

    [TextArea]
    public string UnlockDescription;
    public List<PlantData> PlantsToUnlock;
    public List<DoggyData> DoggiesToUnlock;
    public int TreatReward = 0;
    [SerializeField] private int maxDoggySlotIncrease = 0;
    public int MaxDoggySlotIncrease => maxDoggySlotIncrease;
    [SerializeField] private int maxGoldenTilesIncrease = 0;
    public int MaxGoldenTilesIncrease => maxGoldenTilesIncrease;
    public int MaxMoneyIncrease = 0;

    [Header("Prerequisites")]
    public AchievementData RequiredAchievement;
    public DoggyData RequiredDoggy;
}