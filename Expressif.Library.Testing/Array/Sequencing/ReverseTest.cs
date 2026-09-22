using Expressif.Library.Array.Aggregation;
using Expressif.Library.Array.Combination;
using Expressif.Library.Array.Grouping;
using Expressif.Library.Array.Partitioning;
using Expressif.Library.Array.Selection;
using Expressif.Library.Array.Sequencing;
using Expressif.Library.Array.Set;
using Expressif.Library.Array;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Array.Sequencing;

[TestFixture]
public class ReverseTest
{
    [Conformance]
    public void Reverse_Valid(object input, object? expected)
        => Assert.That(new Reverse().Evaluate(input), Is.EqualTo(expected));
}

