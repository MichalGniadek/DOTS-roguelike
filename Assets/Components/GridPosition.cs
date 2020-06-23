using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct GridPosition : IComponentData
{
    public int2 Value;
}
