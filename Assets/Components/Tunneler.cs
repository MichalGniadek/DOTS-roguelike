using Unity.Entities;
using UnityEngine;

[GenerateAuthoringComponent]
public struct Tunneler : IComponentData
{
    [HideInInspector] public Direction direction;

    public int lifeExpectancy;
    public int tunnelSize;
    [Range(0, 1)] public float sizeChangeProbability;
    [Range(0, 1)] public float turnProbability;

    [Range(0, 1)] public float spawnTunnelerProbability;
    [Range(0, 1)] public float spawnRoomerProbability;
}