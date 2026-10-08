using Expressif.Functions.Accumulation;
using Expressif.Values;

namespace Expressif.Library.Grouping;

/// <summary>Summarizes each group against one summary of all grouped values and returns an ordered dictionary.</summary>
[Function(prefix: "")]
[Scope("grouping")]
public sealed class SummarizeAgainst : IFunction<GroupingValue, DictionaryValue>
{
    private readonly Func<IIncrementalAggregation> local;
    private readonly Func<IIncrementalAggregation> global;
    private readonly Func<IFunction> combine;

    /// <param name="local">The incremental aggregation applied independently to each group's values.</param>
    /// <param name="global">The incremental aggregation applied once across every group's values.</param>
    /// <param name="combine">The operation combining a finalized local summary with the global summary.</param>
    public SummarizeAgainst(
        [ArgumentRole(ArgumentRole.Accumulator)] Func<IIncrementalAggregation> local,
        [ArgumentRole(ArgumentRole.Accumulator)] Func<IIncrementalAggregation> global,
        [ArgumentRole(ArgumentRole.Transformation)] Func<IFunction> combine)
        => (this.local, this.global, this.combine) = (local, global, combine);

    public DictionaryValue Evaluate(GroupingValue value)
    {
        var globalState = global().CreateSession();
        var localStates = new IAggregationSession[value.Count];
        for (var index = 0; index < value.Count; index++)
        {
            var localState = local().CreateSession();
            localStates[index] = localState;
            foreach (var item in value[index].Values)
            {
                localState.Add(item);
                globalState.Add(item);
            }
        }

        var globalResult = globalState.Snapshot();
        if (value.Count == 0)
            return new DictionaryValue([]);

        var operation = combine();
        var pairs = new PairValue[value.Count];
        for (var index = 0; index < value.Count; index++)
        {
            var localResult = localStates[index].Snapshot();
            var arguments = new TupleValue(localResult, globalResult);
            pairs[index] = new PairValue(value[index].Key, EvaluationRuntime.EvaluateNested(operation, arguments));
        }

        return new DictionaryValue(pairs);
    }

    object? IFunction.Evaluate(object? value) => value is GroupingValue grouping ? Evaluate(grouping) : null;
}
