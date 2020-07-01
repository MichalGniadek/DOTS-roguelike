using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

//[DisableAutoCreation]
public class RoomerSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var map = GameManager.instance.map;
        Entity futureRoomTile = GameManager.instance.futureRoomTile;
        Entity floorTile = GameManager.instance.floorPrefab;
        Entity doorTile = GameManager.instance.doorPrefab;

        Entities.WithStructuralChanges().ForEach(
        (Entity entity, ref GridPosition position, ref Roomer roomer) =>
        {
            int2 roomCenter = position.Value +
                roomer.direction.ToInt2() * (1 + (roomer.roomSize / 2));

            bool canBuild = true;
            for (int x = -1 - roomer.roomSize / 2; x <= 1 + roomer.roomSize / 2; x++)
            {
                for (int y = -1 - roomer.roomSize / 2; y <= 1 + roomer.roomSize / 2; y++)
                {
                    int2 tilePosition = roomCenter + new int2(x, y);
                    Entity tileEntity = map.GetTileEntity(tilePosition);
                    if (tileEntity == Entity.Null)
                    {
                        canBuild = false;
                        break;
                    }
                    else
                    {
                        var tile = EntityManager.GetComponentData<Tile>(tileEntity);
                        if (tile.type != Tile.Type.Wall)
                        {
                            canBuild = false;
                        }
                    }
                }
                if (!canBuild) break;
            }

            if (canBuild)
            {
                // Build door
                {
                    Entity tileEntity = map.GetTileEntity(position.Value);

                    if (tileEntity != Entity.Null)
                    {
                        EntityManager.AddComponent<TileChanger>(tileEntity);
                        EntityManager.SetComponentData(tileEntity,
                        new TileChanger
                        {
                            newTilePrefab = doorTile
                        });
                    }
                }

                // Build room
                for (int x = -roomer.roomSize / 2; x <= roomer.roomSize / 2; x++)
                {
                    for (int y = -roomer.roomSize / 2; y <= roomer.roomSize / 2; y++)
                    {
                        int2 tilePosition = roomCenter + new int2(x, y);
                        Entity tileEntity = map.GetTileEntity(tilePosition);

                        EntityManager.AddComponent<TileChanger>(tileEntity);
                        EntityManager.SetComponentData(tileEntity,
                        new TileChanger
                        {
                            newTilePrefab = futureRoomTile
                        });
                    }
                }
            }
            EntityManager.DestroyEntity(entity);
        }).Run();

        Entities.WithStructuralChanges().ForEach(
        (Entity Entity, ref Tile tile) =>
        {
            if (tile.type == Tile.Type.FutureRoom
                || tile.type == Tile.Type.FutureTunnel)
            {
                EntityManager.AddComponent<TileChanger>(Entity);
                EntityManager.SetComponentData(Entity,
                new TileChanger
                {
                    newTilePrefab = floorTile
                });
            }
        }).Run();
    }

    protected override void OnCreate()
    {
        base.OnCreate();
        Enabled = false;
    }
}