using Expressif.Functions;
using Expressif.Functions.Special;
using Expressif.Testing.Conformance;
using Expressif.Values.Casters;
using Expressif.Values;
using System.Numerics;
using Expressif.Bindings;

namespace Expressif.Testing.Functions.Special;

[TestFixture]
public class CoerceFunctionsTest
{
    [Conformance]
    public void Coerce_Valid_TypeDirected(object? value, string expression, string expected)
    {
        var input = value is string text
            ? text.Contains("=>")
                ? Expression.CreateClosed(text).Evaluate(null)
                : text.StartsWith("T(") || text.StartsWith('{')
                    ? new ParameterValueConverter().Parse(text)
                    : value
            : value;
        Assert.That(ValueFormatter.Format(Expression.Create(expression).Evaluate(input)), Is.EqualTo(expected));
    }

    [TestCase("coerce()")]
    [TestCase("coerce(:text, $2 -> :integer)")]
    [TestCase("coerce(name -> :text, $2 -> :integer)")]
    [TestCase("coerce(age -> :integer, age -> :text)")]
    [TestCase("coerce($1 -> :integer, $1 -> :text)")]
    [TestCase("coerce($^1 -> :integer)")]
    [TestCase("coerce(:expression)")]
    public void Coerce_InvalidSpecification_ThrowsBindingError(string expression)
        => Assert.That(() => Expression.Create(expression), Throws.Exception);

    [Test]
    public void Coerce_Constructors_SeparatePositionalTypesFromSelectorMappings()
    {
        var positional = new Coerce(typeof(int));
        var record = new RecordValue();
        record.Set("age", "42");
        var selective = new Coerce(new CoercionMapping(
            new FieldCoercionSelector("age"),
            typeof(int)));

        Assert.Multiple(() =>
        {
            Assert.That(positional.Evaluate("42"), Is.EqualTo(42));
            Assert.That(((RecordValue)selective.Evaluate(record)!)["age"], Is.EqualTo(42));
            Assert.That(
                typeof(Coerce).GetConstructors().Select(constructor => constructor.GetParameters().Single().ParameterType),
                Is.EquivalentTo(new[] { typeof(Type[]), typeof(CoercionMapping[]) }));
        });
    }

    [Test]
    public void Coerce_MissingRecordField_PreservesRecord()
        => Assert.That(
            ValueFormatter.Format(Expression.Create("coerce(missing -> :integer)").Evaluate(new RecordValue())),
            Is.EqualTo("{}"));

    [Test]
    public void Coerce_FailedConversion_ReturnsNull()
        => Assert.That(Expression.Create("coerce(:integer)").Evaluate("abc"), Is.Null);

    [TestCase("coerce(:integer, :text)", "42")]
    [TestCase("coerce(:integer)", "{name := \"Bob\"}")]
    [TestCase("coerce(name -> :text)", "T(\"Bob\", \"42\")")]
    public void Coerce_UnknownInputShape_InvalidAtEvaluation_ThrowsStructuralValidationError(
        string expression,
        string input)
    {
        var value = input.StartsWith("T(") || input.StartsWith('{')
            ? new ParameterValueConverter().Parse(input)
            : input;
        Assert.That(
            () => Expression.Create(expression).Evaluate(value),
            Throws.TypeOf<StructuralValidationException>());
    }

    [TestCase("\"42\" | coerce(:integer, :text)")]
    [TestCase("trim | coerce(:integer, :text)")]
    [TestCase("T(\"Bob\", \"42\") | coerce(name -> :text)")]
    [TestCase("{name := \"Bob\"} | coerce(:text)")]
    public void Coerce_KnownInputShape_InvalidAtBinding_ThrowsBindingError(string expression)
        => Assert.That(() => Expression.Create(expression), Throws.TypeOf<BindingException>());

    [TestCase(
        "T(\"Bob\", \"42\") | coerce(:text, :integer, :boolean)",
        "T(\"Bob\", 42)")]
    [TestCase(
        "T(\"Bob\", \"42\") | coerce($2 -> :integer)",
        "T(\"Bob\", \"42\")")]
    public void Coerce_UnavailableTuplePositions_AreIgnored(string expression, string expected)
        => Assert.That(
            ValueFormatter.Format(Expression.Create(expression).Evaluate(null)),
            Is.EqualTo(expected));

    [Test]
    public void Coerce_PairToTuple_MaterializesOrdinaryTuple()
        => Assert.That(
            Expression.CreateClosed("(\"USA\" => 42) | coerce(:tuple)").Evaluate(null),
            Is.TypeOf<Expressif.Values.Tuple>().And.EqualTo(new TupleValue("USA", 42m)));

    [Test]
    public void Coerce_GroupToTuple_UsesKeyAndValuesPositions()
        => Assert.That(
            new Coerce(typeof(TupleValue)).Evaluate(new Group("USA", new[] { 1, 2 })),
            Is.TypeOf<Expressif.Values.Tuple>().And.EqualTo(new TupleValue("USA", new[] { 1, 2 })));

