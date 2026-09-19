using System.Collections;

namespace Expressif.Values;

/// <summary>Represents a value with ordered tuple positions.</summary>
public interface IPositionalValue
{
    int Arity { get; }
    object? GetPosition(int index);
}

internal static class PositionalValueEquality
{
    public static bool Equals(IPositionalValue left, object? right)
    {
        if (right is not IPositionalValue positional || left.Arity != positional.Arity)
            return false;
        for (var index = 0; index < left.Arity; index++)
        {
            if (!StructuralValueComparer.Instance.Equals(left.GetPosition(index), positional.GetPosition(index)))
                return false;
        }
        return true;
    }

    public static int GetHashCode(IPositionalValue value)
    {
        var hash = default(HashCode);
        for (var index = 0; index < value.Arity; index++)
        {
            var position = value.GetPosition(index);
            hash.Add(StructuralValueComparer.Instance.GetHashCode(position));
        }
        return hash.ToHashCode();
    }
}

internal sealed class StructuralValueComparer : IEqualityComparer<object?>
{
    public static StructuralValueComparer Instance { get; } = new();

    public new bool Equals(object? left, object? right)
    {
        if (ReferenceEquals(left, right))
            return true;
        if (left is null || right is null)
            return false;
        if (left is IPositionalValue || right is IPositionalValue)
        {
            return left is IPositionalValue leftPositional
                && right is IPositionalValue rightPositional
                && PositionalValueEquality.Equals(leftPositional, rightPositional);
        }
        if (left is RecordValue || right is RecordValue)
        {
            return left is RecordValue leftRecord
                && right is RecordValue rightRecord
                && RecordsEqual(leftRecord, rightRecord);
        }
        if (left is IEnumerable leftValues && right is IEnumerable rightValues
            && left is not string && right is not string)
            return EnumerablesEqual(leftValues, rightValues);
        return left.Equals(right);
    }

    public int GetHashCode(object? value)
    {
        if (value is null)
            return 0;
        if (value is IPositionalValue positional)
            return PositionalValueEquality.GetHashCode(positional);
        if (value is RecordValue record)
        {
            var hash = default(HashCode);
            foreach (var field in record)
            {
                hash.Add(field.Key, StringComparer.Ordinal);
                hash.Add(GetHashCode(field.Value));
            }
            return hash.ToHashCode();
        }
        if (value is IEnumerable values && value is not string)
        {
            var hash = default(HashCode);
            foreach (var item in values)
                hash.Add(GetHashCode(item));
            return hash.ToHashCode();
        }
        return value.GetHashCode();
    }

    private bool RecordsEqual(RecordValue left, RecordValue right)
        => left.Count == right.Count
            && left.Zip(right).All(fields => fields.First.Key == fields.Second.Key
                && Equals(fields.First.Value, fields.Second.Value));

    private bool EnumerablesEqual(IEnumerable left, IEnumerable right)
    {
        var leftEnumerator = left.GetEnumerator();
        var rightEnumerator = right.GetEnumerator();
        while (true)
        {
            var hasLeft = leftEnumerator.MoveNext();
            var hasRight = rightEnumerator.MoveNext();
            if (hasLeft != hasRight)
                return false;
            if (!hasLeft)
                return true;
            if (!Equals(leftEnumerator.Current, rightEnumerator.Current))
                return false;
        }
    }
}
