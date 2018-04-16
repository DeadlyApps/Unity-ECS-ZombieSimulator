using Unity.Entities;
using Unity.Transforms;
using Unity.Transforms2D;
using UnityEngine;


public class ZombieSimulatorBootstrap {

    public static EntityArchetype HumanArchetype { get; private set; }
    public static EntityArchetype ZombieArchetype { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        var entityManager = World.Active.GetOrCreateManager<EntityManager>();

        DefineArchetypes(entityManager);

    }

    private static void DefineArchetypes(EntityManager entityManager)
    {
        //Humans - Composition of the humans

        HumanArchetype = entityManager.CreateArchetype(typeof(Position2D), typeof(TransformMatrix));

        //Zombies

        ZombieArchetype = entityManager.CreateArchetype(typeof(Position2D), typeof(TransformMatrix));
    }
}
