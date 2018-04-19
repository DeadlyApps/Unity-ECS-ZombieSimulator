using Unity.Entities;
using Unity.Mathematics;

public struct Human : IComponentData
{
    public float TimeTillNextDirectionChange;
    public int IsInfected;
}