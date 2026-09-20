using Expressif.Library.Array.Aggregation;
using Expressif.Library.Array.Combination;
using Expressif.Library.Array.Grouping;
using Expressif.Library.Array.Partitioning;
using Expressif.Library.Array.Selection;
using Expressif.Library.Array.Sequencing;
using Expressif.Library.Array.Set;
using System.Collections;
using Expressif.Library.Array;
using Expressif.Testing.Conformance;
using SingleFunction = Expressif.Library.Array.Selection.Single;

namespace Expressif.Testing.Array.Selection;

[TestFixture]
public class SingleTest
{
    [Conformance]
    public void Single_Valid_Cardinality(object? input, object? expected)
        => Assert.That(new SingleFunction().Evaluate(input), Is.EqualTo(expected));

    [Test]
    public void Evaluate_SoleStructuredValue_PreservesReference()
    {
        var value = new object();

        Assert.That(new SingleFunction().Evaluate(new[] { value }), Is.SameAs(value));
    }

    [Test]
    public void Evaluate_MultipleElements_StopsAfterSecondElement()
    {
        var source = new ThrowAfterSecondEnumerable();

        Assert.That(new SingleFunction().Evaluate(source), Is.Null);
    }

    private sealed class ThrowAfterSecondEnumerable : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            yield return 1;
            yield return 2;
            throw new InvalidOperationException("The source should not be enumerated past its second element.");
        }
    }
}
