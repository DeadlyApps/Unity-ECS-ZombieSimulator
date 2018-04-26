using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Transforms2D;
using UnityEngine;


public class ZombieSimulatorBootstrap
{
    private static ZombieSettings Settings;

    public static MeshInstanceRenderer HumanLook;
    public static MeshInstanceRenderer ZombieLook;

    public static EntityArchetype HumanArchetype { get; private set; }
    public static EntityArchetype ZombieArchetype { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        var entityManager = World.Active.GetOrCreateManager<EntityManager>();
        DefineArchetypes(entityManager);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void InitializeWithScene()
    {
        var settingsGO = GameObject.Find("ZombieSettings");
        Settings = settingsGO?.GetComponent<ZombieSettings>();
        if (!Settings)
            return;

        HumanLook = GetLookFromPrototype("HumanRenderPrototype");
        ZombieLook = GetLookFromPrototype("ZombieRenderPrototype");

        NewGame();
    }

    private static void NewGame()
    {
        var entityManager = World.Active.GetOrCreateManager<EntityManager>();
        CreateHumans(entityManager);
        CreateZombies(entityManager);
    }

    private static void CreateZombies(EntityManager entityManager)
    {
        NativeArray<Entity> zombies = new NativeArray<Entity>(Settings.HumanCount, Allocator.Persistent);
        entityManager.CreateEntity(ZombieArchetype, zombies);

        for (int i = 0; i < Settings.HumanCount; i++)
        {
            var randomSpawnLocation = EntityUtil.GetOffScreenLocation();

            // We can tweak a few components to make more sense like this.
            InitializeZombie(entityManager, zombies[i], randomSpawnLocation);
        }
    }


    public static void InitializeZombie(EntityManager entityManager, Entity zombie, float2 position)
    {
        entityManager.SetComponentData(zombie, new Position2D { Value = position });
        entityManager.SetComponentData(zombie, new Heading2D { Value = new float2(1.0f, 0.0f) });
        entityManager.SetComponentData(zombie, new MoveSpeed { speed = 0 });

        // Finally we add a shared component which dictates the rendered look
        entityManager.AddSharedComponentData(zombie, ZombieLook);
    }

    private static void CreateHumans(EntityManager entityManager)
    {
        int length = Settings.HumanCount;// + Settings.ZombieCount;
        NativeArray<Entity> humans = new NativeArray<Entity>(length, Allocator.Persistent);
        entityManager.CreateEntity(HumanArchetype, humans);

        for (int i = 0; i < length; i++)
        {
            var human = humans[i];
            var randomSpawnLocation = ComputeSpawnLocation();

            // We can tweak a few components to make more sense like this.
            entityManager.SetComponentData(human, new Position2D { Value = randomSpawnLocation });
            entityManager.SetComponentData(human, new Heading2D { Value = new float2(0.0f, 1.0f) });
            entityManager.SetComponentData(human, new MoveSpeed { speed = Settings.HumanSpeed });

            if (i < Settings.ZombieCount)
            {
                entityManager.SetComponentData(human, new Human { IsInfected = 1 });
            }

            // Finally we add a shared component which dictates the rendered look
            entityManager.AddSharedComponentData(human, HumanLook);
        }

    }

    private static float2 ComputeSpawnLocation()
    {
        var settings = ZombieSettings.Instance;

        float r = UnityEngine.Random.value;
        float x0 = settings.Playfield.xMin;
        float x1 = settings.Playfield.xMax;
        float x = x0 + (x1 - x0) * r;

        float r2 = UnityEngine.Random.value;
        float y0 = settings.Playfield.yMin;
        float y1 = settings.Playfield.yMax;
        float y = y0 + (y1 - y0) * r2;

        return new float2(x, y);
    }

    private static void DefineArchetypes(EntityManager entityManager)
    {
        //Humans - Composition of the humans

        HumanArchetype = entityManager.CreateArchetype(typeof(Human),
                                                        typeof(Heading2D),
                                                        typeof(MoveSpeed),
                                                        typeof(Position2D),
                                                        typeof(TransformMatrix));

        ZombieArchetype = entityManager.CreateArchetype(typeof(Zombie),
                                                        typeof(Heading2D),
                                                        typeof(MoveSpeed),
                                                        typeof(Position2D),
                                                        typeof(TransformMatrix));
    }

    private static MeshInstanceRenderer GetLookFromPrototype(string protoName)
    {
        var proto = GameObject.Find(protoName);
        var result = proto.GetComponent<MeshInstanceRendererComponent>().Value;
        UnityEngine.Object.Destroy(proto);
        return result;
    }

}
