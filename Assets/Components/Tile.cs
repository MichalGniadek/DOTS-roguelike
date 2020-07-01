using Unity.Entities;
using UnityEngine;

[GenerateAuthoringComponent]
public struct Tile : IComponentData
{
    public enum Type { Floor, Wall, FutureRoom, FutureTunnel, Door };
    public Type type;

    /// <summary> Wall, player, enemy </summary>
    [HideInInspector] public Entity blockingEntity;
    /// <summary> Item, trap, door </summary>
    [HideInInspector] public Entity nonBlockingEntity;

    public bool tileBlocksVision;
    public bool tileBlocksMovement;

    [HideInInspector] public bool isVisionBlocked;
    [HideInInspector] public bool isMovementBlocked;
    [HideInInspector] public bool IsSpaceForNonBlocking;
}