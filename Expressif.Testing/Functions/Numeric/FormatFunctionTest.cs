using Expressif.Functions.Numeric;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Functions.Numeric;

[TestFixture]
public class FormatFunctionTest
{
    [Conformance]
    public void HumanReadableFormatDecimal_DefaultPrecision_Valid(decimal value, string expected)
        => Assert.That(new HumanReadableFormatDecimal().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void HumanReadableFormatDecimal_Precision_Valid(decimal value, int precision, string expected)
        => Assert.That(new HumanReadableFormatDecimal(() => precision).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void HumanReadableFormatDecimalBytes_DefaultPrecision_Valid(decimal value, string expected)
        => Assert.That(new HumanReadableFormatDecimalBytes().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void HumanReadableFormatDecimalBytes_Precision_Valid(decimal value, int precision, string expected)
        => Assert.That(new HumanReadableFormatDecimalBytes(() => precision).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void HumanReadableFormatBinaryBytes_DefaultPrecision_Valid(decimal value, string expected)
        => Assert.That(new HumanReadableFormatBinaryBytes().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void HumanReadableFormatBinaryBytes_Precision_Valid(decimal value, int precision, string expected)
        => Assert.That(new HumanReadableFormatBinaryBytes(() => precision).Evaluate(value), Is.EqualTo(expected));

    [Test]
    [TestCase(-1)]
    [TestCase(4)]
    public void HumanReadableFormat_InvalidPrecision_ReturnsNull(int precision)
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
            new Expressif.Values.Special.Null(),
            new Expressif.Values.Special.Empty(),
            new Expressif.Values.Special.Whitespace(),
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
}
