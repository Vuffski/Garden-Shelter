using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPlantData", menuName = "Garden/Plant Data")]
public class PlantData : ScriptableObject
{
    public string PlantName;
    public Sprite Icon;
    public Color IconColor = Color.white;
    public int Cost;
    public float GrowthTime;
    public int SellValue;
    public List<int> StorageExpansionCosts;
}
