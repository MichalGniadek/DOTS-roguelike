using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class PlayerInputSystem : SystemBase
{
    enum Direction { NONE, UP, DOWN, LEFT, RIGHT };

    protected override void OnUpdate()
    {
        Direction direction = Direction.NONE;
        if (Input.GetKeyDown(KeyCode.UpArrow)) direction = Direction.UP;
        else if (Input.GetKeyDown(KeyCode.DownArrow)) direction = Direction.DOWN;
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) direction = Direction.LEFT;
        else if (Input.GetKeyDown(KeyCode.RightArrow)) direction = Direction.RIGHT;

        var map = GameManager.instance.map;

        if (direction != Direction.NONE)
        {
            Entities.WithAll<PlayerInput>().ForEach(
                (Entity entity, ref GridPosition position) =>
                {
                    int2 targetPosition = position.Value;

                    if (direction == Direction.UP) targetPosition.y++;
                    else if (direction == Direction.DOWN) targetPosition.y--;
                    else if (direction == Direction.LEFT) targetPosition.x--;
                    else if (direction == Direction.RIGHT) targetPosition.x++;

                    Entity targetTileEntity = map.GetTileEntity(targetPosition);
                    Tile targetTile = GetComponent<Tile>(targetTileEntity);
                    if (!targetTile.isMovementBlocked)
                    {
                        position.Value = targetPosition;
                    }
                })
            .ScheduleParallel();
        }
    }
}