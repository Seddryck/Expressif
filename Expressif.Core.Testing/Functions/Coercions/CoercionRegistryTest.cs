using Expressif.Functions;
using Expressif.Functions.Coercions;
using Expressif.Discovery;
using Expressif.Library.Special;
using Expressif.Values;
using Expressif.Values.Casters;
using System.Numerics;

namespace Expressif.Testing.Functions.Coercions;

[TestFixture]
public class CoercionRegistryTest
{
    public static IEnumerable<TestCaseData> DeclaredPairs()
    {
        return new CoercionIntrospector(typeof(Coerce).Assembly).Describe().Select(info =>
            new TestCaseData(info.Name, info.SourceType, info.TargetType, info.ImplementationType)
                .SetName($"{info.Name}.{info.SourceType.Name}.{info.TargetType.Name}"));
    }

    [TestCaseSource(nameof(DeclaredPairs))]
    public void Registry_ClosesEveryDeclaredPair(
        string name,
        Type sourceType,
        Type targetType,
        Type implementationType)
    {
        var success = new CoercionRegistry(typeof(Coerce).Assembly).TryCreate(sourceType, targetType, out var function);
        var contract = typeof(IFunction<,>).MakeGenericType(sourceType, targetType);

        Assert.Multiple(() =>
        {
            Assert.That(success, Is.True);
            Assert.That(function, Is.TypeOf(implementationType));
            Assert.That(function.GetType().GetInterfaces(), Does.Contain(contract));
        });
    }

    [TestCase(typeof(BigInteger))]
    [TestCase(typeof(char))]
    [TestCase(typeof(DateTime))]
    public void Registry_RejectsUnsupportedNumericSources(Type sourceType)
        => Assert.That(
            new CoercionRegistry(typeof(Coerce).Assembly).TryCreate(sourceType, typeof(decimal?), out _),
            Is.False);

    [Test]
    public void Introspector_ReportsEveryResolvablePair()
    {
        var registry = new CoercionRegistry(typeof(Coerce).Assembly);
        var descriptions = new CoercionIntrospector(typeof(Coerce).Assembly).Describe();

        Assert.That(
            descriptions,
            Has.All.Matches<CoercionInfo>(info =>
                registry.TryResolve(info.SourceType, info.TargetType, out var name) && name == info.Name));
    }

    [Test]
    public void NumericDescriptor_ClosesGenericFunction()
    {
        Assert.That(new CoercionRegistry(typeof(Coerce).Assembly)
            .TryCreate(typeof(int), typeof(decimal?), out var function), Is.True);
        Assert.That(function, Is.TypeOf<CoerceNumeric<int>>());
    }

    [Test]
    public void UnsupportedGenericNumber_MatchesObjectFallback()
    {
        var value = new BigInteger(42);

        Assert.Multiple(() =>
        {
            Assert.That(new CoerceNumeric<BigInteger>().Evaluate(value), Is.Null);
            Assert.That(new CoerceNumeric().Evaluate(value), Is.Null);
        });
    }

    [Test]
    public void EveryNumericSource_HasTypedAndFallbackParity()
    {
        AssertAll((byte)42);
        AssertAll((sbyte)-42);
        AssertAll((short)-42);
        AssertAll((ushort)42);
        AssertAll(42);
        AssertAll(42U);
        AssertAll(42L);
        AssertAll(42UL);
        AssertAll(42F);
        AssertAll(42D);
        AssertAll(42M);
    }

