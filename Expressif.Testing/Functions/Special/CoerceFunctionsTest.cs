using Expressif.Functions;
using Expressif.Functions.Special;
using Expressif.Testing.Conformance;
using Expressif.Values.Casters;

namespace Expressif.Testing.Functions.Special;

[TestFixture]
public class CoerceFunctionsTest
{
    [Conformance]
    public void CoerceNumeric_Valid(object? value, decimal? expected)
        => Assert.That(new CoerceNumeric().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void CoerceInt_Valid(object? value, int? expected)
        => Assert.That(new CoerceInt().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void CoerceText_Valid(object? value, string? expected)
        => Assert.That(new CoerceText().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void CoerceBoolean_Valid(object? value, bool? expected)
        => Assert.That(new CoerceBoolean().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void CoerceDate_Valid(object? value, DateOnly? expected)
        => Assert.That(new CoerceDate().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void CoerceTime_Valid(object? value, TimeOnly? expected)
        => Assert.That(new CoerceTime().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void CoerceDatetime_Valid(object? value, DateTime? expected)
        => Assert.That(new CoerceDateTime().Evaluate(value), Is.EqualTo(expected));

    [Test]
    public void CoerceFunctions_ImplementTypedAndUntypedContracts()
    {
        IFunction<object?, decimal?> typed = new CoerceNumeric();
        IFunction untyped = typed;

        Assert.Multiple(() =>
        {
            Assert.That(typed.Evaluate("42.7"), Is.EqualTo(42.7m));
            Assert.That(untyped.Evaluate("42.7"), Is.EqualTo(42.7m));
        });
    }

    [TestCase("coerce-numeric", "42.7", typeof(decimal))]
    [TestCase("coerce-int", "42", typeof(int))]
    [TestCase("coerce-text", 42, typeof(string))]
    [TestCase("coerce-boolean", "yes", typeof(bool))]
    [TestCase("coerce-date", "2026-08-19", typeof(DateOnly))]
    [TestCase("coerce-time", "14:30:00", typeof(TimeOnly))]
    [TestCase("coerce-datetime", "2026-08-19 14:30:00", typeof(DateTime))]
    public void CoerceFunctions_AreAvailableToExpressionFactory(string name, object value, Type expectedType)
    {
        var function = new ExpressionFactory().Instantiate(name, new Context());

        Assert.That(function.Evaluate(value), Is.TypeOf(expectedType));
    }

    [TestCase(null)]
    [TestCase(0)]
    [TestCase(42.7)]
    [TestCase("yes")]
    [TestCase("abc")]
    public void CoerceBoolean_MatchesCasterTryCast(object? value)
    {
        var functionResult = new CoerceBoolean().Evaluate(value);
        var casterSuccess = new Caster().TryCast<bool>(value, out var casterResult);

        Assert.That(functionResult, Is.EqualTo(casterSuccess ? casterResult : null));
    }

    [Test]
    public void CoerceInt_Overflow_ReturnsNull()
        => Assert.That(new CoerceInt().Evaluate(long.MaxValue), Is.Null);
}
