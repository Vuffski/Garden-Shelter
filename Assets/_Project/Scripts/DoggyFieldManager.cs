using System;
using System.Collections.Generic;
using UnityEngine;

public class DoggyFieldManager : MonoBehaviour
{
    [SerializeField] private int baseMaxActiveDoggies = 1;
    private int bonusMaxActiveDoggies = 0;

    private Dictionary<DoggyData, TileView> activeDoggies = new Dictionary<DoggyData, TileView>();
    private List<DoggyData> activeDoggyList = new List<DoggyData>();

    public event Action OnActiveDoggiesChanged;

    public int MaxActiveDoggies => baseMaxActiveDoggies + bonusMaxActiveDoggies;
    public int ActiveDoggyCount => activeDoggies.Count;

    public bool IsDoggyActive(DoggyData doggy)
    {
        if (doggy == null) return false;
        return activeDoggies.ContainsKey(doggy);
    }

    public TileView GetTileFor(DoggyData doggy)
    {
        if (doggy == null) return null;
        if (activeDoggies.TryGetValue(doggy, out TileView tile))
        {
            return tile;
        }
        return null;
    }

    public void RegisterDoggy(DoggyData doggy, TileView tile)
    {
        if (doggy == null) return;
        if (!activeDoggies.ContainsKey(doggy))
        {
            activeDoggyList.Add(doggy);
        }
        activeDoggies[doggy] = tile;
        OnActiveDoggiesChanged?.Invoke();
    }

    public void UnregisterDoggy(DoggyData doggy)
    {
        if (doggy == null) return;
        if (activeDoggies.Remove(doggy))
        {
            activeDoggyList.Remove(doggy);
        }
        OnActiveDoggiesChanged?.Invoke();
    }

    public IEnumerable<DoggyData> GetActiveDoggies()
    {
        return activeDoggyList;
    }

    public bool CanPlaceNewDoggy()
    {
        return ActiveDoggyCount < MaxActiveDoggies;
    }

    public void IncreaseMaxActiveDoggies(int amount)
    {
        bonusMaxActiveDoggies += amount;
        OnActiveDoggiesChanged?.Invoke();
    }
}
