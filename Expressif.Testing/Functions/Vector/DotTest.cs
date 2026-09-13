using Expressif.Functions;
using Expressif.Functions.Vector;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Functions.Vector;

public class DotTest
{
    [Conformance]
    public void Dot_Valid_EqualDimension(string value, string vector, decimal expected)
        => Assert.That(new Dot(() => Parse(vector)).Evaluate(Parse(value)), Is.EqualTo(expected));

    [Conformance]
    public void Dot_Valid_DimensionMismatch(string value, string vector, decimal? expected)
        => Assert.That(new Dot(() => Parse(vector)).Evaluate(Parse(value)), Is.EqualTo(expected));

    [Test]
    public void Evaluate_TupleInput_ReturnsNull()
        => Assert.That(
            ((IFunction)new Dot(() => new VectorValue(1, 2))).Evaluate(new TupleValue(1, 2)),
            Is.Null);

    [Test]
    public void Expression_VectorDot_ReturnsExpected()
        => Assert.That(Expression.CreateClosed("V(1, 2, 3) | dot(V(4, 5, 6))").Evaluate(null), Is.EqualTo(32m));

    [Test]
    public void Expression_DimensionMismatch_ReturnsNull()
        => Assert.That(Expression.CreateClosed("V(1, 2) | dot(V(3, 4, 5))").Evaluate(null), Is.Null);

    private static VectorValue Parse(string source)
        => (VectorValue)Expression.CreateClosed(source).Evaluate(null)!;
}
