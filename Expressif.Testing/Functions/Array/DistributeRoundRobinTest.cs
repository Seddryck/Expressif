using Expressif.Functions.Array;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Functions.Array;

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
            Expression.Create("distribute-round-robin(2)").Evaluate(new[] { 1, 2, 3 }),
            Is.EqualTo(new object?[][] { [1, 3], [2] }));
}
