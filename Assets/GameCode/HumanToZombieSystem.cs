using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Transforms2D;

class HumanToZombieSystem : JobComponentSystem
{
    [Inject] private HumanConversionData humanData;
    [Inject] private ZombieConversionData zombieData;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new HumanToZombieJob
        {
            humanData = humanData,
            zombieData = zombieData
        };

        return job.Schedule(humanData.Length, 64, inputDeps);
    }
}

public struct HumanToZombieJob : IJobParallelFor
{
    public HumanConversionData humanData;
    public ZombieConversionData zombieData;

    public void Execute(int index)
    {
        var human = humanData.Humans[index];
        if (human.IsInfected == 1 && human.WasConverted == 0)
        {
            var position = humanData.Positions[index];
            var humanOriginalPosition = position.Value;
            position.Value = EntityUtil.GetOffScreenLocation();
            humanData.Positions[index] = position;

            var zombie = zombieData.Zombies[index];
            zombie.BecomeActive = 1;
            zombie.BecomeZombiePosition = humanOriginalPosition;
            zombieData.Zombies[index] = zombie;

            var moveSpeed = humanData.MoveSpeeds[index];
            moveSpeed.speed = 0;
            humanData.MoveSpeeds[index] = moveSpeed;

            human.WasConverted = 1;
            humanData.Humans[index] = human;
        }
    }
}

public struct HumanConversionData
{
    public int Length;
    public ComponentDataArray<Position2D> Positions;
    public ComponentDataArray<Human> Humans;
    public ComponentDataArray<MoveSpeed> MoveSpeeds;
}

public struct ZombieConversionData
{
    public int Length;
    //[ReadOnly] public ComponentDataArray<Position2D> Positions;
    public ComponentDataArray<Zombie> Zombies;
}