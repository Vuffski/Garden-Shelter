using UnityEngine;

[CreateAssetMenu(fileName = "NewPlantData", menuName = "Garden/Plant Data")]
public class PlantData : ScriptableObject
{
    public string PlantName;
    public Sprite Icon;
    public int Cost;
    public float GrowthTime;
    public int SellValue;
}
