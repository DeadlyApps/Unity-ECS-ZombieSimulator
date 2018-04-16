using Unity.Entities;
using Unity.Transforms2D;

class HumanNavigationSystem : ComponentSystem
{
    public struct HumanData
    {
        public int Length;
        public ComponentDataArray<Position2D> Position;
        public ComponentDataArray<Heading2D> Heading;
        public ComponentDataArray<Human> Human;
    }

    [Inject] private HumanData humanDatum;

    protected override void OnUpdate()
    {
        for (int index = 0; index < humanDatum.Length; ++index)
        {
            Heading2D heading2D = humanDatum.Heading[index];
            heading2D.Value = new Unity.Mathematics.float2(1f, 0f);
            humanDatum.Heading[index] = heading2D;
        }
    }
}
