using Unity.Entities;
using UnityEngine;

[GenerateAuthoringComponent]
public struct Roomer : IComponentData
{
    [HideInInspector] public Direction direction;

    public int roomSize;
}