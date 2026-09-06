using Expressif.Functions;
using Expressif.Functions.Vector;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Functions.Vector;

public class MagnitudeTest
{
    [Conformance]
    public void Magnitude_Valid_Vector(string value, decimal expected)
        => Assert.That(new Magnitude().Evaluate(Parse(value)), Is.EqualTo(expected));

    [Test]
    public void Evaluate_FractionalComponents_ReturnsExpectedPrecision()
        => Assert.That(
            new Magnitude().Evaluate(new VectorValue(1, 1)),
            Is.EqualTo((decimal)Math.Sqrt(2d)));

    [Test]
    public void Evaluate_TupleInput_ReturnsNull()
        => Assert.That(((IFunction)new Magnitude()).Evaluate(new TupleValue(3, 4)), Is.Null);

    [Test]
    public void Expression_VectorMagnitude_ReturnsExpected()
        => Assert.That(Expression.CreateClosed("V(3, 4) | magnitude").Evaluate(null), Is.EqualTo(5m));

    private static VectorValue Parse(string source)
        => (VectorValue)Expression.CreateClosed(source).Evaluate(null)!;
}
