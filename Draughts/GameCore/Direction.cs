using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameCore
{
    public class Direction
    {
        public static readonly Direction UP_LEFT = new Direction(-1, -1);
        public static readonly Direction UP_RIGHT = new Direction(-1, 1);
        public static readonly Direction DOWN_LEFT = new Direction(1, -1);
        public static readonly Direction DOWN_RIGHT = new Direction(1, 1);

        public int Y { get; private set; }
        public int X { get; private set; }

        public Direction(int y, int x)
        {
            this.Y = y;
            this.X = x;
        }

        public bool isUp()
        {
            return Y < 0;
        }

        public bool isDown()
        {
            return Y > 0;
        }

        public static Direction getDirection(Position before, Position after)
        {
            int v = (after.Row < before.Row) ? -1 : 1;
            int h = (after.Col < before.Col) ? -1 : 1;

            return new Direction(v, h);
        }

        public static Direction[] getAll()
        {
            return new Direction[] { UP_LEFT, UP_RIGHT, DOWN_LEFT, DOWN_RIGHT };
        }
    }
}
