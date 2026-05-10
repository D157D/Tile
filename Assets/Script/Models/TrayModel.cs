using System;
using System.Collections.Generic;
using System.Linq;

public class TrayModel
{
    private readonly int maxSlots;
    private readonly List<TileModel> trayTiles = new List<TileModel>();

    public event Action<TileModel, int> OnTileAddedToTray;
    public event Action<List<TileModel>> OnTilesMatched;
    public event Action OnTrayFull;
    public event Action OnTrayChanged;

    public TrayModel(int maxSlots)
    {
        this.maxSlots = maxSlots;
    }

    public bool CanAdd() => trayTiles.Count < maxSlots;

    public void AddTile(TileModel tile)
    {
        if (!CanAdd()) return;

        tile.Status = TileStatus.Tray;

        int insertIndex = trayTiles.Count;
        for (int i = trayTiles.Count - 1; i >= 0; i--)
        {
            if (trayTiles[i].Icon == tile.Icon)
            {
                insertIndex = i + 1;
                break;
            }
        }
        trayTiles.Insert(insertIndex, tile);

        OnTileAddedToTray?.Invoke(tile, insertIndex);
        CheckForMatches();
    }

    private void CheckForMatches()
    {
        var counts = trayTiles.GroupBy(t => t.Icon).ToDictionary(g => g.Key, g => g.Count());
        var matchFoundIcon = counts.FirstOrDefault(x => x.Value >= GameConstants.MATCH_COUNT).Key;

        if (matchFoundIcon != null)
        {
            var matchedTiles = trayTiles.Where(t => t.Icon == matchFoundIcon).Take(GameConstants.MATCH_COUNT).ToList();
            foreach (var match in matchedTiles)
            {
                trayTiles.Remove(match);
                match.Status = TileStatus.Matched;
            }
            OnTilesMatched?.Invoke(matchedTiles);
            return;
        }

        if (trayTiles.Count >= maxSlots)
        {
            OnTrayFull?.Invoke();
        }
        else
        {
            OnTrayChanged?.Invoke();
        }
    }

    public int GetCount() => trayTiles.Count;
    public IReadOnlyList<TileModel> GetTiles() => trayTiles;
}
