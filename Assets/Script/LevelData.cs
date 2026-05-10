using UnityEngine;
using System.Collections.Generic;

public struct TilePos
{
    public float X;
    public float Y;
    public int Layer;

    public TilePos(float x, float y, int layer)
    {
        X = x;
        Y = y;
        Layer = layer;
    }
}

public static class LevelData 
{
    public static readonly TilePos[] Layout1 = new TilePos[] 
    {
        // LAYER 0
        new TilePos(-3, -3, 0), new TilePos(-2, -2, 0), new TilePos(-1, -1, 0), new TilePos(0, 0, 0), 
        new TilePos( 1,  1, 0), new TilePos( 2,  2, 0), new TilePos( 3,  3, 0),

        new TilePos(-3,  3, 0), new TilePos(-2,  2, 0), new TilePos(-1,  1, 0), 
        new TilePos( 1, -1, 0), new TilePos( 2, -2, 0), new TilePos( 3, -3, 0),

        new TilePos( 0, -1, 0), new TilePos( 0,  1, 0), new TilePos(-1,  0, 0), new TilePos( 1, 0, 0),

        // LAYER 1
        new TilePos(-2, -2, 1), new TilePos(-1, -1, 1), new TilePos(0, 0, 1), 
        new TilePos( 1,  1, 1), new TilePos( 2,  2, 1),

        new TilePos(-2,  2, 1), new TilePos(-1,  1, 1),  
        new TilePos( 1, -1, 1), new TilePos( 2, -2, 1),

        // LAYER 2
        new TilePos(-0.5f, -0.5f, 2), new TilePos(0.5f, -0.5f, 2), 
        new TilePos(-0.5f,  0.5f, 2), new TilePos(0.5f,  0.5f, 2),

        // LAYER 3
        new TilePos( 0,    -0.5f, 3), new TilePos( 0,     0.5f, 3), 
        new TilePos(-0.5f,  0,    3), new TilePos( 0.5f,  0,    3),

        // LAYER 4 & 5
        new TilePos(0, 0, 4), 
        new TilePos(0, 0, 5)
    };

    public static readonly TilePos[] Layout2 = new TilePos[]
    {
        new TilePos(-1, -1, 0), new TilePos(0, -1, 0), new TilePos(1, -1, 0),
        new TilePos(-1,  0, 0), new TilePos(0,  0, 0), new TilePos(1,  0, 0),
        new TilePos(-1,  1, 0), new TilePos(0,  1, 0), new TilePos(1,  1, 0),
        
        new TilePos(-0.5f, -0.5f, 1), new TilePos(0.5f, -0.5f, 1),
        new TilePos(-0.5f,  0.5f, 1), new TilePos(0.5f,  0.5f, 1),
        
        new TilePos(0, 0, 2),
        new TilePos(0, 0, 3) 
    };

    public static readonly TilePos[][] AllLayouts = new TilePos[][] 
    {
        Layout1,
        Layout2
    };

    public static TilePos[] GetLayout(int level)
    {
        int index = (level - 1) % AllLayouts.Length;
        return AllLayouts[index];
    }
}

public static class LevelGenerator
{
    public static List<TileModel> GenerateProcedural(int level, Sprite[] icons, int maxTraySlots)
    {
        int numTriplets = 10 + level * 2;
        var boardTiles = new List<TileModel>();
        var tray = new List<Sprite>();
        
        int generatedTriplets = 0;
        int tileIdCounter = 0;

        float minX = -3f; float maxX = 3f;
        float minY = -3f; float maxY = 3f;

        while (generatedTriplets < numTriplets || tray.Count > 0)
        {
            bool canAdd = generatedTriplets < numTriplets && tray.Count <= maxTraySlots - 3;
            bool canPlace = tray.Count > 0;

            bool doAdd = false;
            if (canAdd && canPlace) doAdd = Random.value > 0.5f;
            else if (canAdd) doAdd = true;

            if (doAdd)
            {
                Sprite icon = icons[Random.Range(0, icons.Length)];
                tray.Add(icon); tray.Add(icon); tray.Add(icon);
                generatedTriplets++;
            }
            else
            {
                int trayIndex = Random.Range(0, tray.Count);
                Sprite iconToPlace = tray[trayIndex];
                tray.RemoveAt(trayIndex);

                float x = Mathf.Round(Random.Range(minX, maxX) * 2f) / 2f;
                float y = Mathf.Round(Random.Range(minY, maxY) * 2f) / 2f;

                int maxLayer = -1;
                foreach (var tile in boardTiles)
                {
                    if (Mathf.Abs(tile.X - x) < GameConstants.OVERLAP_THRESHOLD &&
                        Mathf.Abs(tile.Y - y) < GameConstants.OVERLAP_THRESHOLD)
                    {
                        if (tile.Z > maxLayer) maxLayer = tile.Z;
                    }
                }

                boardTiles.Add(new TileModel(tileIdCounter++, x, y, maxLayer + 1, iconToPlace));
            }
        }

        return boardTiles;
    }

    public static List<TileModel> FillPredefined(TilePos[] layout, Sprite[] icons)
    {
        var result = new List<TileModel>();
        var remaining = new List<TilePos>(layout);
        int tileId = 0;
        
        var iconPool = new List<Sprite>();
        int numPairs = layout.Length / GameConstants.MATCH_COUNT;
        for (int i = 0; i < numPairs; i++)
        {
            Sprite icon = icons[Random.Range(0, icons.Length)];
            for (int j = 0; j < GameConstants.MATCH_COUNT; j++) iconPool.Add(icon);
        }

        while (remaining.Count >= GameConstants.MATCH_COUNT)
        {
            var freeTiles = new List<TilePos>();
            foreach (var t in remaining)
            {
                bool isBlocked = false;
                foreach (var other in remaining)
                {
                    if (other.Layer > t.Layer && 
                        Mathf.Abs(other.X - t.X) < GameConstants.OVERLAP_THRESHOLD && 
                        Mathf.Abs(other.Y - t.Y) < GameConstants.OVERLAP_THRESHOLD)
                    {
                        isBlocked = true;
                        break;
                    }
                }
                if (!isBlocked) freeTiles.Add(t);
            }
            
            if (freeTiles.Count < GameConstants.MATCH_COUNT) break; 
            
            Sprite currentIcon = iconPool[0];
            iconPool.RemoveRange(0, GameConstants.MATCH_COUNT);
            
            for (int i = 0; i < GameConstants.MATCH_COUNT; i++)
            {
                int rIndex = Random.Range(0, freeTiles.Count);
                TilePos picked = freeTiles[rIndex];
                freeTiles.RemoveAt(rIndex);
                remaining.Remove(picked);
                
                result.Add(new TileModel(tileId++, picked.X, picked.Y, picked.Layer, currentIcon));
            }
        }
        
        foreach (var leftover in remaining)
        {
            Sprite icon = iconPool.Count > 0 ? iconPool[0] : icons[0];
            if (iconPool.Count > 0) iconPool.RemoveAt(0);
            result.Add(new TileModel(tileId++, leftover.X, leftover.Y, leftover.Layer, icon));
        }
        
        return result;
    }
}
