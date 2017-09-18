using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameCore
{
    public class Position
    {
        public int Col { get; set; }
        public int Row { get; set; }

        public Position()
        {
            Row = 0;
            Col = 0;
        }

        public Position(int row, int col)
        {
            this.Row = row;
            this.Col = col;
        }

        public Position(Position pos)
        {
            this.Row = pos.Row;
            this.Col = pos.Col;
        }

        public void set(int row, int col)
        {
            this.Row = row;
            this.Col = col;
        }

        public void move(Direction direction)
        {
            Row += direction.Y;
            Col += direction.X;
        }

        public bool isInRange(int from, int to)
        {
            return (Row >= from && Row < to && Col >= from && Col < to);
        }

        public int getDistance(Position other)
        {
            int x = other.Col - this.Col;
            int y = other.Row - this.Row;
            return (int)Math.Sqrt(x * x + y * y);
        }

        public bool isOnList(List<Position> positions)
        {
            foreach (Position p in positions)
                if (this.Equals(p))
                    return true;

            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is Position)
            {
                Position other = (Position)obj;
                return (this.Row == other.Row && this.Col == other.Col);
            }

            return false;
        }

        public override string ToString()
        {
            return Row + " " + Col;
        }
    }
}
