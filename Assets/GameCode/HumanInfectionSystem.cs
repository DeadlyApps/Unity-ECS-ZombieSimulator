using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms2D;
using UnityEngine;

class HumanInfectionSystem : JobComponentSystem
{
    [Inject] private HumanInfectionData humanData;
    [Inject] private ZombiePositionData zombieTargetData;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new HumanInfectionJob
        {
            zombieTargetData = zombieTargetData,
            humanData = humanData,
            infectionDistance = ZombieSettings.Instance.InfectionDistance
        };

        return job.Schedule(humanData.Length, 64, inputDeps);
    }
}


struct HumanInfectionJob : IJobParallelFor
{
    public HumanInfectionData humanData;

    [NativeDisableParallelForRestriction]
    public ZombiePositionData zombieTargetData;
    public float infectionDistance;

    public void Execute(int index)
    {
        float2 humanPosition = humanData.Positions[index].Value;

        for (int i = 0; i < zombieTargetData.Length; i++)
        {
            float2 zombiePosition = zombieTargetData.Positions[i].Value;

            float2 delta = zombiePosition - humanPosition;
            float distSquared = math.dot(delta, delta);

            if (distSquared < infectionDistance)
            {
                var human = humanData.Humans[index];
                human.IsInfected = 1;
                humanData.Humans[index] = human;
            }

        }
    }
}

public struct HumanInfectionData
{
    public int Length;
    [ReadOnly] public ComponentDataArray<Position2D> Positions;
    public ComponentDataArray<Human> Humans;
}

public struct ZombiePositionData
{
    public int Length;
    [ReadOnly] public ComponentDataArray<Position2D> Positions;
    [ReadOnly] public ComponentDataArray<Zombie> Zombies;
}