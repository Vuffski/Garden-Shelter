using System;
using System.Collections.Generic;
using UnityEngine;

public class HarvestInventory : MonoBehaviour
{
    public static HarvestInventory Instance { get; private set; }

    [SerializeField] private EconomyManager economyManager;

    private Dictionary<PlantData, int> counts = new Dictionary<PlantData, int>();
    private Dictionary<PlantData, int> expansionLevels = new Dictionary<PlantData, int>();

    public event Action<PlantData> OnCountChanged;

    private void Awake()
    {
        Instance = this;
        if (economyManager == null)
        {
            economyManager = FindAnyObjectByType<EconomyManager>();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public int GetCount(PlantData plant)
    {
        if (plant == null) return 0;
        if (counts.TryGetValue(plant, out int count))
        {
            return count;
        }
        return 0;
    }

    public int GetStorageCap(PlantData plant)
    {
        if (plant == null) return 1;
        if (expansionLevels.TryGetValue(plant, out int levels))
        {
            return 1 + levels;
        }
        return 1;
    }

    public bool IsAtStorageCap(PlantData plant)
    {
        if (plant == null) return false;
        return GetCount(plant) >= GetStorageCap(plant);
    }

    public bool HasMoreExpansionLevels(PlantData plant)
    {
        if (plant == null || plant.StorageExpansionCosts == null) return false;
        int levels = 0;
        expansionLevels.TryGetValue(plant, out levels);
        return levels < plant.StorageExpansionCosts.Count;
    }

    public int GetNextExpansionCost(PlantData plant)
    {
        if (plant == null || plant.StorageExpansionCosts == null) return 0;
        int levels = 0;
        expansionLevels.TryGetValue(plant, out levels);
        return plant.StorageExpansionCosts[levels];
    }

    public void ExpandStorage(PlantData plant)
    {
        if (plant == null) return;
        if (expansionLevels.ContainsKey(plant))
        {
            expansionLevels[plant]++;
        }
        else
        {
            expansionLevels[plant] = 1;
        }
        OnCountChanged?.Invoke(plant);
    }

    public void AddHarvest(PlantData plant, int amount = 1)
    {
        if (plant == null || amount <= 0) return;

        int currentCount = GetCount(plant);
        int cap = GetStorageCap(plant);
        int roomLeft = Mathf.Max(0, cap - currentCount);

        int toAdd = Mathf.Min(amount, roomLeft);
        int remainder = amount - roomLeft;

        if (toAdd > 0)
        {
            if (counts.ContainsKey(plant))
            {
                counts[plant] += toAdd;
            }
            else
            {
                counts[plant] = toAdd;
            }
        }

        if (remainder > 0 && economyManager != null)
        {
            economyManager.Earn(remainder * plant.SellValue);
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

    public void SpendOne(PlantData plant)
    {
        if (plant == null) return;
        Spend(new List<PlantCost> { new PlantCost { Plant = plant, Amount = 1 } });
    }
}