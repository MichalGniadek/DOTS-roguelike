using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public class TileUpdateSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var ecb = ecbSystem.CreateCommandBuffer();

        Entities.WithoutBurst().ForEach(
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
        var tileRenderes = GameManager.instance.tileRenderes;
        var tilesData = GameManager.instance.tilesData;

        // Entities.WithAll<Character>().ForEach(
        //     (Entity entity, ref GridPosition position) =>
        //     {
        //         Entity tileEntity = map.GetTileEntity(position.Value);
        //         var tile = GetComponent<Tile>(tileEntity);

        //         // Add a check if an entity already exists here
        //         tile.blockingEntity = entity;
        //         tile.isMovementBlocked = true;

        //         SetComponent(tileEntity, tile);
        //     }
        // ).Run();

        Entities.WithoutBurst().ForEach((PlayerInput p) =>
        {
            for (int x = 0; x < map.size.x; x++)
                for (int y = 0; y < map.size.y; y++)
                {
                    if (map.GetTile(x, y).changed)
                    {
                        var e = map.GetTile(x, y);
                        e.changed = false;
                        map.SetTile(x, y, e);

                        tileRenderes[y + map.size.x * x].sprite =
                            tilesData[(int)map.GetTileType(x, y)].sprite;
                    }
                }
        }).Run();

        ecbSystem.AddJobHandleForProducer(Dependency);
    }

    private EndInitializationEntityCommandBufferSystem ecbSystem;

    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
    }
}