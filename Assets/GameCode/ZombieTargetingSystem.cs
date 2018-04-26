using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Transforms2D;
using UnityEngine;

[UpdateAfter(typeof(HumanToZombieSystem))]
class ZombieTargetingSystem : JobComponentSystem
{
    [Inject] private ZombieTargetingData zombieTargetingData;
    [Inject] private HumanTargetingData humanTargetingData;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new ZombieTargetingJob
        {
            zombieTargetingData = zombieTargetingData,
            humanTargetingData = humanTargetingData,
            dt = Time.deltaTime
        };

        return job.Schedule(zombieTargetingData.Length, 64, inputDeps);
    }
}

public struct ZombieTargetingJob : IJobParallelFor
{
    public ZombieTargetingData zombieTargetingData;

    [NativeDisableParallelForRestriction]
    public HumanTargetingData humanTargetingData;

    public float dt;

    public void Execute(int index)
    {
        var zombie = zombieTargetingData.Zombie[index];
        if (zombie.BecomeActive != 1)
            return;

        if (zombie.HumanTargetIndex != -1)
        {
            var human = humanTargetingData.Humans[zombie.HumanTargetIndex];
            if(human.IsInfected != 1)
            {
                return;
            }
        }

        float2 zombiePosition = zombieTargetingData.Position[index].Value;
        bool foundOne = false;
        float nearestDistance = float.MaxValue;

        int humanTargetIndex = -1;

        for (int i = 0; i < humanTargetingData.Length; i++)
        {
            var human = humanTargetingData.Humans[i];
            if (human.IsInfected == 1)
                continue;

            foundOne = true;

            float2 humanPosition = humanTargetingData.Positions[i].Value;
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