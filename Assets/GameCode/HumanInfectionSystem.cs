using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms2D;
using UnityEngine;

class HumanInfectionSystem : JobComponentSystem
{
    [Inject] private HumanInfectionData humanData;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new HumanInfectionJob
        {
            humanData = humanData,
            infectionDistance = ZombieSettings.Instance.InfectionDistance
        };

        return job.Schedule(inputDeps);
    }
}


struct HumanInfectionJob : IJob
{
    [NativeDisableParallelForRestriction]
    public HumanInfectionData humanData;

    public float infectionDistance;

    public void Execute()
    {
        for (int humanIndex = 0; humanIndex < humanData.Length; humanIndex++)
        {
            if (humanData.Humans[humanIndex].IsZombie == 1)
                continue;

            float2 humanPosition = humanData.Positions[humanIndex].Value;

            for (int zombieIndex = 0; zombieIndex < humanData.Length; zombieIndex++)
            {
                float2 zombiePosition = humanData.Positions[zombieIndex].Value;
                if (humanData.Humans[zombieIndex].IsZombie != 1)
                    continue;

                float2 delta = zombiePosition - humanPosition;
                float distSquared = math.dot(delta, delta);
                
                if (distSquared < infectionDistance)
                {
                    var human = humanData.Humans[humanIndex];
                    human.IsInfected = 1;
                    humanData.Humans[humanIndex] = human;
                }

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