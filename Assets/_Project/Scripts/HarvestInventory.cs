using System;
using System.Collections.Generic;
using UnityEngine;

public class HarvestInventory : MonoBehaviour
{
    private Dictionary<PlantData, int> counts = new Dictionary<PlantData, int>();

    public event Action<PlantData> OnCountChanged;

    public int GetCount(PlantData plant)
    {
        if (plant == null) return 0;
        if (counts.TryGetValue(plant, out int count))
        {
            return count;
        }
        return 0;
    }

    public void AddHarvest(PlantData plant, int amount = 1)
    {
        if (plant == null || amount <= 0) return;
        if (counts.ContainsKey(plant))
        {
            counts[plant] += amount;
        }
        else
        {
            counts[plant] = amount;
        }
        OnCountChanged?.Invoke(plant);
    }

    public bool CanAfford(List<PlantCost> costs)
    {
        if (costs == null) return true;
        foreach (var cost in costs)
        {
            if (GetCount(cost.Plant) < cost.Amount)
            {
                return false;
            }
        }
        return true;
    }

    public void Spend(List<PlantCost> costs)
    {
        if (costs == null) return;
        foreach (var cost in costs)
        {
            if (cost.Plant == null) continue;
            if (counts.ContainsKey(cost.Plant))
            {
                counts[cost.Plant] -= cost.Amount;
            }
            else
            {
                counts[cost.Plant] = -cost.Amount;
            }
            OnCountChanged?.Invoke(cost.Plant);
        }
    }
}