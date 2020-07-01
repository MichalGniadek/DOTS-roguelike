using Unity.Entities;
using Unity.Mathematics;

public class TunnelerSystem : SystemBase
{
    int turnsInOneTick = 1;

    protected override void OnUpdate()
    {
        var map = GameManager.instance.map;
        Entity futureTunnelTile = GameManager.instance.futureTunnelTile;
        Entity tunnelerPrefab = GameManager.instance.tunnelerPrefab;
        Entity roomerPrefab = GameManager.instance.roomerPrefab;
        Random random = new Random((uint)(1 + Time.ElapsedTime * 10000));

        int turnsInOneTickLocal = turnsInOneTick;

        Entities.WithStructuralChanges().ForEach(
        (Entity entity, ref GridPosition position, ref Tunneler tunneler) =>
        {
            if (tunneler.lifeExpectancy > 0)
            {
                tunneler.lifeExpectancy -= turnsInOneTickLocal;
                for (int iteration = 0; iteration < turnsInOneTickLocal; iteration++)
                {
                    // Move tunneler
                    int2 targetPosition = position.Value + tunneler.direction.ToInt2();
                    while (!map.InBounds(targetPosition))
                    {
                        if (random.NextBool()) tunneler.direction.Rotate(1);
                        else tunneler.direction.Rotate(3);

                        targetPosition = position.Value + tunneler.direction.ToInt2();
                    }
                    position.Value = targetPosition;

                    // Dig
                    int2 digDirection = tunneler.direction.GetRotated().ToInt2();
                    for (int i = -tunneler.tunnelSize / 2;
                             i <= tunneler.tunnelSize / 2;
                             i++)
                    {
                        Entity tileEntity =
                            map.GetTileEntity(position.Value + i * digDirection);

                        if (tileEntity != Entity.Null)
                        {
                            EntityManager.AddComponent<TileChanger>(tileEntity);
                            EntityManager.SetComponentData(tileEntity,
                            new TileChanger
                            {
                                newTilePrefab = futureTunnelTile
                            });
                        }
                    }

                    // Tunnels
                    if (random.NextFloat() < tunneler.spawnTunnelerProbability)
                    {
                        var newTunneler = EntityManager.Instantiate(tunnelerPrefab);
                        EntityManager.SetComponentData(newTunneler, position);
                        var newTunnelerData =
                            EntityManager.GetComponentData<Tunneler>(newTunneler);

                        if (random.NextBool())
                            newTunnelerData.direction =
                                tunneler.direction.GetRotated(1);
                        else
                            newTunnelerData.direction =
                                tunneler.direction.GetRotated(3);

                        newTunnelerData.lifeExpectancy = tunneler.lifeExpectancy;

                        EntityManager.SetComponentData(newTunneler, newTunnelerData);
                    }

                    // Rooms
                    if (random.NextFloat() < tunneler.spawnRoomerProbability)
                    {
                        var newRoomer = EntityManager.Instantiate(roomerPrefab);

                        Direction spawnDirection = tunneler.direction.GetRotated(1);

                        EntityManager.SetComponentData(newRoomer, new GridPosition
                        {
                            Value = position.Value + spawnDirection.ToInt2() *
                                (1 + (tunneler.tunnelSize / 2))
                        });
                        EntityManager.SetComponentData(newRoomer, new Roomer
                        {
                            direction = tunneler.direction.GetRotated(),
                            roomSize = random.NextInt(3, 6)
                        });
                    }
                    if (random.NextFloat() < tunneler.spawnRoomerProbability)
                    {
                        var newRoomer = EntityManager.Instantiate(roomerPrefab);

                        Direction spawnDirection = tunneler.direction.GetRotated(3);

                        EntityManager.SetComponentData(newRoomer, new GridPosition
                        {
                            Value = position.Value + spawnDirection.ToInt2() *
                                (1 + (tunneler.tunnelSize / 2))
                        });
                        EntityManager.SetComponentData(newRoomer, new Roomer
                        {
                            direction = tunneler.direction.GetRotated(),
                            roomSize = random.NextInt(3, 6)
                        });
                    }

                    // Random changes
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
                                    EntityManager.AddComponent<TileChanger>(tileEntity);
                                    EntityManager.SetComponentData(tileEntity,
                                    new TileChanger
                                    {
                                        newTilePrefab = futureTunnelTile
                                    });
                                }
                            }
                    }
                }
            }
            else
            {
                EntityManager.DestroyEntity(entity);
            }
        }).Run();
    }

    protected override void OnStopRunning()
    {
        Enabled = false;
        World.GetOrCreateSystem<RoomerSystem>().Enabled = true;
        // World.GetOrCreateSystem<SimulationSystemGroup>()
        // .AddSystemToUpdateList(
        //     World.GetOrCreateSystem<RoomerSystem>()
        // );
    }
}