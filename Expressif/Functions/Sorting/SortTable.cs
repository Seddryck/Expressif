using System.Collections;
using Expressif.Functions.Array;
using Expressif.Values;

namespace Expressif.Functions.Sorting;

/// <summary>Normalizes pairs of sort keys and original values into shared headers and data rows.</summary>
[Function(prefix: "")]
[Scope("sorting")]
public sealed class SortTable : BaseArrayFunction<SortTableValue>
{
    protected override object EvaluateArray(IEnumerable enumerable)
    {
        var pairs = enumerable.Cast<object?>().ToArray();
        if (pairs.Length == 0)
            return new SortTableValue([], []);

        if (pairs.Any(item => item is not PairValue { Key: SortKeyValue }))
            throw new ArgumentException("Every SortTable input row must be a pair with a SortKey key.", nameof(enumerable));

        var typed = pairs.Cast<PairValue>().Select(pair => (Key: (SortKeyValue)pair.Key!, pair.Value)).ToArray();
        var first = typed[0].Key.Terms;
        foreach (var row in typed.Skip(1))
        {
            if (row.Key.Terms.Count != first.Count)
                throw new ArgumentException("Every SortTable row must have the same key arity.", nameof(enumerable));
            for (var index = 0; index < first.Count; index++)
            {
                var expected = first[index];
                var actual = row.Key.Terms[index];
                if (!expected.Comparer.Equals(actual.Comparer)
                    || expected.Ascending != actual.Ascending
                    || expected.NullsFirst != actual.NullsFirst)
                    throw new ArgumentException($"SortTable metadata differs at key position {index}.", nameof(enumerable));
            }
        }

        var headers = first.Select(term => new SortHeader(term.Comparer, term.Ascending, term.NullsFirst));
        var rows = typed.Select(row => new SortRow(row.Key.Terms.Select(term => term.Value).ToArray(), row.Value));
        return new SortTableValue(headers, rows);
    }
}
