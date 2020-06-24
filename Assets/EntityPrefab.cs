using Unity.Entities;
using UnityEngine;

[System.Serializable]
public class EntityPrefab
{
    static GameObjectConversionSettings settings;

    public GameObject go;
    private Entity entity;
    public Entity Entity
    {
        get
        {
            if (entity == Entity.Null) Convert();
            return entity;
        }
    }

    public static implicit operator Entity(EntityPrefab prefab)
    {
        if (prefab.entity == Entity.Null) prefab.Convert();
        return prefab.entity;
    }

    void Convert()
    {
        if (settings == null)
        {
            var world = World.DefaultGameObjectInjectionWorld;
            settings = GameObjectConversionSettings.FromWorld(world, null);
        }

        entity = GameObjectConversionUtility
            .ConvertGameObjectHierarchy(go, settings);
    }
}