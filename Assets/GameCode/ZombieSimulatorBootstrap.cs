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
        Entity firstHuman = entityManager.CreateEntity(HumanArchetype);

        // We can tweak a few components to make more sense like this.
        entityManager.SetComponentData(firstHuman, new Position2D { Value = new float2(0.0f, 0.0f) });
        entityManager.SetComponentData(firstHuman, new Heading2D { Value = new float2(0.0f, 1.0f) });
        entityManager.SetComponentData(firstHuman, new MoveSpeed { speed = Settings.HumanSpeed });

        // Finally we add a shared component which dictates the rendered look
        entityManager.AddSharedComponentData(firstHuman, HumanLook);

    }

    private static void DefineArchetypes(EntityManager entityManager)
    {
        //Humans - Composition of the humans

        HumanArchetype = entityManager.CreateArchetype(typeof(MoveSpeed), typeof(Heading2D), typeof(Position2D), typeof(TransformMatrix));

    }

    private static MeshInstanceRenderer GetLookFromPrototype(string protoName)
    {
        var proto = GameObject.Find(protoName);
        var result = proto.GetComponent<MeshInstanceRendererComponent>().Value;
        Object.Destroy(proto);
        return result;
    }

}
