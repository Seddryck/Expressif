using System;
using Expressif.Functions;
using Expressif.Values.Casters;

namespace Expressif.Library.Array.Aggregation;

/// <summary>
/// Counts the number of accumulated items, including <see langword="null"/> values.
/// </summary>
[Function(prefix: "", Name = "count")]
public class CountAccumulator : BaseArrayAggregation
{
    public override IAggregationSession CreateSession() => new Session();

    private sealed class Session : IAggregationSession
    {
        private int count;

        public void Add(object? item) => count++;

        public object Snapshot() => count;
    }
}

/// <summary>
/// Computes the sum of all accumulated numeric values.
/// </summary>
/// <remarks>
/// Each item is converted using <see cref="NumericCaster"/>.
/// A <see cref="InvalidCastException"/> is thrown when a <see langword="null"/> value is accumulated.
/// </remarks>
[Function(prefix: "", Name = "sum")]
public class SumAccumulator : BaseArrayAggregation
{
    public override IAggregationSession CreateSession() => new Session();

    private sealed class Session : IAggregationSession
    {
        private readonly NumericCaster caster = new();
        private decimal sum;

        public void Add(object? item)
            => sum += caster.Cast(item ?? throw new InvalidCastException("Cannot cast null value to numeric for sum aggregation."));

        public object Snapshot() => sum;
    }
}

/// <summary>
/// Tracks the smallest numeric value found during accumulation.
/// </summary>
/// <remarks>
/// Returns <see langword="null"/> when no value has been accumulated.
/// </remarks>
[Function(prefix: "", Name = "min")]
public class MinAccumulator : BaseArrayAggregation
{
    public override IAggregationSession CreateSession() => new Session();

    private sealed class Session : IAggregationSession
    {
        private readonly NumericCaster caster = new();
        private decimal? minimum;

        public void Add(object? item)
        {
            var numeric = caster.Cast(item ?? throw new InvalidCastException("Cannot cast null value to numeric for min aggregation."));
            minimum = minimum.HasValue ? Math.Min(minimum.Value, numeric) : numeric;
        }

        public object? Snapshot() => minimum;
    }
}

/// <summary>
/// Tracks the greatest numeric value found during accumulation.
/// </summary>
/// <remarks>
/// Returns <see langword="null"/> when no value has been accumulated.
/// </remarks>
[Function(prefix: "", Name = "max")]
public class MaxAccumulator : BaseArrayAggregation
{
    public override IAggregationSession CreateSession() => new Session();

    private sealed class Session : IAggregationSession
    {
        private readonly NumericCaster caster = new();
        private decimal? maximum;

        public void Add(object? item)
        {
            var numeric = caster.Cast(item ?? throw new InvalidCastException("Cannot cast null value to numeric for max aggregation."));
            maximum = maximum.HasValue ? Math.Max(maximum.Value, numeric) : numeric;
        }

        public object? Snapshot() => maximum;
    }
}

/// <summary>
/// Stores the first accumulated item and ignores all subsequent items.
/// </summary>
/// <remarks>
/// Returns <see langword="null"/> when no value has been accumulated.
/// </remarks>
[Function(prefix: "", Name = "first")]
public class FirstAccumulator : BaseArrayAggregation
{
    public override IAggregationSession CreateSession() => new Session();

    private sealed class Session : IAggregationSession
    {
        private object? first;
        private bool hasValue;

        public void Add(object? item)
        {
            if (hasValue)
                return;

            first = item;
            hasValue = true;
        }

        public object? Snapshot() => hasValue ? first : null;
    }
}

/// <summary>
/// Stores the most recently accumulated item.
/// </summary>
/// <remarks>
/// Returns <see langword="null"/> when no value has been accumulated.
/// </remarks>
[Function(prefix: "", Name = "last")]
public class LastAccumulator : BaseArrayAggregation
{
    public override IAggregationSession CreateSession() => new Session();

    private sealed class Session : IAggregationSession
    {
        private object? last;
        private bool hasValue;

        public void Add(object? item)
        {
            last = item;
            hasValue = true;
        }

        public object? Snapshot() => hasValue ? last : null;
    }
}

/// <summary>
/// Returns <see langword="true"/> only when every accumulated boolean value is <see langword="true"/>.
/// </summary>
/// <remarks>
/// The neutral value is <see langword="true"/>. Each item is converted using <see cref="BooleanCaster"/>.
/// A <see cref="InvalidCastException"/> is thrown when a <see langword="null"/> value is accumulated.
/// </remarks>
[Function(prefix: "", Name = "every")]
public class EveryAccumulator : BaseArrayAggregation
{
    public override IAggregationSession CreateSession() => new Session();

    private sealed class Session : IAggregationSession
    {
        private readonly BooleanCaster caster = new();
        private bool every = true;

        public void Add(object? item)
            => every &= caster.Cast(item ?? throw new InvalidCastException("Cannot cast null value to boolean for every aggregation."));

        public object Snapshot() => every;
    }
}

/// <summary>
/// Returns <see langword="true"/> when at least one accumulated boolean value is <see langword="true"/>.
/// </summary>
/// <remarks>
/// The neutral value is <see langword="false"/>. Each item is converted using <see cref="BooleanCaster"/>.
/// A <see cref="InvalidCastException"/> is thrown when a <see langword="null"/> value is accumulated.
/// </remarks>
[Function(prefix: "", Name = "any")]
public class AnyAccumulator : BaseArrayAggregation
{
    public override IAggregationSession CreateSession() => new Session();

    private sealed class Session : IAggregationSession
    {
        private readonly BooleanCaster caster = new();
        private bool any;

        public void Add(object? item)
            => any |= caster.Cast(item ?? throw new InvalidCastException("Cannot cast null value to boolean for any aggregation."));

        public object Snapshot() => any;
    }
}

/// <summary>
/// Combines accumulated items in source order by evaluating an expression against the accumulated value and current item.
/// </summary>
/// <remarks>
/// The expression receives a two-element tuple where <c>$0</c> is the accumulated value and <c>$1</c> is the current item.
/// Without an initial value, the first item becomes the accumulated value. An empty input then returns <see langword="null"/>.
/// </remarks>
[Function(prefix: "", Name = "reduce")]
public class ReduceAccumulator : BaseArrayAggregation
{
    private readonly Func<IFunction> operationProvider;
    private readonly Func<object?>? initialProvider;

    /// <param name="operation">Specifies the expression evaluated against each accumulated-value/current-item tuple.</param>
    public ReduceAccumulator(Func<IFunction> operation)
        => operationProvider = operation;

    /// <param name="operation">Specifies the expression evaluated against each accumulated-value/current-item tuple.</param>
    /// <param name="initial">Specifies the initial accumulated value. It is returned unchanged for an empty input.</param>
    public ReduceAccumulator(Func<IFunction> operation, Func<object?> initial)
        => (operationProvider, initialProvider) = (operation, initial);

    public override IAggregationSession CreateSession()
        => new Session(operationProvider.Invoke(), initialProvider is not null, initialProvider?.Invoke());

    private sealed class Session(IFunction operation, bool hasValue, object? value) : IAggregationSession
    {
        private object? value = value;
        private bool hasValue = hasValue;

        public void Add(object? item)
        {
            if (!hasValue)
            {
                value = item;
                hasValue = true;
                return;
            }

            var pair = new Expressif.Values.TupleValue(value, item);
            value = EvaluationRuntime.EvaluateNested(operation, pair);
        }

        public object? Snapshot() => hasValue ? value : null;
    }
}
