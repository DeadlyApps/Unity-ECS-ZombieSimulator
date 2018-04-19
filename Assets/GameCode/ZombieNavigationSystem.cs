using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms2D;
using UnityEngine;

class ZombieNavigationSystem : JobComponentSystem
{
    [Inject] private ZombieData zombieDatum;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new ZombieNavigationJob
        {
            zombieDatum = zombieDatum,
            //zombieTargetDatum = zombieDatum,
            dt = Time.deltaTime
        };

        return job.Schedule(inputDeps);
    }
}

public struct ZombieNavigationJob : IJob
{

    [NativeDisableParallelForRestriction]
    public ZombieData zombieDatum;

    //[NativeDisableParallelForRestriction]
    //public ZombieData zombieTargetDatum;

    public float dt;

    public void Execute()
    {
        for (int index = 0; index < zombieDatum.Length; index++)
        {
            var zombie = zombieDatum.Humans[index];
            if (zombie.IsZombie != 1) return;

            float2 zombiePosition = zombieDatum.Positions[index].Value;
            int selectedHumanIndex;
            float2 newHeading = new float2(0, 1);
            var zombieTargetDatum = zombieDatum;
            float nearestDistance = float.MaxValue;
            for (int i = 0; i < zombieTargetDatum.Length; i++)
            {
                if (zombieTargetDatum.Humans[i].IsInfected == 1 ||
                    zombieTargetDatum.Humans[i].IsZombie == 1)
                    continue;

                float2 humanPosition = zombieTargetDatum.Positions[i].Value;
                float2 delta = humanPosition - zombiePosition;
                float distSquared = math.dot(delta, delta);

                if (distSquared < nearestDistance)
                {
                    nearestDistance = distSquared;
                    selectedHumanIndex = i;
                    newHeading = math.normalize(delta);
                }
            }


            var heading = zombieDatum.Heading[index];
            heading.Value = newHeading;
            zombieDatum.Heading[index] = heading;
        }
    }
}

public struct ZombieData
{
    public int Length;
    [ReadOnly] public ComponentDataArray<Position2D> Positions;
    public ComponentDataArray<Heading2D> Heading;
    [ReadOnly] public ComponentDataArray<Human> Humans;
}

public struct ZombieTargetData
{
    public int Length;
    [ReadOnly] public ComponentDataArray<Position2D> Positions;
    public ComponentDataArray<Human> Humans;
}