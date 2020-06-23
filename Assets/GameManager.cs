using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    [SerializeField] EntityPrefab floorPrefab = null;
    [SerializeField] EntityPrefab wallPrefab = null;

    public Map map;

    void Start()
    {
        instance = this;

        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        map.array =
            new NativeArray<Entity>(map.size.x * map.size.y, Allocator.Persistent);

        for (int x = 0; x < map.size.x; x++)
            for (int y = 0; y < map.size.y; y++)
            {
                Entity entity;
                if (x == 0 || y == 0 || x == map.size.x - 1 || y == map.size.y - 1 ||
                    (x == 15 && y < 14 && y > 7))
                    entity = entityManager.Instantiate(wallPrefab);
                else
                    entity = entityManager.Instantiate(floorPrefab);

                entityManager.SetName(entity, $"Tile({x},{y})");

                entityManager.SetComponentData(entity,
                    new GridPosition { Value = new int2(x, y) });

                map.SetTileEntity(x, y, entity);
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
    }
}
