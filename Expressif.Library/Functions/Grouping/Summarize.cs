using Expressif.Values;
using DictionaryValue = Expressif.Values.Dictionary;
using GroupingValue = Expressif.Values.Grouping;
using PairValue = Expressif.Values.Pair;

namespace Expressif.Functions.Grouping;

/// <summary>Evaluates an expression once for each group and returns a dictionary from group keys to summary values.</summary>
[Function(prefix: "")]
[Scope("grouping")]
public sealed class Summarize : IFunction<GroupingValue, DictionaryValue>
{
    private Func<IFunction> Expression { get; }

    /// <param name="expression">The expression evaluated against each group's value collection.</param>
    public Summarize(Func<IFunction> expression)
        => Expression = expression;

    public DictionaryValue Evaluate(GroupingValue value)
    {
        var expression = Expression.Invoke();
        return new DictionaryValue(value.Select(group =>
        {
            return new PairValue(group.Key, EvaluationRuntime.EvaluateNested(expression, group.Values));
        }));
    }

    object? IFunction.Evaluate(object? value) => value is GroupingValue grouping ? Evaluate(grouping) : null;
}
