using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class PlayerInputSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Direction direction = (Direction)(-1);
        if (Input.GetKeyDown(KeyCode.UpArrow)) direction = Direction.Up;
        else if (Input.GetKeyDown(KeyCode.DownArrow)) direction = Direction.Down;
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) direction = Direction.Left;
        else if (Input.GetKeyDown(KeyCode.RightArrow)) direction = Direction.Right;

        var map = GameManager.instance.map;

        if ((int)direction != (-1))
        {
            Entities.WithAll<PlayerInput>().ForEach(
                (Entity entity, ref GridPosition position) =>
                {
                    int2 targetPosition = position.Value + direction.ToInt2();

                    if (map.InBounds(targetPosition))
                    {
                        Tile targetTile =
                            GetComponent<Tile>(map.GetTileEntity(targetPosition));
                        if (!targetTile.isMovementBlocked)
                        {
                            position.Value = targetPosition;
                        }
                    }
                })
            .ScheduleParallel();
        }
    }
}