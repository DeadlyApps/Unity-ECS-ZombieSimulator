using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Transforms2D;

class HumanToZombieSystem : ComponentSystem
{
    struct HumansToConvertData
    {
        public int Length;
        public ComponentDataArray<Human> Humans;
        [ReadOnly] public ComponentDataArray<Position2D> Positions;
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

                PostUpdateCommands.CreateEntity(ZombieSimulatorBootstrap.ZombieArchetype);
                PostUpdateCommands.SetComponent(new Position2D { Value = humanData.Positions[i].Value });
                PostUpdateCommands.SetComponent(new Heading2D { Value = new float2(1.0f, 0.0f) });
                PostUpdateCommands.SetComponent(new MoveSpeed { speed = ZombieSettings.Instance.HumanSpeed });

                // Finally we add a shared component which dictates the rendered look
                //PostUpdateCommands.RemoveComponent<MeshInstanceRenderer>(humanData.Entities[i]);
                PostUpdateCommands.AddSharedComponent(ZombieSimulatorBootstrap.ZombieLook);

                //PostUpdateCommands.RemoveComponent<Human>(humanData.Entities[i]);
                //PostUpdateCommands.SetComponent(humanData.Entities[i], default(Zombie));
            }
        }
    }
}
