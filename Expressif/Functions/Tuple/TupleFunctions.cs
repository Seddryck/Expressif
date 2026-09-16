using System;
using Expressif.Values;

namespace Expressif.Functions.Tuple;

/// <summary>
/// Constructs a new tuple by evaluating zero or more positional expressions from left to right against the same input. Spread arguments expand array values in place.
/// </summary>
[Function(prefix: "", aliases: ["tuple"])]
[Scope("tuple")]
public sealed class Tuple : IFunction<object?, TupleValue>, IValueSpreadAware
{
    private Func<ValueArgumentEvaluator[]> Values { get; }

    /// <summary>Creates an empty tuple constructor.</summary>
    public Tuple()
        : this(() => []) { }

    /// <param name="values">Zero or more expressions whose evaluated values become the positions of the resulting tuple.</param>
    public Tuple(Func<ValueArgumentEvaluator[]> values)
        => Values = values;

    public TupleValue Evaluate(object? value)
        => new Expressif.Values.Tuple(ValueArguments.Evaluate(Values.Invoke(), value).ToArray());

    object? IFunction.Evaluate(object? value) => Evaluate(value);
}

/// <summary>
/// Returns the number of positional elements in the input tuple.
/// </summary>
[Function(prefix: "", aliases: ["arity"])]
[Scope("tuple")]
public sealed class Arity : IFunction<IPositionalValue, int>
{
    public int Evaluate(IPositionalValue value) => value.Arity;

    object? IFunction.Evaluate(object? value) => value is IPositionalValue tuple ? Evaluate(tuple) : null;
}

/// <summary>Returns a tuple with two positions exchanged, defaulting to the first and last positions.</summary>
[Function(prefix: "", aliases: ["swap"])]
[Scope("tuple")]
public sealed class Swap : IFunction<IPositionalValue, TupleValue?>, IFunction<VectorValue, VectorValue>
{
    private Func<int>? First { get; }
    private Func<int>? Second { get; }
    public Swap() { }
    /// <param name="first">Specifies the first zero-based position.</param>
    /// <param name="second">Specifies the second zero-based position.</param>
    public Swap(Func<int> first, Func<int> second) => (First, Second) = (first, second);
    public TupleValue? Evaluate(IPositionalValue value)
    {
        if (value.Arity == 0) return PositionalResultFactory.Create(value, []);
        var first = First?.Invoke() ?? 0;
        var second = Second?.Invoke() ?? value.Arity - 1;
        if (first < 0 || first >= value.Arity || second < 0 || second >= value.Arity)
            throw new IndexOutOfRangeException("Tuple swap position is out of range.");
        var values = Enumerable.Range(0, value.Arity).Select(value.GetPosition).ToArray();
        (values[first], values[second]) = (values[second], values[first]);
        return PositionalResultFactory.Create(value, values);
    }
    public VectorValue Evaluate(VectorValue value) => (VectorValue)Evaluate((IPositionalValue)value)!;
    object? IFunction.Evaluate(object? value) => value is IPositionalValue tuple ? Evaluate(tuple) : null;
}

/// <summary>Returns a new tuple with a value appended, expanding tuple values into their positions.</summary>
[Function(prefix: "", aliases: ["extend"])]
[Scope("tuple")]
public sealed class Extend : IFunction<IPositionalValue, TupleValue?>, IFunction<VectorValue, VectorValue?>
{
    private Func<IPositionalValue, object?> Extension { get; }
    /// <param name="value">Specifies the value to append; tuple values are expanded into their positions.</param>
    public Extend(Func<IPositionalValue, object?> value) => Extension = value;
    public TupleValue? Evaluate(IPositionalValue value)
    {
        var extension = Extension.Invoke(value);
        var values = extension is IPositionalValue tuple
            ? PositionalValues(value).Concat(PositionalValues(tuple)).ToArray()
            : PositionalValues(value).Append(extension).ToArray();
        return PositionalResultFactory.Create(value, values);

        static IEnumerable<object?> PositionalValues(IPositionalValue tuple)
            => Enumerable.Range(0, tuple.Arity).Select(tuple.GetPosition);
    }
    public VectorValue? Evaluate(VectorValue value) => (VectorValue?)Evaluate((IPositionalValue)value);
    object? IFunction.Evaluate(object? value) => value is IPositionalValue tuple ? Evaluate(tuple) : null;
}

/// <summary>Returns a tuple containing selected positions in the requested order.</summary>
[Function(prefix: "", aliases: ["pick"])]
[Scope("tuple")]
public sealed class Pick : IFunction<IPositionalValue, TupleValue?>, IFunction<VectorValue, VectorValue>
{
    private Func<int[]> Positions { get; }
    /// <param name="positions">One or more zero-based tuple positions.</param>
    public Pick(Func<int[]> positions) => Positions = positions;
    public TupleValue? Evaluate(IPositionalValue value)
    {
        var positions = Positions.Invoke();
        if (positions.Length == 0) throw new ArgumentException("Pick requires at least one position.");
        if (positions.Any(x => x < 0 || x >= value.Arity)) throw new IndexOutOfRangeException("Tuple pick position is out of range.");
        return PositionalResultFactory.Create(value, positions.Select(value.GetPosition).ToArray());
    }
    public VectorValue Evaluate(VectorValue value) => (VectorValue)Evaluate((IPositionalValue)value)!;
    object? IFunction.Evaluate(object? value) => value is IPositionalValue tuple ? Evaluate(tuple) : null;
}

