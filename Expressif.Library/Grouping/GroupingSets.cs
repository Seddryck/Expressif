using Expressif.Values;
using GroupingValue = Expressif.Values.Grouping;

namespace Expressif.Library.Grouping;

/// <summary>Expands a grouping into explicitly declared sets of retained key dimensions.</summary>
[Function(prefix: "")]
[Scope("grouping")]
public sealed class GroupingSets : IFunction<GroupingValue, GroupingValue>
{
    private readonly Func<object?, object?[]> values;

    /// <summary>Creates a selection with no grouping levels.</summary>
    public GroupingSets()
        : this(_ => []) { }

    /// <param name="values">Zero or more tuples of zero-based key dimension positions to retain.</param>
    public GroupingSets([ArgumentPacking(ArgumentPackingMode.Variadic, AllowSpread = true)] [ArgumentOmission(ArgumentOmissionMode.EmptyVariadic)] Func<object?, object?[]> values)
        => this.values = values;

    public GroupingValue Evaluate(GroupingValue value)
    {
        var dimensions = value.Count == 0 ? 0 : value[0].Key is TupleValue key ? key.Count : 1;
        var levels = new List<bool[]>();
        foreach (var specification in values.Invoke(value))
        {
            if (specification is not TupleValue set)
                throw new ArgumentException("Every grouping set must be a tuple of integer dimension positions.", nameof(value));
            var aggregated = Enumerable.Repeat(true, dimensions).ToArray();
            foreach (var position in set)
            {
                if (position is not (byte or sbyte or short or ushort or int or uint or long or ulong or decimal))
                    throw new ArgumentException("Grouping set positions must be integers.", nameof(value));
                var index = Convert.ToDecimal(position, System.Globalization.CultureInfo.InvariantCulture);
                if (index != decimal.Truncate(index) || index < 0 || index >= dimensions)
                    throw new ArgumentException("Grouping set positions must be integers within the existing key dimensions.", nameof(value));
                aggregated[(int)index] = false;
            }
            if (!levels.Any(level => level.SequenceEqual(aggregated)))
                levels.Add(aggregated);
        }
        return GroupingLevels.Expand(value, _ => levels);
    }

    object? IFunction.Evaluate(object? value) => value is GroupingValue grouping ? Evaluate(grouping) : null;
}
