using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    public EntityPrefab floorPrefab = null;
    public EntityPrefab wallPrefab = null;
    public EntityPrefab tunnelerPrefab = null;

    public Map map;

    void Start()
    {
        instance = this;

        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        map.array =
            new NativeArray<Entity>(map.size.x * map.size.y, Allocator.Persistent);

        for (int x = -1; x <= map.size.x; x++)
            for (int y = -1; y <= map.size.y; y++)
            {
                Entity entity = entityManager.Instantiate(wallPrefab);
                entityManager.SetName(entity, $"Tile({x},{y})");
                entityManager.SetComponentData(entity,
                    new GridPosition { Value = new int2(x, y) });
                if (map.InBounds(x, y))
                    map.SetTileEntity(x, y, entity);
            }

        {
            Entity tunneler = entityManager.Instantiate(tunnelerPrefab);
            entityManager.SetComponentData(tunneler,
                new GridPosition { Value = new int2(map.size.x / 2, map.size.y / 2) }
            );
            var tunnelerData = entityManager.GetComponentData<Tunneler>(tunneler);
            tunnelerData.direction = Direction.Up;
            entityManager.SetComponentData(tunneler, tunnelerData);
        }

        {
            Entity tunneler = entityManager.Instantiate(tunnelerPrefab);
            entityManager.SetComponentData(tunneler,
                new GridPosition { Value = new int2(2, 2) }
            );
            var tunnelerData = entityManager.GetComponentData<Tunneler>(tunneler);
            tunnelerData.direction = Direction.Up;
            entityManager.SetComponentData(tunneler, tunnelerData);
        }

        {
            Entity tunneler = entityManager.Instantiate(tunnelerPrefab);
            entityManager.SetComponentData(tunneler,
                new GridPosition { Value = new int2(map.size.x - 2, map.size.y - 2) }
            );
            var tunnelerData = entityManager.GetComponentData<Tunneler>(tunneler);
            tunnelerData.direction = Direction.Down;
            entityManager.SetComponentData(tunneler, tunnelerData);
        }
    }

    void OnDestroy()
    {
        map.array.Dispose();
    }

    [System.Serializable]
    public struct Map
    {
        public Vector2Int size;
        public NativeArray<Entity> array;

        public Entity GetTileEntity(int x, int y)
        {
            if (!InBounds(x, y)) return Entity.Null;
            return array[x + size.x * y];
        }

        public Entity GetTileEntity(int2 position)
        {
            return GetTileEntity(position.x, position.y);
        }

        public void SetTileEntity(int x, int y, Entity entity)
        {
            array[x + size.x * y] = entity;
        }

        public void SetTileEntity(int2 position, Entity entity)
        {
            SetTileEntity(position.x, position.y, entity);
        }

        public bool InBounds(int x, int y)
        {
            return x > 0 && y > 0 && x < size.x && y < size.y;
        }

        public bool InBounds(int2 position)
        {
            return InBounds(position.x, position.y);
        }
    }
}
