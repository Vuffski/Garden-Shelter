using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDoggyData", menuName = "Garden/Doggy Data")]
public class DoggyData : ScriptableObject
{
    public string DoggyName;
    public Sprite Icon;
    public float Duration;
    public List<PlantCost> Costs;
}
