using System;
using UnityEngine;

public class TileModel
{
    public int Id { get; }
    public float X { get; }
    public float Y { get; }
    public int Z { get; }
    public Sprite Icon { get; }

    private TileStatus status;
    public TileStatus Status 
    { 
        get => status; 
        set { status = value; OnStatusChanged?.Invoke(value); } 
    }

    private bool isBlocked;
    public bool IsBlocked 
    { 
        get => isBlocked; 
        set { isBlocked = value; OnBlockedChanged?.Invoke(value); } 
    }

    public event Action<TileStatus> OnStatusChanged;
    public event Action<bool> OnBlockedChanged;

    public TileModel(int id, float x, float y, int z, Sprite icon)
    {
        Id = id;
        X = x;
        Y = y;
        Z = z;
        Icon = icon;
        Status = TileStatus.Board;
    }
}
