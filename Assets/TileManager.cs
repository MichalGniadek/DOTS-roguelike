using System;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Unity.Entities;

public enum TileType : int { Floor, Wall, FutureRoom, FutureTunnel, Door, OutOfBounds };

[Serializable]
public struct TileData
{
    [HideInInspector] public string name;
    [ShowAssetPreview] public Sprite sprite;
    public bool tileBlocksVision;
    public bool tileBlocksMovement;
}

[Serializable]
public struct NativeTileData
{
    public bool tileBlocksVision;
    public bool tileBlocksMovement;

    public static implicit operator NativeTileData(TileData data)
    {
        var e = new NativeTileData();
        e.tileBlocksMovement = data.tileBlocksMovement;
        e.tileBlocksVision = data.tileBlocksVision;
        return e;
    }
}

[Serializable]
public struct NativeTile
{
    public int type;
    public bool changed;

    /// <summary> Player, enemy </summary>
    public Entity blockingEntity;
    /// <summary> Item, trap, door </summary>
    public Entity nonBlockingEntity;
}