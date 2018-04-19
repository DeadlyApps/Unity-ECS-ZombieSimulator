using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
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
            dt = Time.deltaTime
        };

        return job.Schedule(zombieDatum.Length, 64, inputDeps);
    }
}

public struct ZombieNavigationJob : IJobParallelFor
{
    public ZombieData zombieDatum;
    public float dt;

    public void Execute(int index)
    {
        var zombie = zombieDatum.Zombie[index];

        var heading = zombieDatum.Heading[index];
        heading.Value = new float2(0f, 1f);
        zombieDatum.Heading[index] = heading;
    }
}

public struct ZombieData
{
    public int Length;
    public ComponentDataArray<Position2D> Position;
    public ComponentDataArray<Heading2D> Heading;
    public ComponentDataArray<MoveSpeed> MoveSpeed;
    public ComponentDataArray<Zombie> Zombie;
}