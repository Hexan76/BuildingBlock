namespace Framework.BuildingBlock.Entities.ValueObjects;

public abstract class ValueObject
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
        {
            return false;
        }

        var other = (ValueObject)obj;

        using var thisValues = GetEqualityComponents().GetEnumerator();
        using var otherValues = other.GetEqualityComponents().GetEnumerator();

        while (true)
        {
            var hasNext = thisValues.MoveNext();
            var otherHasNext = otherValues.MoveNext();

            if (!hasNext && !otherHasNext)
            {
                return true;
            }

            if (hasNext != otherHasNext)
            {
                return false;
            }

            if (!Equals(thisValues.Current, otherValues.Current))
            {
                return false;
            }
        }
    }

    public override int GetHashCode()
    {
        HashCode hash = new();

        foreach (var component in GetEqualityComponents())
        {
            hash.Add(component);
        }

        return hash.ToHashCode();
    }

    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !Equals(left, right);
    }
}
