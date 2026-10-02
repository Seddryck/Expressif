using Expressif.Library.Array.Aggregation;
using Expressif.Library.Array.Combination;
using Expressif.Library.Array.Grouping;
using Expressif.Library.Array.Partitioning;
using Expressif.Library.Array.Selection;
using Expressif.Library.Array.Sequencing;
using Expressif.Library.Array.Set;
using Expressif.Library.Array;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Array.Partitioning;

[TestFixture]
public class DistributeRoundRobinTest
{
    [Conformance]
    public void DistributeRoundRobin_Valid_Count(object? input, int count, object? expected)
    {
        if (input is "(null)")
        {
            Assert.That(new DistributeRoundRobin(() => count).Evaluate(null), Is.Null);
            return;
        }

        Assert.That(new DistributeRoundRobin(() => count).Evaluate(input), Is.EqualTo(expected));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Evaluate_NonPositiveCount_ReturnsNull(int count)
        => Assert.That(new DistributeRoundRobin(() => count).Evaluate(new[] { 1, 2 }), Is.Null);

    [Test]
    public void Expression_Count_InstantiatesAndEvaluates()
        => Assert.That(
            TestExpression.Create("distribute-round-robin(2)").Evaluate(new[] { 1, 2, 3 }),
            Is.EqualTo(new object?[][] { [1, 3], [2] }));
}
