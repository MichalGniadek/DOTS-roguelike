using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using NaughtyAttributes;
using System.Collections.Generic;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    public EntityPrefab floorPrefab = null;
    public EntityPrefab wallPrefab = null;
    public EntityPrefab futureRoomTile = null;
    public EntityPrefab futureTunnelTile = null;
    public EntityPrefab tunnelerPrefab = null;
    public EntityPrefab roomerPrefab = null;
    public EntityPrefab doorPrefab = null;

    public Map map;

    [SerializeField, ReorderableList]
    public List<TileData> tilesData;

    [SerializeField]
    public GameObject tileRendererPrefab;
    public List<SpriteRenderer> tileRenderes;

    void OnValidate()
    {
        var tileTypeArray = Enum.GetValues(typeof(TileType));
        foreach (int item in tileTypeArray)
        {
            TileData element;
            if (item < tilesData.Count) element = tilesData[item];
            else element = new TileData();

            element.name = Enum.GetName(typeof(TileType), item);

            if (item < tilesData.Count) tilesData[item] = element;
            else tilesData.Add(element);
        }

        var tileTypes = tileTypeArray.Length;
        if (tileTypes < tilesData.Count)
        {
            tilesData.RemoveRange(tileTypes, tilesData.Count - tileTypes);
        }
    }

    [Button]
    void ResetTilesData()
    {
        tilesData = new List<TileData>();
        OnValidate();
    }

    void Start()
    {
        instance = this;
        tileRenderes = new List<SpriteRenderer>();
        for (int x = 0; x < map.size.x; x++)
            for (int y = 0; y < map.size.y; y++)
            {
                var go = GameManager.Instantiate(
                    tileRendererPrefab,
                    new Vector3(x, y, 0),
                    Quaternion.identity,
                    transform
                );

                tileRenderes.Add(go.GetComponent<SpriteRenderer>());
            }

        map.tilesData = new NativeArray<NativeTileData>(
            Array.ConvertAll(tilesData.ToArray(), item => (NativeTileData)item),
            Allocator.Persistent
        );

        map.array = new NativeArray<NativeTile>(
            map.size.x * map.size.y,
            Allocator.Persistent
        );

        for (int i = 0; i < map.size.x * map.size.y; i++)
        {
            var e = map.array[i];
            e.blockingEntity = Entity.Null;
            e.nonBlockingEntity = Entity.Null;
            e.type = (int)TileType.Wall;
            e.changed = true;
            map.array[i] = e;
        }

        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        {
            Entity tunneler = entityManager.Instantiate(tunnelerPrefab);
            entityManager.SetComponentData(tunneler,
                new GridPosition { Value = new int2(map.size.x / 2, map.size.y / 2) }
            );
            var tunnelerData = entityManager.GetComponentData<Tunneler>(tunneler);
            tunnelerData.direction = (Direction)UnityEngine.Random.Range(0, 4);
            entityManager.SetComponentData(tunneler, tunnelerData);
        }
    }

    void LateUpdate()
    {
        for (int x = 0; x < map.size.x; x++)
            for (int y = 0; y < map.size.y; y++)
            {
                // if (map.GetTile(x, y).changed)
                // {
                //     var e = map.GetTile(x, y);
                //     e.changed = false;
                //     map.SetTile(x, y, e);

                //     tileRenderes[y + map.size.x * x].sprite =
                //         tilesData[(int)map.GetTileType(x, y)].sprite;
                // }
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
        public NativeArray<NativeTile> array;
        public NativeArray<NativeTileData> tilesData;

        public NativeTileData GetTileData(int x, int y)
        {
            return tilesData[(int)GetTileType(x, y)];
        }

        public NativeTileData GetTileData(int2 position)
        {
            return GetTileData(position.x, position.y);
        }

        public TileType GetTileType(int x, int y)
        {
            if (!InBounds(x, y)) return TileType.OutOfBounds;
            return (TileType)array[x + size.x * y].type;
        }

        public TileType GetTileType(int2 position)
        {
            return GetTileType(position.x, position.y);
        }

        public NativeTile GetTile(int x, int y)
        {
            if (!InBounds(x, y)) return new NativeTile();
            return array[x + size.x * y];
        }

        public NativeTile GetTile(int2 position)
        {
            return GetTile(position.x, position.y);
        }

        public void SetTile(int x, int y, NativeTile tile)
        {
            if (InBounds(x, y))
                array[x + size.x * y] = tile;
        }

        public void SetTile(int2 position, NativeTile tile)
        {
            SetTile(position.x, position.y, tile);
        }

        public void SetTileType(int x, int y, TileType type)
        {
            if (InBounds(x, y))
            {
                var e = array[x + size.x * y];
                e.type = (int)type;
                e.changed = true;
                array[x + size.x * y] = e;
            }
        }

        public void SetTileType(int2 position, TileType type)
        {
            SetTileType(position.x, position.y, type);
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
