using UnityEngine;

internal class ZombieSettings : MonoBehaviour
{
    public float HumanSpeed = 10;
    public int HumanCount = 1000;


    public Rect Playfield = new Rect { x = -30.0f, y = -30.0f, width = 60.0f, height = 60.0f };


    public static ZombieSettings Instance { get; private set; }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
    }


}