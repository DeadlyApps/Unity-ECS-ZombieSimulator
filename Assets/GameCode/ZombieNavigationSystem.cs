using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Transforms2D;
using UnityEngine;

class ZombieNavigationSystem : JobComponentSystem
{
    [Inject] private ZombieData zombieDatum;
    [Inject] private ZombieTargetData zombieTargetDatume;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new ZombieNavigationJob
        {
            zombieDatum = zombieDatum,
            zombieTargetDatum = zombieTargetDatume,
            dt = Time.deltaTime
        };

        return job.Schedule(zombieDatum.Length, 64, inputDeps);
    }
}

public struct ZombieNavigationJob : IJobParallelFor
{
    public ZombieData zombieDatum;

    [NativeDisableParallelForRestriction]
    public ZombieTargetData zombieTargetDatum;

    public float dt;

    public void Execute(int index)
    {
        var zombie = zombieDatum.Zombie[index];
        float2 zombiePosition = zombieDatum.Position[index].Value;
        int selectedHumanIndex;
        float2 newHeading = new float2(0, 1);

        float nearestDistance = float.MaxValue; 
        for (int i = 0; i < zombieTargetDatum.Length; i++)
        {
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

public struct ZombieData
{
    public int Length;
    [ReadOnly] public ComponentDataArray<Position2D> Position;
    public ComponentDataArray<Heading2D> Heading;
    [ReadOnly] public ComponentDataArray<Zombie> Zombie;
}

public struct ZombieTargetData
{
    public int Length;
    [ReadOnly] public ComponentDataArray<Position2D> Positions;
    public ComponentDataArray<Human> Humans;
}