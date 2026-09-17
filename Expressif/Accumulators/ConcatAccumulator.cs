using System;
using System.Text;
using Expressif.Values.Casters;

namespace Expressif.Accumulators;

/// <summary>
/// Combines accumulated text values in source order, inserting the separator only between values.
/// </summary>
[Accumulator(prefix: "", aliases: ["concat", "implode"])]
[AccumulatorAliasLifecycle("implode", "concat")]
public class ConcatAccumulator : BaseAccumulator
{
    private readonly Func<string> separatorProvider;
    private readonly StringBuilder value = new();
    private readonly TextCaster caster = new();
    private string separator = string.Empty;
    private bool hasValue;

    public ConcatAccumulator()
        : this(() => string.Empty) { }

    /// <param name="separator">Specifies the text inserted between consecutive accumulated values.</param>
    public ConcatAccumulator(Func<string> separator)
        => separatorProvider = separator;

    public override void Initialize()
    {
        value.Clear();
        separator = separatorProvider.Invoke();
        hasValue = false;
    }

    public override void Accumulate(object? item)
    {
        if (item is null)
            throw new InvalidCastException("Cannot cast null value to text for concat aggregation.");

        if (hasValue)
            value.Append(separator);

        value.Append(caster.Cast(item));
        hasValue = true;
    }

    public override object GetValue()
        => value.ToString();
}
