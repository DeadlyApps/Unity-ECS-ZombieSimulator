using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Transforms2D;

struct HumansToConvertData
{
    public int Length;
    public ComponentDataArray<Human> Humans;
    [ReadOnly] public ComponentDataArray<Position2D> Positions;
    public EntityArray Entities;
}
class HumanToZombieSystem : JobComponentSystem
{

    [Inject] private HumansToConvertData humanData;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new HumanToZombieJob()
        {
            entityManager  = this.EntityManager,
            humanData = humanData
        };

        return job.Schedule(inputDeps);
    }
}

struct HumanToZombieJob : IJob
{

    public EntityManager entityManager;
    public HumansToConvertData humanData;
    public void Execute()
    {
        for (int i = 0; i < humanData.Length; i++)
        {
            Human human = humanData.Humans[i];


            if (human.IsInfected == 1 && human.IsZombie != 1)
            {
                //PostUpdateCommands.DestroyEntity(humanData.Entities[i]);

                //PostUpdateCommands.CreateEntity(ZombieSimulatorBootstrap.ZombieArchetype);
                //PostUpdateCommands.SetComponent(new Position2D { Value = humanData.Positions[i].Value });
                //PostUpdateCommands.SetComponent(new Heading2D { Value = new float2(1.0f, 0.0f) });
                //PostUpdateCommands.SetComponent(new MoveSpeed { speed = ZombieSettings.Instance.HumanSpeed });
                human.IsZombie = 1;
                humanData.Humans[i] = human;
                // Finally we add a shared component which dictates the rendered look
                entityManager.RemoveComponent<MeshInstanceRenderer>(humanData.Entities[i]);
                entityManager.AddSharedComponentData(humanData.Entities[i], ZombieSimulatorBootstrap.ZombieLook);

                //PostUpdateCommands.RemoveComponent<Human>(humanData.Entities[i]);
                //PostUpdateCommands.SetComponent(humanData.Entities[i], default(Zombie));
            }
        }

    }
}