/// <summary>Returns a flat record by applying one positional label to each tuple item and qualifying every field expanded from a record item.</summary>
[Function(prefix: "")]
[Scope("tuple")]
public sealed class Label : IFunction<IPositionalValue, RecordValue>, IValueSpreadAware
{
    private Func<ValueArgumentEvaluator[]> Names { get; }

    /// <param name="names">One label expression for each tuple position, in positional order. Spread arrays expand labels in place.</param>
    public Label(Func<ValueArgumentEvaluator[]> names) => Names = names;

    public RecordValue Evaluate(IPositionalValue value)
        => TupleLabeling.Create(value, ValueArguments.Evaluate(Names.Invoke(), value).ToArray(), qualifyConflictsOnly: false);

    object? IFunction.Evaluate(object? value) => value is IPositionalValue tuple ? Evaluate(tuple) : null;
}

/// <summary>Returns a flat record by applying positional labels only to record fields whose unqualified names would conflict.</summary>
[Function(prefix: "")]
[Scope("tuple")]
public sealed class LabelConflicts : IFunction<IPositionalValue, RecordValue>, IValueSpreadAware
{
    private Func<ValueArgumentEvaluator[]> Names { get; }

    /// <param name="names">One label expression for each tuple position, in positional order. Spread arrays expand labels in place.</param>
    public LabelConflicts(Func<ValueArgumentEvaluator[]> names) => Names = names;

    public RecordValue Evaluate(IPositionalValue value)
        => TupleLabeling.Create(value, ValueArguments.Evaluate(Names.Invoke(), value).ToArray(), qualifyConflictsOnly: true);

    object? IFunction.Evaluate(object? value) => value is IPositionalValue tuple ? Evaluate(tuple) : null;
}

internal static class TupleLabeling
{
    public static RecordValue Create(IPositionalValue value, object?[] labels, bool qualifyConflictsOnly)
    {
        ArgumentNullException.ThrowIfNull(labels);
        if (labels.Length != value.Arity)
            throw new ArgumentException("Number of labels must match tuple arity.", nameof(labels));
        if (labels.Any(label => label is not string))
            throw new ArgumentException("Every label must be text.", nameof(labels));

        var names = labels.Cast<string>().ToArray();
        var conflicts = qualifyConflictsOnly ? CountUnqualifiedNames(value, names) : null;
        var result = new RecordValue();
        for (var index = 0; index < value.Arity; index++)
        {
            var item = value.GetPosition(index);
            if (item is RecordValue record)
            {
                foreach (var field in record)
                {
                    var name = conflicts is not null && conflicts[field.Key] == 1
                        ? field.Key
                        : $"{names[index]}.{field.Key}";
                    SetUnique(result, name, field.Value);
                }
            }
            else
            {
                SetUnique(result, names[index], item);
            }
        }
        return result;
    }

    private static Dictionary<string, int> CountUnqualifiedNames(IPositionalValue value, string[] labels)
    {
        var counts = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var index = 0; index < value.Arity; index++)
        {
            var item = value.GetPosition(index);
            if (item is RecordValue record)
            {
                foreach (var field in record)
                    Increment(field.Key);
            }
            else
            {
                Increment(labels[index]);
            }
        }
        return counts;

        void Increment(string name)
            => counts[name] = counts.GetValueOrDefault(name) + 1;
    }

    private static void SetUnique(RecordValue target, string name, object? value)
    {
        if (target.ContainsKey(name))
            throw new ArgumentException($"Tuple labels produce duplicate field '{name}'.");
        target.Set(name, value);
    }
}

/// <summary>
/// Returns the tuple field at the specified zero-based position. Returns `null` when the input is not a tuple or the position is out of range.
/// </summary>
[Function(prefix: "", aliases: ["tuple-at"])]
[Scope("tuple")]
public class TupleAt : IFunction<IPositionalValue, object?>
{
    public Func<int> Position { get; }

    /// <param name="position">Specifies the zero-based position of the tuple field to return.</param>
    public TupleAt(Func<int> position)
        => Position = position;

    public object? Evaluate(object? value)
    {
        var position = Position.Invoke();
        var index = position == int.MinValue ? -1 : position < 0 ? tupleIndexFromEnd(position) : position;
        return value is IPositionalValue tuple && index >= 0 && index < tuple.Arity
            ? tuple.GetPosition(index)
            : null;

        int tupleIndexFromEnd(int offset)
            => value is IPositionalValue tuple ? tuple.Arity + offset : -1;
    }

    public object? Evaluate(IPositionalValue value) => Evaluate((object?)value);
}

/// <summary>
/// Returns the first field of a tuple. Returns `null` when the input is not a tuple.
/// </summary>
[Function(prefix: "", aliases: ["tuple-first"])]
[Scope("tuple")]
public class TupleFirst : TupleAt
{
    public TupleFirst()
        : base(() => 0) { }
}

/// <summary>
/// Returns the second field of a tuple. Returns `null` when the input is not a tuple.
/// </summary>
[Function(prefix: "", aliases: ["tuple-second"])]
[Scope("tuple")]
public class TupleSecond : TupleAt
{
    public TupleSecond()
        : base(() => 1) { }
}
