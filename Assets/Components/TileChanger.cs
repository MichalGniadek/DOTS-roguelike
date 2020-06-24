using Unity.Entities;

[GenerateAuthoringComponent]
public struct TileChanger : IComponentData
{
    public Entity newTilePrefab;
}