    [Conformance]
    public void CoerceNumeric_Valid(object? value, decimal? expected)
        => Assert.That(new CoerceNumeric().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void CoerceInt_Valid(object? value, int? expected)
        => Assert.That(new CoerceInt().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void CoerceText_Valid(object? value, string? expected)
        => Assert.That(new CoerceText().Evaluate(value), Is.EqualTo(expected));

    [TestCase("")]
    [TestCase(" ")]
    [TestCase("\t\r\n")]
    public void CoerceText_EmptyOrWhitespaceString_PreservesValue(string value)
    {
        IFunction<string, string?> typed = new CoerceText();

        Assert.Multiple(() =>
        {
            Assert.That(new CoerceText().Evaluate(value), Is.EqualTo(value));
            Assert.That(typed.Evaluate(value), Is.EqualTo(value));
        });
    }

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
    public void CoerceTime_Valid_DateOnly(DateOnly value, TimeOnly expected)
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
        var function = Expression.Create(name, new Context());

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

    [Test]
    public void NumericCoercions_UseGenericMathForEverySupportedClrNumber()
    {
        AssertNumeric((byte)42, 42m);
        AssertNumeric((sbyte)-42, -42m);
        AssertNumeric((short)-42, -42m);
        AssertNumeric((ushort)42, 42m);
        AssertNumeric(42, 42m);
        AssertNumeric(42U, 42m);
        AssertNumeric(42L, 42m);
        AssertNumeric(42UL, 42m);
        AssertNumeric(42F, 42m);
        AssertNumeric(42D, 42m);
        AssertNumeric(42M, 42m);
    }

    [Test]
    public void IntegerCoercions_RequireLosslessGenericMathConversion()
    {
        AssertInteger((short)-42, -42);
        AssertInteger(42U, 42);
        AssertInteger(42L, 42);
        AssertInteger(42F, 42);
        AssertInteger(42D, 42);
        AssertInteger(42M, 42);
        AssertInteger(42.5D, null);
        AssertInteger(long.MaxValue, null);
    }

    [Test]
    public void BooleanAndTextCoercions_UseGenericMath()
    {
        AssertBoolean(0, false);
        AssertBoolean(-1L, true);
        AssertBoolean(0M, false);
        AssertBoolean(0.5D, true);
        AssertText((byte)42, "42");
        AssertText(-42L, "-42");
        AssertText(42.5M, "42.5");
    }

    [Test]
    public void TemporalTypedContracts_MatchObjectFallback()
    {
        AssertTyped<DateTime, DateOnly?>(new CoerceDate(), new DateTime(2026, 8, 19, 12, 30, 0));
        AssertTyped<YearMonth, DateOnly?>(new CoerceDate(), new YearMonth(2026, 8));
        AssertTyped<DateOnly, DateTime?>(new CoerceDateTime(), new DateOnly(2026, 8, 19));
        AssertTyped<YearMonth, DateTime?>(new CoerceDateTime(), new YearMonth(2026, 8));
        AssertTyped<DateOnly, TimeOnly?>(new CoerceTime(), new DateOnly(2026, 8, 19));
        AssertTyped<DateTime, TimeOnly?>(new CoerceTime(), new DateTime(2026, 8, 19, 12, 30, 0));
        AssertTyped<DateOnly, string?>(new CoerceText(), new DateOnly(2026, 8, 19));
    }

    private static void AssertNumeric<T>(T value, decimal? expected)
        where T : INumber<T>
    {
        IFunction<T, decimal?> typed = new CoerceNumeric<T>();
        Assert.That(typed.Evaluate(value), Is.EqualTo(expected));
        Assert.That(new CoerceNumeric().Evaluate(value), Is.EqualTo(expected));
    }

    private static void AssertInteger<T>(T value, int? expected)
        where T : INumber<T>
    {
        IFunction<T, int?> typed = new CoerceInt<T>();
        Assert.That(typed.Evaluate(value), Is.EqualTo(expected));
        Assert.That(new CoerceInt().Evaluate(value), Is.EqualTo(expected));
    }

    private static void AssertBoolean<T>(T value, bool? expected)
        where T : INumber<T>
    {
        IFunction<T, bool?> typed = new CoerceBoolean<T>();
        Assert.That(typed.Evaluate(value), Is.EqualTo(expected));
        Assert.That(new CoerceBoolean().Evaluate(value), Is.EqualTo(expected));
    }

    private static void AssertText<T>(T value, string? expected)
        where T : INumber<T>
    {
        IFunction<T, string?> typed = new CoerceText<T>();
        Assert.That(typed.Evaluate(value), Is.EqualTo(expected));
        Assert.That(new CoerceText().Evaluate(value), Is.EqualTo(expected));
    }

    private static void AssertTyped<TIn, TOut>(IFunction<TIn, TOut> typed, TIn value)
    {
        var fallback = (IFunction<object?, TOut>)(object)typed;
        Assert.That(typed.Evaluate(value), Is.EqualTo(fallback.Evaluate(value)));
    }
}
