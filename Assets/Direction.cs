using Unity.Mathematics;

public enum Direction : int { Up = 0, Right = 1, Down = 2, Left = 3 }

public static class DirectionExtensions
{
    public static void Rotate(this ref Direction direction, int times)
    {
        int i = (int)direction;
        i += times;
        direction = (Direction)(i % 4);
    }

    public static Direction GetRotated(this Direction direction, int times = 1)
    {
        int i = (int)direction;
        i += times;
        return (Direction)(i % 4);
    }

    public static int2 ToInt2(this Direction direction)
    {
        switch (direction)
        {
            case Direction.Up: return new int2(0, 1);
            case Direction.Right: return new int2(1, 0);
            case Direction.Down: return new int2(0, -1);
            case Direction.Left: return new int2(-1, 0);
            default: return new int2(0, 0);
        }
    }
}