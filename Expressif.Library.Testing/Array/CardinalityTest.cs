using Expressif.Library.Array.Aggregation;
using Expressif.Library.Array.Combination;
using Expressif.Library.Array.Grouping;
using Expressif.Library.Array.Partitioning;
using Expressif.Library.Array.Selection;
using Expressif.Library.Array.Sequencing;
using Expressif.Library.Array.Set;
using Expressif.Library.Array;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Array;

public class CardinalityTest
{
    [Conformance]
    public void Cardinality_Valid(object?[] value, int expected)
        => Assert.That(new Cardinality().Evaluate(value), Is.EqualTo(expected));
}
