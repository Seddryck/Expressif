using Expressif.Library.Numeric.Arithmetic;
using Expressif.Library.Numeric.Classification;
using Expressif.Library.Numeric.Conversion;
using Expressif.Library.Numeric.Formatting;
using Expressif.Library.Numeric.Rounding;
using Expressif.Library.IO;
using Expressif.Library.Numeric;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Numeric.Formatting;

[TestFixture]
public class FormatFunctionTest
{
    [Conformance]
    public void HumanReadableFormatDecimal_Valid_DefaultPrecision(decimal value, string expected)
        => Assert.That(new HumanReadableFormatDecimal().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void HumanReadableFormatDecimal_Valid_Precision(decimal value, int precision, string expected)
        => Assert.That(new HumanReadableFormatDecimal(() => precision).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void HumanReadableFormatDecimalBytes_Valid_DefaultPrecision(decimal value, string expected)
        => Assert.That(new HumanReadableFormatDecimalBytes().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void HumanReadableFormatDecimalBytes_Valid_Precision(decimal value, int precision, string expected)
        => Assert.That(new HumanReadableFormatDecimalBytes(() => precision).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void HumanReadableFormatBinaryBytes_Valid_DefaultPrecision(decimal value, string expected)
        => Assert.That(new HumanReadableFormatBinaryBytes().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void HumanReadableFormatBinaryBytes_Valid_Precision(decimal value, int precision, string expected)
        => Assert.That(new HumanReadableFormatBinaryBytes(() => precision).Evaluate(value), Is.EqualTo(expected));

    [Test]
    [TestCase(-1)]
    [TestCase(4)]
    public void HumanReadableFormat_ReturnsNull_InvalidPrecision(int precision)
    {
        Assert.That(new HumanReadableFormatDecimal(() => precision).Evaluate(1000), Is.Null);
        Assert.That(new HumanReadableFormatDecimalBytes(() => precision).Evaluate(1000), Is.Null);
        Assert.That(new HumanReadableFormatBinaryBytes(() => precision).Evaluate(1024), Is.Null);
    }

    [Test]
    public void FormatFunctions_SentinelValues_ReturnNull()
    {
        object?[] sentinels =
        [
            null,
            DBNull.Value,
            Expressif.Values.Special.Null.Instance,
            Expressif.Values.Special.Empty.Instance,
            Expressif.Values.Special.Whitespace.Instance,
            "(null)",
            "(empty)",
            "(blank)"
        ];

        foreach (var sentinel in sentinels)
        {
            var sentinelLabel = sentinel ?? "null";
            using (Assert.EnterMultipleScope())
            {
                Assert.That(new HumanReadableFormatDecimal().Evaluate(sentinel), Is.Null, $"Failed for HumanReadableFormatDecimal with sentinel '{sentinelLabel}'");
                Assert.That(new HumanReadableFormatDecimalBytes().Evaluate(sentinel), Is.Null, $"Failed for HumanReadableFormatDecimalBytes with sentinel '{sentinelLabel}'");
                Assert.That(new HumanReadableFormatBinaryBytes().Evaluate(sentinel), Is.Null, $"Failed for HumanReadableFormatBinaryBytes with sentinel '{sentinelLabel}'");
            }
        }
    }

    [Conformance]
    public void HumanReadableFormatDecimal_Invalid(object? value, string? expected)
        => Assert.That(new HumanReadableFormatDecimal().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void HumanReadableFormatDecimalBytes_Invalid(object? value, string? expected)
        => Assert.That(new HumanReadableFormatDecimalBytes().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void HumanReadableFormatBinaryBytes_Invalid(object? value, string? expected)
        => Assert.That(new HumanReadableFormatBinaryBytes().Evaluate(value), Is.EqualTo(expected));
}
