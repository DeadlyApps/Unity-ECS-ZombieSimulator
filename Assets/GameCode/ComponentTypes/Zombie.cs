using Unity.Entities;
using Unity.Mathematics;

public struct Zombie : IComponentData
{
    public float2 BecomeZombiePosition;
    public int BecomeActive;
    public int FinishedActivation;
    public int HumanTargetIndex;
}