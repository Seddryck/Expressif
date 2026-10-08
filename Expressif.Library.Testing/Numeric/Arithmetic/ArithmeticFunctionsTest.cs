using Expressif.Library.Numeric.Arithmetic;
using Expressif.Library.Numeric.Classification;
using Expressif.Library.Numeric.Conversion;
using Expressif.Library.Numeric.Formatting;
using Expressif.Library.Numeric.Rounding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Library.IO;
using Expressif.Library.Numeric;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Numeric.Arithmetic;

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
    public void Add_Valid_3(object value, decimal param, decimal? expected)
        => Assert.That(new Add(() => param).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Add_Valid_3Times2(object value, decimal param, int times, decimal? expected)
        => Assert.That(new Add(() => param, () => times)
            .Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Subtract_Valid_3(object value, decimal param, decimal? expected)
        => Assert.That(new Subtract(() => param).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Subtract_Valid_3Times2(object value, decimal param, int times, decimal? expected)
        => Assert.That(new Subtract(() => param, () => times)
            .Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Multiply_Valid_3(object value, decimal param, decimal? expected)
        => Assert.That(new Multiply(() => param)
            .Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Divide_Valid_4(object value, decimal param, decimal? expected)
        => Assert.That(new Divide(() => param)
            .Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void GreatestCommonDivisor_Valid(object value, int parameter, decimal? expected)
        => Assert.That(new GreatestCommonDivisor(() => parameter).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void LowestCommonMultiple_Valid(object value, int parameter, decimal? expected)
        => Assert.That(new LowestCommonMultiple(() => parameter).Evaluate(value), Is.EqualTo(expected));
}

