using Expressif.Functions.Numeric;
using Expressif.Testing.Conformance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Functions.Numeric;

[TestFixture]
public class ArithmeticFunctionsTest
{
    [Conformance]
    public void Increment_Valid(object value, decimal? expected)
        => Assert.That(new Increment().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Decrement_Valid(object value, decimal? expected)
        => Assert.That(new Decrement().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Add_3_Valid(object value, decimal param, decimal? expected)
        => Assert.That(new Add(() => param).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Add_3Times2_Valid(object value, decimal param, int times, decimal? expected)
        => Assert.That(new Add(() => param, () => times)
            .Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Subtract_3_Valid(object value, decimal param, decimal? expected)
        => Assert.That(new Subtract(() => param).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Subtract_3Times2_Valid(object value, decimal param, int times, decimal? expected)
        => Assert.That(new Subtract(() => param, () => times)
            .Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Multiply_3_Valid(object value, decimal param, decimal? expected)
        => Assert.That(new Multiply(() => param)
            .Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Divide_4_Valid(object value, decimal param, decimal? expected)
        => Assert.That(new Divide(() => param)
            .Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void GreatestCommonDivisor_Valid(object value, int parameter, decimal? expected)
        => Assert.That(new GreatestCommonDivisor(() => parameter).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void LowestCommonMultiple_Valid(object value, int parameter, decimal? expected)
        => Assert.That(new LowestCommonMultiple(() => parameter).Evaluate(value), Is.EqualTo(expected));
}

