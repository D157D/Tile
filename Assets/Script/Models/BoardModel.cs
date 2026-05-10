using System.Collections.Generic;
using System.Linq;
using System;

public class BoardModel
{
    private List<TileModel> activeTiles = new List<TileModel>();

    public IReadOnlyList<TileModel> ActiveTiles => activeTiles;

    public void Initialize(List<TileModel> tiles)
    {
        activeTiles = tiles;
        UpdateBlockedStatus();
    }

    public void UpdateBlockedStatus()
    {
        foreach (var tile in activeTiles)
        {
            if (tile.Status != TileStatus.Board) continue;

            bool isBlocked = false;
            foreach (var other in activeTiles)
            {
                if (other.Status == TileStatus.Board && other.Z > tile.Z)
                {
                    float dx = Math.Abs(tile.X - other.X);
                    float dy = Math.Abs(tile.Y - other.Y);
                    if (dx < GameConstants.OVERLAP_THRESHOLD && dy < GameConstants.OVERLAP_THRESHOLD)
                    {
                        isBlocked = true;
                        break;
                    }
                }
            }
            tile.IsBlocked = isBlocked;
        }
    }

    public int GetRemainingBoardTilesCount()
    {
        return activeTiles.Count(t => t.Status == TileStatus.Board);
    }
}