    [Test]
    public void EveryClosedNonNumericPair_HasTypedAndFallbackParity()
    {
        AssertPair<bool, decimal?>(new CoerceNumeric(), true);
        AssertPair<string, decimal?>(new CoerceNumeric(), "42.5");
        AssertPair<bool, int?>(new CoerceInt(), false);
        AssertPair<string, int?>(new CoerceInt(), "42");
        AssertPair<OrderingValue, int?>(new CoerceInt(), OrderingValue.Less);
        AssertPair<OrderingValue, decimal?>(new CoerceNumeric(), OrderingValue.Greater);
        AssertPair<bool, bool?>(new CoerceBoolean(), true);
        AssertPair<string, bool?>(new CoerceBoolean(), "yes");
        AssertPair<string, string?>(new CoerceText(), "text");
        AssertPair<bool, string?>(new CoerceText(), true);
        AssertPair<DateOnly, string?>(new CoerceText(), new DateOnly(2026, 8, 22));
        AssertPair<DateTime, string?>(new CoerceText(), new DateTime(2026, 8, 22, 12, 30, 0));
        AssertPair<YearMonth, string?>(new CoerceText(), new YearMonth(2026, 8));
        AssertPair<DateOnly, DateOnly?>(new CoerceDate(), new DateOnly(2026, 8, 22));
        AssertPair<DateTime, DateOnly?>(new CoerceDate(), new DateTime(2026, 8, 22, 12, 30, 0));
        AssertPair<YearMonth, DateOnly?>(new CoerceDate(), new YearMonth(2026, 8));
        AssertPair<string, DateOnly?>(new CoerceDate(), "2026-08-22");
        AssertPair<TimeOnly, TimeOnly?>(new CoerceTime(), new TimeOnly(12, 30));
        AssertPair<DateOnly, TimeOnly?>(new CoerceTime(), new DateOnly(2026, 8, 22));
        AssertPair<DateTime, TimeOnly?>(new CoerceTime(), new DateTime(2026, 8, 22, 12, 30, 0));
        AssertPair<string, TimeOnly?>(new CoerceTime(), "12:30:00");
        AssertPair<DateTime, DateTime?>(new CoerceDateTime(), new DateTime(2026, 8, 22, 12, 30, 0));
        AssertPair<DateOnly, DateTime?>(new CoerceDateTime(), new DateOnly(2026, 8, 22));
        AssertPair<YearMonth, DateTime?>(new CoerceDateTime(), new YearMonth(2026, 8));
        AssertPair<string, DateTime?>(new CoerceDateTime(), "2026-08-22 12:30:00");
    }

    [Test]
    public void OrderingDescriptor_AcceptsOnlyNumericSources()
    {
        var sourceTypes = new CoercionIntrospector(typeof(Coerce).Assembly).Describe()
            .Where(info => info.Name == "coerce-ordering")
            .Select(info => info.SourceType);

        Assert.Multiple(() =>
        {
            Assert.That(sourceTypes, Is.EquivalentTo(NumericCoercion.SupportedSourceTypes
                .SelectMany(type => new[] { type, typeof(Nullable<>).MakeGenericType(type) })));
            Assert.That(sourceTypes, Does.Not.Contain(typeof(string))
                .And.Not.Contain(typeof(bool))
                .And.Not.Contain(typeof(DateOnly)));
        });
    }

    [TestCase("(null)")]
    [TestCase("")]
    [TestCase("   ")]
    public void TextStringContract_SpecialValuesMatchObjectFallback(string value)
    {
        IFunction<string, string?> typed = new CoerceText();
        IFunction<object?, string?> fallback = new CoerceText();

        Assert.That(typed.Evaluate(value), Is.EqualTo(fallback.Evaluate(value)));
    }

    [Test]
    public void NumericBoundaries_HaveTypedAndFallbackParity()
    {
        AssertPair<double, decimal?>(new CoerceNumeric<double>(), double.MaxValue, new CoerceNumeric());
        AssertPair<long, int?>(new CoerceInt<long>(), long.MaxValue, new CoerceInt());
        AssertPair<double, int?>(new CoerceInt<double>(), 42.7, new CoerceInt());
    }

    private static void AssertAll<T>(T value)
        where T : INumber<T>
    {
        AssertPair<T, decimal?>(new CoerceNumeric<T>(), value, new CoerceNumeric());
        AssertPair<T, int?>(new CoerceInt<T>(), value, new CoerceInt());
        AssertPair<T, bool?>(new CoerceBoolean<T>(), value, new CoerceBoolean());
        AssertPair<T, string?>(new CoerceText<T>(), value, new CoerceText());
    }

    private static void AssertPair<TIn, TOut>(IFunction<TIn, TOut> function, TIn value)
        => AssertPair(function, value, (IFunction<object?, TOut>)(object)function);

    private static void AssertPair<TIn, TOut>(
        IFunction<TIn, TOut> typed,
        TIn value,
        IFunction<object?, TOut> fallback)
        => Assert.That(typed.Evaluate(value), Is.EqualTo(fallback.Evaluate(value)));
}
