using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public class TileBlockingSystem : SystemBase
{
    // All this is probably going to get changed
    // because two entites can walk/ get pushed
    // into the same tile if it happend at the same tick
    // There probably need to be some kind of helper
    // function that moves entity from one tile to another.
    protected override void OnUpdate()
    {
        Entities.ForEach((ref Tile tile) =>
        {
            tile.blockingEntity = Entity.Null;
            tile.nonBlockingEntity = Entity.Null;
            tile.isMovementBlocked = tile.tileBlocksMovement;
            tile.IsSpaceForNonBlocking = !tile.tileBlocksMovement;
        }).ScheduleParallel();

        var map = GameManager.instance.map;

        Entities.WithAll<Character>().ForEach(
            (Entity entity, ref GridPosition position) =>
            {
                Entity tileEntity = map.GetTileEntity(position.Value);
                Tile tile = GetComponent<Tile>(tileEntity);

                tile.blockingEntity = entity;
                tile.isMovementBlocked = true;

                SetComponent(tileEntity, tile);
            }
        ).Run();// Not ScheduleParallel because of SetComponent()
    }
}