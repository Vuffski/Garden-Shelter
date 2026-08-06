using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldenTileManager : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private int baseMaxGoldenTiles = 0;
    [SerializeField] private float goldenTileCooldown = 1f;
    [SerializeField] private float goldenTileFadeout = 30f;

    private int bonusMaxGoldenTiles = 0;
    public int MaxGoldenTiles => baseMaxGoldenTiles + bonusMaxGoldenTiles;

    private readonly List<Coroutine> slotRoutines = new List<Coroutine>();
    private readonly HashSet<TileView> ownedTiles = new HashSet<TileView>();

    private void Start()
    {
        SyncSlotCount();
    }

    public void IncreaseMaxGoldenTiles(int amount)
    {
        if (amount <= 0) return;
        bonusMaxGoldenTiles += amount;
        SyncSlotCount();
    }

    private void SyncSlotCount()
    {
        while (slotRoutines.Count < MaxGoldenTiles)
        {
            int slotIndex = slotRoutines.Count;
            Coroutine routine = StartCoroutine(GoldenSlotRoutine(slotIndex));
            slotRoutines.Add(routine);
        }
    }

    private IEnumerator GoldenSlotRoutine(int slotIndex)
    {
        while (true)
        {
            TileView tile = FindRandomFreeTile();

            if (tile == null)
            {
                yield return new WaitForSeconds(goldenTileCooldown);
                continue;
            }

            ownedTiles.Add(tile);
            bool freed = false;

            void OnOccupied(TileView t)
            {
                t.ActivateGoldenGlow();
            }

            void OnUnoccupied(TileView t)
            {
                t.ClearGolden();
                freed = true;
            }

            void OnExpired(TileView t)
            {
                freed = true;
            }

            tile.OnBecameOccupied += OnOccupied;
            tile.OnBecameUnoccupied += OnUnoccupied;
            tile.OnGoldenExpired += OnExpired;

            tile.SetGolden(goldenTileFadeout);

            yield return new WaitUntil(() => freed);

            tile.OnBecameOccupied -= OnOccupied;
            tile.OnBecameUnoccupied -= OnUnoccupied;
            tile.OnGoldenExpired -= OnExpired;

            ownedTiles.Remove(tile);

            yield return new WaitForSeconds(goldenTileCooldown);
        }
    }

    private TileView FindRandomFreeTile()
    {
        if (gridManager == null) return null;

        List<TileView> candidates = new List<TileView>();
        foreach (TileView tile in gridManager.GetAllTiles())
        {
            if (tile != null && !tile.IsOccupied && !tile.IsGolden && !ownedTiles.Contains(tile))
            {
                candidates.Add(tile);
            }
        }

        if (candidates.Count == 0) return null;
        return candidates[Random.Range(0, candidates.Count)];
    }
}
