using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Transforms2D;
using UnityEngine;


public class ZombieSimulatorBootstrap {
    private static ZombieSettings Settings;

    public static MeshInstanceRenderer HumanLook;

    public static EntityArchetype HumanArchetype { get; private set; }

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

        NewGame();
    }

    private static void NewGame()
    {
        var entityManager = World.Active.GetOrCreateManager<EntityManager>();

        for (int i = 0; i < Settings.HumanCount; i++)
        {
            CreateHuman(entityManager);
        }
    }

    private static void CreateHuman(EntityManager entityManager)
    {
        Entity firstHuman = entityManager.CreateEntity(HumanArchetype);

        var randomSpawnLocation = ComputeSpawnLocation();

        // We can tweak a few components to make more sense like this.
        entityManager.SetComponentData(firstHuman, new Position2D { Value = randomSpawnLocation });
        entityManager.SetComponentData(firstHuman, new Heading2D { Value = new float2(0.0f, 1.0f) });
        entityManager.SetComponentData(firstHuman, new MoveSpeed { speed = Settings.HumanSpeed });

        // Finally we add a shared component which dictates the rendered look
        entityManager.AddSharedComponentData(firstHuman, HumanLook);
    }

    private static float2 ComputeSpawnLocation()
    {
        var settings = ZombieSettings.Instance;

        float r = Random.value;
        float x0 = settings.Playfield.xMin;
        float x1 = settings.Playfield.xMax;
        float x = x0 + (x1 - x0) * r;

        float r2 = Random.value;
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

    }

    private static MeshInstanceRenderer GetLookFromPrototype(string protoName)
    {
        var proto = GameObject.Find(protoName);
        var result = proto.GetComponent<MeshInstanceRendererComponent>().Value;
        Object.Destroy(proto);
        return result;
    }

}
