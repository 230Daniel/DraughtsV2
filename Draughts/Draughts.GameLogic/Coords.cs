using System;

namespace Draughts.GameLogic;

public readonly struct Coords : IEquatable<Coords>
{
    public int X { get; }
    public int Y { get; }

    public Coords(int x, int y)
    {
        X = x;
        Y = y;
    }

    public bool Equals(Coords other)
        => X == other.X && Y == other.Y;

    public override bool Equals(object obj)
        => obj is Coords other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(X, Y);

    public static bool operator ==(Coords a, Coords b)
        => a.Equals(b);

    public static bool operator !=(Coords a, Coords b)
        => !a.Equals(b);
}
