using Unity.Entities;
using Input = UnityEngine.Input;
using KeyCode = UnityEngine.KeyCode;
using Unity.Mathematics;

public class MapGenerationSystem : SystemBase
{
    int numberOfTurns = 1000;

    protected override void OnUpdate()
    {
        int speedup = 10;
        var ecb = ecbSystem.CreateCommandBuffer().ToConcurrent();
        if (numberOfTurns >= 0)
        {
            numberOfTurns -= speedup;
            var map = GameManager.instance.map;
            Entity floorPrefab = GameManager.instance.floorPrefab;
            Random random = new Random((uint)(1 + Time.ElapsedTime * 10000));

            Entities.ForEach(
            (int entityInQueryIndex, ref GridPosition position, ref Tunneler tunneler) =>
            {
                for (int z = 0; z < speedup; z++)
                {
                    position.Value += tunneler.direction.ToInt2();

                    if (!map.InBounds(position.Value))
                    {
                        position.Value -= tunneler.direction.ToInt2();
                        if (random.NextBool()) tunneler.direction.Rotate(1);
                        else tunneler.direction.Rotate(3);
                    }

                    int2 digDirection = tunneler.direction.GetRotated().ToInt2();
                    for (int i = -tunneler.tunnelSize / 2;
                            i <= tunneler.tunnelSize / 2; i++)
                    {
                        Entity tileEntity =
                            map.GetTileEntity(position.Value + i * digDirection);

                        if (tileEntity != Entity.Null)
                        {
                            ecb.AddComponent(entityInQueryIndex, tileEntity,
                            new TileChanger
                            {
                                newTilePrefab = floorPrefab
                            });
                        }
                    }

                    if (random.NextFloat() < tunneler.sizeChangeProbability)
                    {
                        if (random.NextBool()) tunneler.tunnelSize += 2;
                        else if (tunneler.tunnelSize > 2) tunneler.tunnelSize -= 2;
                    }

                    if (random.NextFloat() < tunneler.turnProbability)
                    {
                        if (random.NextBool()) tunneler.direction.Rotate(1);
                        else tunneler.direction.Rotate(3);

                        for (int x = -tunneler.tunnelSize / 2;
                                x <= tunneler.tunnelSize / 2; x++)
                            for (int y = -tunneler.tunnelSize / 2;
                                    y <= tunneler.tunnelSize / 2; y++)
                            {
                                Entity tileEntity =
                                    map.GetTileEntity(position.Value + new int2(x, y));

                                if (tileEntity != Entity.Null)
                                {
                                    ecb.AddComponent(entityInQueryIndex, tileEntity,
                                    new TileChanger
                                    {
                                        newTilePrefab = floorPrefab
                                    });
                                }
                            }
                    }
                }
            }).ScheduleParallel();
        }
        else
        {
            Entities.WithAll<Tunneler>().ForEach(
            (Entity entity, int entityInQueryIndex) =>
            {
                ecb.DestroyEntity(entityInQueryIndex, entity);
            }).ScheduleParallel();
        }
        ecbSystem.AddJobHandleForProducer(Dependency);
    }

    private EndSimulationEntityCommandBufferSystem ecbSystem;

    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }
}