using Expressif.Functions;
using Expressif.Functions.Vector;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Functions.Vector;

public class NormalizeTest
{
    [Conformance]
    public void Normalize_Valid_Nonzero(string value, string expected)
        => Assert.That(
            new Normalize().Evaluate(Parse(value)),
            Is.EqualTo(Parse(expected)));

    [TestCase("V(0, 0)")]
    [TestCase("V()")]
    public void Evaluate_ZeroVector_Throws(string source)
        => Assert.That(
            () => new Normalize().Evaluate(Parse(source)),
            Throws.InvalidOperationException.With.Message.Contains("cannot be normalized"));

    [Test]
    public void Evaluate_ResultMagnitude_IsOne()
    {
        var result = new Normalize().Evaluate(new VectorValue(1, 1));
        Assert.That(VectorMagnitude(result), Is.EqualTo(1d).Within(1e-12));
    }

    [Test]
    public void Evaluate_TupleInput_ReturnsNull()
        => Assert.That(((IFunction)new Normalize()).Evaluate(new TupleValue(3, 4)), Is.Null);

    private static VectorValue Parse(string source)
        => (VectorValue)Expression.CreateClosed(source).Evaluate(null)!;

    private static double VectorMagnitude(VectorValue value)
        => Math.Sqrt(Enumerable.Range(0, value.Arity)
            .Select(index => Convert.ToDouble(value.GetPosition(index)))
            .Sum(component => component * component));
}
