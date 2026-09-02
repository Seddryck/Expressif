using System;
using System.Collections;
using System.Collections.Generic;
using Expressif.Values.Casters;

namespace Expressif.Functions.Array;

/// <summary>
/// Randomly distributes array values among output arrays according to relative output weights. Returns `null` when the input, weights, or seed cannot be evaluated.
/// </summary>
[Function(prefix: "", aliases: [])]
[Scope("array/partitioning")]
public sealed class DistributeRandomSplit : BaseArrayFunction
{
    public Func<object?[]> Weights { get; }
    public Func<int>? Seed { get; }

    /// <param name="weights">Specifies a non-empty array of finite, non-negative output weights with a positive total.</param>
    public DistributeRandomSplit(Func<object?[]> weights)
        => (Weights, Seed) = (weights, null);

    /// <param name="weights">Specifies a non-empty array of finite, non-negative output weights with a positive total.</param>
    /// <param name="seed">Specifies an optional seed that makes assignments reproducible on the same runtime version.</param>
    public DistributeRandomSplit(Func<object?[]> weights, Func<int> seed)
        => (Weights, Seed) = (weights, seed);

    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        if (enumerable is null || !TryGetWeights(out var weights, out var total))
            return null;

        var outputs = new List<object?>[weights.Length];
        for (var index = 0; index < outputs.Length; index++)
            outputs[index] = [];

        var random = Seed is null ? Random.Shared : new Random(Seed.Invoke());
        foreach (var item in enumerable)
            outputs[SelectOutput(random, weights, total)].Add(item);

        var result = new object?[outputs.Length][];
        for (var index = 0; index < outputs.Length; index++)
            result[index] = outputs[index].ToArray();

        return result;
    }

    private bool TryGetWeights(out decimal[] weights, out decimal total)
    {
        var values = Weights.Invoke();
        weights = values is null ? [] : new decimal[values.Length];
        total = 0;
        if (values is null || values.Length == 0)
            return false;

        for (var index = 0; index < values.Length; index++)
        {
            if (values[index] is null
                || !TypeChecker.IsNumericType(values[index]!)
                || !NumericCoercion.TryToDecimal(values[index], out var weight)
                || weight < 0)
                return false;

            weights[index] = weight;
            try
            {
                total = checked(total + weight);
            }
            catch (OverflowException)
            {
                return false;
            }
        }

        return total > 0;
    }

    private static int SelectOutput(Random random, decimal[] weights, decimal total)
    {
        var sample = (decimal)random.NextDouble() * total;
        decimal cumulative = 0;
        for (var index = 0; index < weights.Length; index++)
        {
            cumulative += weights[index];
            if (sample < cumulative)
                return index;
        }

        return weights.Length - 1;
    }
}
