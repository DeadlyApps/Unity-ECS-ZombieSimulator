using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Transforms2D;
using UnityEngine;

[UpdateAfter(typeof(HumanToZombieSystem))]
class ZombieNavigationSystem : JobComponentSystem
{
    [Inject] private ZombieData zombieDatum;
    [Inject] private ZombieTargetData zombieTargetDatume;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new ZombieNavigationJob
        {
            zombieTargetingData = zombieDatum,
            humanTargetingData = zombieTargetDatume,
            dt = Time.deltaTime
        };

        return job.Schedule(zombieDatum.Length, 64, inputDeps);
    }
}
[ComputeJobOptimization]
public struct ZombieNavigationJob : IJobParallelFor
{
    public ZombieData zombieTargetingData;

    [NativeDisableParallelForRestriction]
    public ZombieTargetData humanTargetingData;

    public float dt;

    public void Execute(int index)
    {
        var zombie = zombieTargetingData.Zombie[index];
        if (zombie.BecomeActive != 1)
            return;
        if (zombie.HumanTargetIndex == -1)
        {
            var moveSpeed = zombieTargetingData.MoveSpeeds[index];
            moveSpeed.speed = 0;
            zombieTargetingData.MoveSpeeds[index] = moveSpeed;
            return;
        }

        float2 zombiePosition = zombieTargetingData.Position[index].Value;
        float2 newHeading = new float2(0, 1);
        float2 humanPosition = humanTargetingData.Positions[zombie.HumanTargetIndex].Value;
        float2 delta = humanPosition - zombiePosition;

        newHeading = math.normalize(delta);

        var heading = zombieTargetingData.Heading[index];
        heading.Value = newHeading;
        zombieTargetingData.Heading[index] = heading;

    }
}

public struct ZombieData
{
    public int Length;
    [ReadOnly] public ComponentDataArray<Position2D> Position;
    public ComponentDataArray<Heading2D> Heading;
    public ComponentDataArray<MoveSpeed> MoveSpeeds;
    [ReadOnly] public ComponentDataArray<Zombie> Zombie;
}

public struct ZombieTargetData
{
    public int Length;
    [ReadOnly] public ComponentDataArray<Position2D> Positions;
    [ReadOnly] public ComponentDataArray<Human> Humans;
}