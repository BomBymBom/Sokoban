using System;

namespace Game.Core
{
    [Serializable]
    public class GridPosition : IEquatable<GridPosition>
    {
        public int X;
        public int Y;
        
        public GridPosition() { }
        
        public GridPosition(int x, int y)
        {
            X = x;
            Y = y;
        }
        
        public GridPosition Move(Direction direction)
        {
            return direction switch
            {
                Direction.Up => new GridPosition(X, Y + 1),
                Direction.Down => new GridPosition(X, Y - 1),
                Direction.Left => new GridPosition(X - 1, Y),
                Direction.Right => new GridPosition(X + 1, Y),
                _ => new GridPosition(X, Y)
            };
        }
        
        public bool Equals(GridPosition other)
        {
            if (other == null) return false;
            return X == other.X && Y == other.Y;
        }
        
        public override bool Equals(object obj) => obj is GridPosition other && Equals(other);
        
        public override int GetHashCode() => HashCode.Combine(X, Y);
        
        public override string ToString() => $"({X}, {Y})";
    }
}