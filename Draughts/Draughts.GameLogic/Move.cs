using System;

namespace Draughts.GameLogic;

public readonly struct Move : IEquatable<Move>
{
    public Coords Origin { get; }
    public Coords Destination { get; }

    public Move(Coords origin, Coords destination)
    {
        Origin = origin;
        Destination = destination;
    }
    
    public bool Equals(Move other)
        => Origin.Equals(other.Origin) && Destination.Equals(other.Destination);
    
    public override bool Equals(object obj)
        => obj is Move other && Equals(other);
    
    public override int GetHashCode()
        => HashCode.Combine(Origin, Destination);
    
    public static bool operator ==(Move a, Move b)
        => a.Equals(b);

    public static bool operator !=(Move a, Move b)
        => !a.Equals(b);
}
