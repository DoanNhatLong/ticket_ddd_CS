namespace TrainBooking.Domain.Abstractions;

/// <summary>
/// Base class for all domain entities. Identity-based equality.
/// </summary>
public abstract class Entity
{
    protected Entity(int id)
    {
        Id = id;
    }

    protected Entity() { }

    public int Id { get; protected init; }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        // Transient entities (not yet persisted) are never equal.
        if (Id == 0 || other.Id == 0)
            return false;

        return Id == other.Id;
    }

    public static bool operator ==(Entity? left, Entity? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(Entity? left, Entity? right) => !(left == right);

    public override int GetHashCode() => Id.GetHashCode() * 41;
}
