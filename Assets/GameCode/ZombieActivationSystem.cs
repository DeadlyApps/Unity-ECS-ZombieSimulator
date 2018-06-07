using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Transforms2D;

class ZombieActivationSystem : JobComponentSystem
{
    [Inject] private ZombieActivationData zombieData;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new ZombieActivationJob
        {
            zombieData = zombieData,
            moveSpeed = ZombieSettings.Instance.HumanSpeed * 2
        };

        return job.Schedule(zombieData.Length, 64, inputDeps);
    }
}
[ComputeJobOptimization]
public struct ZombieActivationJob : IJobParallelFor
{
    public ZombieActivationData zombieData;
    public float moveSpeed;

    public void Execute(int index)
    {
        var zombie = zombieData.Zombies[index];
        if (zombie.BecomeActive == 1 && zombie.FinishedActivation == 0)
        {
            var position = zombieData.Positions[index];
            position.Value = zombie.BecomeZombiePosition;
            zombieData.Positions[index] = position;

            var moveSpeed = zombieData.MoveSpeeds[index];
            moveSpeed.speed = this.moveSpeed;
            zombieData.MoveSpeeds[index] = moveSpeed;

            zombie.FinishedActivation = 1;
            zombieData.Zombies[index] = zombie;
        }


    }
}

public struct ZombieActivationData
{
    public int Length;
    public ComponentDataArray<Position2D> Positions;
    public ComponentDataArray<Zombie> Zombies;
    public ComponentDataArray<MoveSpeed> MoveSpeeds;
}