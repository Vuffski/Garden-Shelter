using System.Collections.Generic;
using UnityEngine;

public class DoggyFieldManager : MonoBehaviour
{
    private Dictionary<DoggyData, TileView> activeDoggies = new Dictionary<DoggyData, TileView>();

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
        activeDoggies[doggy] = tile;
    }

    public void UnregisterDoggy(DoggyData doggy)
    {
        if (doggy == null) return;
        activeDoggies.Remove(doggy);
    }
}
