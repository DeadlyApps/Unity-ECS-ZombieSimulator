using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Transforms2D;
using UnityEngine;

[UpdateAfter(typeof(HumanToZombieSystem))]
class HumanNavigationSystem : JobComponentSystem
{
    [Inject] private HumanData humanDatum;

    NativeArray<float> randomFloats = new NativeArray<float>(1000000, Allocator.Persistent);
    NativeArray<float> randomFloats2 = new NativeArray<float>(1000000, Allocator.Persistent);

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        for (int i = 0; i < humanDatum.Length; i++)
        {
            randomFloats[i] = Random.value;
            randomFloats2[i] = Random.value;
        }

        var humanNavigationJob = new HumanNavigationJob
        {
            humanDatum = humanDatum,
            dt = Time.deltaTime,
            randomFloats = randomFloats,
            randomFloats2 = randomFloats2
        };

        return humanNavigationJob.Schedule(humanDatum.Length, 64, inputDeps);
    }
}

struct HumanNavigationJob : IJobParallelFor
{
    public HumanData humanDatum;
    public float dt;
    public NativeArray<float> randomFloats;
    public NativeArray<float> randomFloats2;

    public void Execute(int index)
    {
        var human = humanDatum.Human[index];
        human.TimeTillNextDirectionChange -= dt;

        if (human.TimeTillNextDirectionChange <= 0)
        {
            human.TimeTillNextDirectionChange = 5;

            float randomX = (randomFloats[index] - 0.5f) * 2f;
            float randomY = (randomFloats2[index] - 0.5f) * 2f;

            Heading2D heading2D = humanDatum.Heading[index];
            heading2D.Value = new float2(randomX, randomY);
            humanDatum.Heading[index] = heading2D;

            //MoveSpeed moveSpeed = humanDatum.MoveSpeed[index];
            //moveSpeed.speed = index;
            //humanDatum.MoveSpeed[index] = moveSpeed;
        }

        humanDatum.Human[index] = human;
    }
}

public struct HumanData
{
    public int Length;
    public ComponentDataArray<Position2D> Positions;
    public ComponentDataArray<Heading2D> Heading;
    public ComponentDataArray<MoveSpeed> MoveSpeed;
    public ComponentDataArray<Human> Human;
}