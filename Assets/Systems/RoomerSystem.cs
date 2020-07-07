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
                    if (map.GetTileType(tilePosition) != TileType.Wall)
                    {
                        canBuild = false;
                        break;
                    }
                }
                if (!canBuild) break;
            }

            if (canBuild)
            {
                map.SetTileType(position.Value, TileType.Door);

                // Build room
                for (int x = -roomer.roomSize / 2; x <= roomer.roomSize / 2; x++)
                {
                    for (int y = -roomer.roomSize / 2; y <= roomer.roomSize / 2; y++)
                    {
                        map.SetTileType(roomCenter + new int2(x, y),
                            TileType.Floor);
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