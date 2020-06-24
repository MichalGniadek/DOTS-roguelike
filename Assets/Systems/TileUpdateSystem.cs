using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public class TileUpdateSystem : SystemBase
{
    // All this is probably going to get changed
    // because two entites can walk/ get pushed
    // into the same tile if it happend at the same tick
    // There probably need to be some kind of helper
    // function that moves entity from one tile to another.
    protected override void OnUpdate()
    {
        var ecb = ecbSystem.CreateCommandBuffer();

        Entities.WithoutBurst().WithStructuralChanges().ForEach(
        (int entityInQueryIndex, Entity entity, SpriteRenderer sprite, in Tile tile, in TileChanger tileChanger) =>
        {
            var newSprite = EntityManager
                .GetComponentObject<SpriteRenderer>(tileChanger.newTilePrefab);
            sprite.sprite = newSprite.sprite;
            sprite.color = newSprite.color;

            var newTileData = GetComponent<Tile>(tileChanger.newTilePrefab);

            newTileData.blockingEntity = tile.blockingEntity;
            newTileData.nonBlockingEntity = tile.nonBlockingEntity;

            ecb.SetComponent(entity, newTileData);
            ecb.RemoveComponent<TileChanger>(entity);
        }
        ).Run();

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

        ecbSystem.AddJobHandleForProducer(Dependency);
    }

    private EndInitializationEntityCommandBufferSystem ecbSystem;

    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
    }
}