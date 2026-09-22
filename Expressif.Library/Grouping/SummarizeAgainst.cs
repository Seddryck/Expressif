using Expressif.Functions.Accumulation;
using Expressif.Values;
using GroupingValue = Expressif.Values.Grouping;

namespace Expressif.Library.Grouping;

/// <summary>Summarizes each group against one summary of all grouped values and returns an ordered dictionary.</summary>
[Function(prefix: "")]
[Scope("grouping")]
public sealed class SummarizeAgainst : IFunction<GroupingValue, DictionaryValue>
{
    private readonly Func<IAccumulator> local;
    private readonly Func<IAccumulator> global;
    private readonly Func<IFunction> combine;

    /// <param name="local">The accumulator applied independently to each group's values.</param>
    /// <param name="global">The accumulator applied once across every group's values.</param>
    /// <param name="combine">The operation combining a finalized local summary with the global summary.</param>
    public SummarizeAgainst(Func<IAccumulator> local, Func<IAccumulator> global, Func<IFunction> combine)
        => (this.local, this.global, this.combine) = (local, global, combine);

    public DictionaryValue Evaluate(GroupingValue value)
    {
        var globalState = global();
        globalState.Initialize();
        var localStates = new IAccumulator[value.Count];
        for (var index = 0; index < value.Count; index++)
        {
            var localState = local();
            localState.Initialize();
            localStates[index] = localState;
            foreach (var item in value[index].Values)
            {
                localState.Accumulate(item);
                globalState.Accumulate(item);
            }
        }

        var globalResult = globalState.GetValue();
        if (value.Count == 0)
            return new DictionaryValue([]);

        var operation = combine();
        var pairs = new PairValue[value.Count];
        for (var index = 0; index < value.Count; index++)
        {
            var localResult = localStates[index].GetValue();
            var arguments = new TupleValue(localResult, globalResult);
            pairs[index] = new PairValue(value[index].Key, EvaluationRuntime.EvaluateNested(operation, arguments));
        }

        return new DictionaryValue(pairs);
    }

    object? IFunction.Evaluate(object? value) => value is GroupingValue grouping ? Evaluate(grouping) : null;
}
