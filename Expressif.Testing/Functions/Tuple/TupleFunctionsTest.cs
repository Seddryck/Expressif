using Expressif.Functions.Tuple;
using Expressif.Values;

namespace Expressif.Testing.Functions.Tuple;

public class TupleFunctionsTest
{
    [Test]
    public void Evaluate_Accessors_Valid()
    {
        var tuple = new TupleValue(10, 20, 30);
        Assert.Multiple(() =>
        {
            Assert.That(new TupleFirst().Evaluate(tuple), Is.EqualTo(10));
            Assert.That(new TupleSecond().Evaluate(tuple), Is.EqualTo(20));
            Assert.That(new TupleAt(() => 2).Evaluate(tuple), Is.EqualTo(30));
            Assert.That(new TupleAt(() => 3).Evaluate(tuple), Is.Null);
            Assert.That(new TupleFirst().Evaluate(new object[] { 10, 20 }), Is.Null);
        });
    }
}
