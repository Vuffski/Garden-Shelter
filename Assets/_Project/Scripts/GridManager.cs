using UnityEngine;

public class GridManager : MonoBehaviour
{
    public const int GridWidth = 9;
    public const int GridHeight = 9;
    public const int BlockSize = 3;

    public GameObject tilePrefab;
    public float tileSize = 1f;
    public float blockGap = 0.15f;

    private Tile[,] grid;

    private void Awake()
    {
        GenerateGrid();
        SpawnVisuals();
    }

    private void GenerateGrid()
    {
        grid = new Tile[GridWidth, GridHeight];

        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                TileType type = (x % BlockSize == 1 && y % BlockSize == 1) ? TileType.Special : TileType.Basic;
                grid[x, y] = new Tile(x, y, type);
            }
        }
    }

    private void SpawnVisuals()
    {
        if (tilePrefab == null)
        {
            Debug.LogWarning("TilePrefab is not assigned in GridManager!");
            return;
        }

        GameObject tilesParent = new GameObject("Tiles");
        tilesParent.transform.SetParent(transform, false);

        float totalSpanX = (GridWidth - 1) * tileSize + (GridWidth / BlockSize - 1) * blockGap;
        float totalSpanY = (GridHeight - 1) * tileSize + (GridHeight / BlockSize - 1) * blockGap;

        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                Tile tile = grid[x, y];
                if (tile == null) continue;

                int blockX = x / BlockSize;
                int blockY = y / BlockSize;

                float rawX = x * tileSize + blockX * blockGap;
                float rawY = y * tileSize + blockY * blockGap;

                float posX = rawX - (totalSpanX / 2f);
                float posY = rawY - (totalSpanY / 2f);

                Vector3 position = new Vector3(posX, posY, 0f);
                GameObject spawnedVisual = Instantiate(tilePrefab, position, Quaternion.identity, tilesParent.transform);

                TileView tileView = spawnedVisual.GetComponent<TileView>();
                if (tileView != null)
                {
                    tileView.X = x;
                    tileView.Y = y;

                    Color fillColor = (tile.Type == TileType.Special) ? Color.yellow : Color.grey;
                    tileView.SetFillColor(fillColor);
                }
            }
        }
    }

    public Tile GetTile(int x, int y)
    {
        if (x < 0 || x >= GridWidth || y < 0 || y >= GridHeight)
        {
            return null;
        }

        return grid[x, y];
    }
}
