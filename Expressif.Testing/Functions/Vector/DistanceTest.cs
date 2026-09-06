using Expressif.Functions;
using Expressif.Functions.Vector;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Functions.Vector;

public class DistanceTest
{
    [Conformance]
    public void Distance_Valid_EqualDimension(string value, string vector, decimal expected)
        => Assert.That(new Distance(() => Parse(vector)).Evaluate(Parse(value)), Is.EqualTo(expected));

    [Test]
    public void Evaluate_Symmetric_ReturnsSameDistance()
    {
        var left = new VectorValue(1, 2);
        var right = new VectorValue(4, 6);

        Assert.That(new Distance(() => right).Evaluate(left), Is.EqualTo(new Distance(() => left).Evaluate(right)));
    }

    [Test]
    public void Evaluate_DimensionMismatch_Throws()
        => Assert.That(
            () => new Distance(() => new VectorValue(1)).Evaluate(new VectorValue(1, 2)),
            Throws.ArgumentException.With.Message.Contains("equal dimensions"));

    [Test]
    public void Evaluate_TupleInput_ReturnsNull()
        => Assert.That(
            ((IFunction)new Distance(() => new VectorValue(1, 2))).Evaluate(new TupleValue(1, 2)),
            Is.Null);

    [Test]
    public void Expression_VectorDistance_ReturnsExpected()
        => Assert.That(Expression.CreateClosed("V(1, 2) | distance(V(4, 6))").Evaluate(null), Is.EqualTo(5m));

    private static VectorValue Parse(string source)
        => (VectorValue)Expression.CreateClosed(source).Evaluate(null)!;
}
