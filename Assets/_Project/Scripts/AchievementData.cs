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

    [TextArea]
    public string UnlockDescription;
    public List<PlantData> PlantsToUnlock;
    public List<DoggyData> DoggiesToUnlock;

    [Header("Prerequisites")]
    public AchievementData RequiredAchievement;
    public DoggyData RequiredDoggy;
}