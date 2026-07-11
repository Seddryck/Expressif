using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions.Numeric;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Functions.Numeric;

[TestFixture]
public class NumericFunctionsTest
{
    [Conformance]
    public void NullToZero_Valid(object value, decimal? expected)
        => Assert.That(new NullToZero().Evaluate(value), Is.EqualTo(expected));

    [Test]
    [TestCase(typeof(DBNull), 0)]
    public void NullToZero_DBNull_Valid(Type type, decimal? expected)
        => Assert.That(new NullToZero().Evaluate(
            type.GetField("Value", BindingFlags.Static | BindingFlags.Public)!.GetValue(null))
            , Is.EqualTo(expected));

    [Conformance]
    public void Floor_Valid(object value, decimal? expected)
        => Assert.That(new Floor().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Ceiling_Valid(object value, decimal? expected)
        => Assert.That(new Ceiling().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Integer_Valid(object value, decimal? expected)
        => Assert.That(new Integer().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Round_Valid(object value, int digits, decimal? expected)
        => Assert.That(new Round(() => digits).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Clip_Interval1010Valid(object value, decimal min, decimal max, decimal? expected)
        => Assert.That(new Clip(() => min, () => max)
            .Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Invert_Valid(object value, decimal? expected)
        => Assert.That(new Invert().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Oppose_Valid(object? value, decimal? expected)
        => Assert.That(new Oppose().Evaluate(value), Is.EqualTo(expected));
}
