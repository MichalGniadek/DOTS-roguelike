using Unity.Entities;
using UnityEngine;

[System.Serializable]
public class EntityPrefab
{
    static GameObjectConversionSettings settings;

    public GameObject go;
    private Entity entity;

    public static implicit operator Entity(EntityPrefab prefab)
    {
        if (prefab.entity == Entity.Null)
        {
            if (settings == null)
            {
                var world = World.DefaultGameObjectInjectionWorld;
                settings = GameObjectConversionSettings.FromWorld(world, null);
            }

            prefab.entity = GameObjectConversionUtility
                .ConvertGameObjectHierarchy(prefab.go, settings);
        }
        return prefab.entity;
    }
}