using Expressif.Predicates.Numeric;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Predicates.Numeric;

[TestFixture]
public class ModuloTest
{
    [Conformance]
    public void HasRemainder_Valid(object value, int modulus, int remainder, bool expected)
    {
        var predicate = new Modulo(() => modulus, () => remainder);
        Assert.Multiple(() =>
        {
            Assert.That(predicate.Reference.Invoke(), Is.EqualTo(modulus));
            Assert.That(predicate.Remainder.Invoke(), Is.EqualTo(remainder));
            Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
        });
    }

    [Conformance]
    public void IsDivisibleBy_Valid(object? value, int divisor, bool expected)
    {
        var predicate = new DivisibleBy(() => divisor);
        Assert.Multiple(() =>
        {
            Assert.That(predicate.Divisor.Invoke(), Is.EqualTo(divisor));
            Assert.That(predicate.Remainder.Invoke(), Is.Zero);
            Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
        });
    }

    [Conformance]
    public void IsEven_Valid(object? value, bool expected)
    {
        var predicate = new Even();
        Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
    }

    [Conformance]
    public void IsOdd_Valid(object? value, bool expected)
    {
        var predicate = new Odd();
        Assert.That(predicate.Evaluate(value), Is.EqualTo(expected));
    }
}
