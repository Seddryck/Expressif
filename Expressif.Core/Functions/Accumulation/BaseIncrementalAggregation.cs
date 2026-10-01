using System.Collections;
using Expressif.Functions;

namespace Expressif.Functions.Accumulation;

/// <summary>Base class for ordinary functions that also support incremental aggregation.</summary>
[Scope("array/aggregation")]
public abstract class BaseIncrementalAggregation : IIncrementalAggregation
{
    object? IFunction<IEnumerable, object?>.Evaluate(IEnumerable value)
        => Evaluate(value);

    public virtual object? Evaluate(object? value)
        => value is IEnumerable enumerable && value is not string
            ? Evaluate(enumerable)
            : null;

    public object? Evaluate(IEnumerable value)
    {
        var session = CreateSession();
        foreach (var item in value)
            session.Add(item);

        return session.Snapshot();
    }

    public abstract IAggregationSession CreateSession();
}
