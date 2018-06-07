using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Transforms2D;
using UnityEngine;

[UpdateAfter(typeof(HumanToZombieSystem))]
[UpdateAfter(typeof(HumanNavigationSystem))]
class ZombieTargetingSystem : JobComponentSystem
{
    [Inject] private ZombieTargetingData zombieTargetingData;
    [Inject] private HumanTargetingData humanTargetingData;


    public NativeList<Human> Humans;
    public NativeList<Position2D> HumanPositions;

    protected override void OnCreateManager(int capacity)
    {
        Humans = new NativeList<Human>(Allocator.Persistent);
        HumanPositions = new NativeList<Position2D>(Allocator.Persistent);
        base.OnCreateManager(capacity);
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        inputDeps.Complete();

        Humans.ResizeUninitialized(humanTargetingData.Length);
        HumanPositions.ResizeUninitialized(humanTargetingData.Length);

        for (int i = 0; i < humanTargetingData.Length; i++)
        {
            Humans[i] = humanTargetingData.Humans[i];
            HumanPositions[i] = humanTargetingData.Positions[i];
        }

        var copyJob = new CopyHumansToNativeListJob
        {
            HumanTargetingData = this.humanTargetingData,
            Humans = this.Humans,
            HumanPositions = this.HumanPositions
        };

        inputDeps = copyJob.Schedule(humanTargetingData.Length, 64, inputDeps);

        var job = new ZombieTargetingJob
        {
            zombieTargetingData = zombieTargetingData,
            Humans = Humans,
            HumanPositions = HumanPositions,
            dt = Time.deltaTime
        };

        return job.Schedule(zombieTargetingData.Length, 64, inputDeps);
    }
}

[ComputeJobOptimization]
public struct CopyHumansToNativeListJob : IJobParallelFor
{
    [ReadOnly]
    public HumanTargetingData HumanTargetingData;

    public NativeArray<Human> Humans;
    public NativeArray<Position2D> HumanPositions;

    public void Execute(int index)
    {
        Humans[index] = HumanTargetingData.Humans[index];
        HumanPositions[index] = HumanTargetingData.Positions[index];
    }
}

[ComputeJobOptimization]
public struct ZombieTargetingJob : IJobParallelFor
{
    public ZombieTargetingData zombieTargetingData;

    [ReadOnly]
    public NativeArray<Human> Humans;
    [ReadOnly]
    public NativeArray<Position2D> HumanPositions;

    public float dt;

    public void Execute(int index)
    {
        var zombie = zombieTargetingData.Zombie[index];
        if (zombie.BecomeActive != 1)
            return;

        if (zombie.HumanTargetIndex != -1)
        {
            var human = Humans[zombie.HumanTargetIndex];
            if (human.IsInfected != 1)
            {
                return;
            }
        }

        float2 zombiePosition = zombieTargetingData.Position[index].Value;
        bool foundOne = false;
        float nearestDistance = float.MaxValue;

        int humanTargetIndex = -1;

        for (int i = 0; i < Humans.Length; i++)
        {
            var human = Humans[i];
            if (human.IsInfected == 1)
                continue;

            foundOne = true;

            float2 humanPosition = HumanPositions[i].Value;
            float distSquared = math.distance(humanPosition, zombiePosition);

            if (distSquared < nearestDistance)
            {
                nearestDistance = distSquared;
                humanTargetIndex = i;
            }

            //if (distSquared < 5)
            //    break;
        }

        if (foundOne)
        {
            zombie.HumanTargetIndex = humanTargetIndex;
            zombieTargetingData.Zombie[index] = zombie;
        }
        else
        {
            zombie.HumanTargetIndex = -1;
            zombieTargetingData.Zombie[index] = zombie;
        }
    }
}

public struct ZombieTargetingData
{
    public int Length;
    [ReadOnly] public ComponentDataArray<Position2D> Position;
    public ComponentDataArray<Zombie> Zombie;
}

public struct HumanTargetingData
{
    public int Length;
    [ReadOnly] public ComponentDataArray<Position2D> Positions;
    [ReadOnly] public ComponentDataArray<Human> Humans;
}