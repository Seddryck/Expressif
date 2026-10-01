using System;
using System.Text;
using Expressif.Functions;
using Expressif.Values.Casters;

namespace Expressif.Library.Text;

/// <summary>
/// Combines accumulated text values in source order, inserting the separator only between values.
/// </summary>
[Function(prefix: "", Name = "concat")]
[Scope("array/aggregation")]
public class ConcatAccumulator : BaseArrayAggregation
{
    private readonly Func<string> separatorProvider;

    /// <summary>Creates a concatenation accumulator with no separator.</summary>
    public ConcatAccumulator()
        : this(() => string.Empty) { }

    /// <param name="separator">Specifies the text inserted between consecutive accumulated values.</param>
    public ConcatAccumulator(
        [ArgumentEvaluation(ArgumentEvaluationMode.Ambient)] Func<string> separator)
        => separatorProvider = separator;

    public override IAggregationSession CreateSession()
        => new Session(separatorProvider.Invoke());

    private sealed class Session(string separator) : IAggregationSession
    {
        private readonly StringBuilder value = new();
        private readonly TextCaster caster = new();
        private bool hasValue;

        public void Add(object? item)
        {
            if (item is null)
                throw new InvalidCastException("Cannot cast null value to text for concat aggregation.");

            if (hasValue)
                value.Append(separator);

            value.Append(caster.Cast(item));
            hasValue = true;
        }

        public object Snapshot() => value.ToString();
    }
}
