using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Expressif.Values.Casters;

namespace Expressif.Functions.Array;

/// <summary>
/// Distributes array values into two groups whose aggregate evaluated weights are approximately balanced. Returns `null` when the input or a weight cannot be evaluated.
/// </summary>
[Function(prefix: "", aliases: [])]
[Scope("array/partitioning")]
public sealed class DistributeWeight : BaseArrayFunction
{
    public Func<IFunction> Weight { get; }

    /// <param name="weight">Specifies the expression that produces a finite, non-negative numeric weight for each input value.</param>
    public DistributeWeight(Func<IFunction> weight)
        => Weight = weight;

    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        if (enumerable is null)
            return null;

        var weightFunction = Weight.Invoke();
        var weighted = new List<WeightedValue>();
        var position = 0;
        foreach (var item in enumerable)
        {
            object? evaluated;
            using (EvaluationRuntime.Derive(item))
                evaluated = weightFunction.Evaluate(item);

            if (evaluated is null
                || !TypeChecker.IsNumericType(evaluated)
                || !NumericCoercion.TryToDecimal(evaluated, out var weight)
                || weight < 0)
                return null;

            weighted.Add(new WeightedValue(item, weight, position));
            position++;
        }

        decimal firstWeight = 0;
        decimal secondWeight = 0;
        var firstCount = 0;
        var secondCount = 0;

        foreach (var value in weighted.OrderByDescending(value => value.Weight).ThenBy(value => value.Position))
        {
            var useFirst = firstWeight < secondWeight
                || (firstWeight == secondWeight && firstCount <= secondCount);
            try
            {
                if (useFirst)
                {
                    firstWeight = checked(firstWeight + value.Weight);
                    firstCount++;
                    value.Group = 0;
                }
                else
                {
                    secondWeight = checked(secondWeight + value.Weight);
                    secondCount++;
                    value.Group = 1;
                }
            }
            catch (OverflowException)
            {
                return null;
            }
        }

        var first = weighted.Where(value => value.Group == 0).Select(value => value.Value).ToArray();
        var second = weighted.Where(value => value.Group == 1).Select(value => value.Value).ToArray();
        return new object?[][] { first, second };
    }

    private sealed class WeightedValue(object? value, decimal weight, int position)
    {
        public object? Value { get; } = value;
        public decimal Weight { get; } = weight;
        public int Position { get; } = position;
        public int Group { get; set; }
    }
}
