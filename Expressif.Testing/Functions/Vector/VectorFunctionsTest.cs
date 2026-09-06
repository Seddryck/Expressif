using Expressif.Functions.Tuple;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Functions.Vector;

public class VectorFunctionsTest
{
    [Conformance]
    public void Vector_Valid_Components(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(Expression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [TestCase("V(1, \"x\")")]
    [TestCase("V(1, #null)")]
    public void Vector_InvalidComponent_Throws(string source)
        => Assert.That(
            () => Expression.CreateClosed(source).Evaluate(null),
            Throws.Exception.With.InnerException.TypeOf<ArgumentException>()
                .And.InnerException.Message.Contains("Vector components must be numeric."));

    [Test]
    public void Vector_InvalidSpreadComponent_Throws()
        => Assert.That(
            () => Expression.CreateClosed("V(...T(1, \"x\"))").Evaluate(null),
            Throws.Exception.With.InnerException.TypeOf<SpreadArgumentException>()
                .And.InnerException.Message.EqualTo("Vector spread argument must evaluate to a vector."));

    [Test]
    public void TupleOperations_Vector_PreserveVectorOrReturnNull()
    {
        var vector = new Expressif.Values.Vector(1, 2, 3);

        Assert.Multiple(() =>
        {
            Assert.That(new Swap().Evaluate(vector), Is.TypeOf<Expressif.Values.Vector>());
            Assert.That(new Pick(() => [2, 0]).Evaluate(vector),
                Is.TypeOf<Expressif.Values.Vector>().And.EqualTo(new VectorValue(3, 1)));
            Assert.That(new Extend(_ => 4).Evaluate(vector),
                Is.TypeOf<Expressif.Values.Vector>().And.EqualTo(new VectorValue(1, 2, 3, 4)));
            Assert.That(new Extend(_ => "x").Evaluate(vector), Is.Null);
            Assert.That(new Extend(_ => new Expressif.Values.Tuple(4, "x")).Evaluate(vector), Is.Null);
        });
    }

    [Test]
    public void NumericTuple_IsNeverPromotedToVector()
        => Assert.That(
            new Swap().Evaluate(new Expressif.Values.Tuple(1, 2)),
            Is.TypeOf<Expressif.Values.Tuple>());

    [TestCase("V(1, 2) | swap", "V(2, 1)")]
    [TestCase("V(1, 2, 3) | pick(2, 0)", "V(3, 1)")]
    [TestCase("V(1, 2) | extend(3)", "V(1, 2, 3)")]
    public void TupleOperation_VectorExpression_PreservesVector(string source, string expected)
        => Assert.That(ValueFormatter.Format(Expression.CreateClosed(source).Evaluate(null)), Is.EqualTo(expected));

    [Test]
    public void Extend_VectorWithNonnumericComponent_ReturnsNull()
        => Assert.That(Expression.CreateClosed("V(1, 2) | extend(\"x\")").Evaluate(null), Is.Null);
}
