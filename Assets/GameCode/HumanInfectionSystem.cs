using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms2D;
using UnityEngine;

class HumanToZombieSystem : ComponentSystem
{
    struct HumansToConvertData
    {
        public int Length;
        public ComponentDataArray<Human> Humans;
        public EntityArray Entities;
    }

    [Inject] private HumansToConvertData humanData;

    protected override void OnUpdate()
    {
        for (int i = 0; i < humanData.Length; i++)
        {
            Human human = humanData.Humans[i];
            if (human.IsInfected == 1)
            {
                PostUpdateCommands.DestroyEntity(humanData.Entities[i]);
            }
        }
    }
}

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
                var human = humanData.Humans[i];
                human.IsInfected = 1;
                humanData.Humans[i] = human;

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