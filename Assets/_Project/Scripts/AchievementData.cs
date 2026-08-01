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
}