using Expressif.Functions.Numeric;

namespace Expressif.Testing.Functions.Numeric;

[TestFixture]
public class FormatFunctionTest
{
    [Test]
    [TestCase(1048576, "1.05 M")]
    [TestCase(1000, "1.00 k")]
    [TestCase(999, "999")]
    [TestCase(0, "0")]
    [TestCase(-1048576, "-1.05 M")]
    public void HumanReadableFormatDecimal_DefaultPrecision_Valid(decimal value, string expected)
        => Assert.That(new HumanReadableFormatDecimal().Evaluate(value), Is.EqualTo(expected));

    [Test]
    [TestCase(1048576, 0, "1 M")]
    [TestCase(1048576, 3, "1.049 M")]
    [TestCase(999, 2, "999")]
    [TestCase(1000, 0, "1 k")]
    public void HumanReadableFormatDecimal_Precision_Valid(decimal value, int precision, string expected)
        => Assert.That(new HumanReadableFormatDecimal(() => precision).Evaluate(value), Is.EqualTo(expected));

    [Test]
    [TestCase(1048576, "1.05 MB")]
    [TestCase(1000, "1.00 KB")]
    [TestCase(999, "999 B")]
    [TestCase(0, "0 B")]
    [TestCase(-1048576, "-1.05 MB")]
    public void HumanReadableFormatDecimalBytes_DefaultPrecision_Valid(decimal value, string expected)
        => Assert.That(new HumanReadableFormatDecimalBytes().Evaluate(value), Is.EqualTo(expected));

    [Test]
    [TestCase(1048576, 0, "1 MB")]
    [TestCase(1048576, 3, "1.049 MB")]
    [TestCase(1000, 0, "1 KB")]
    public void HumanReadableFormatDecimalBytes_Precision_Valid(decimal value, int precision, string expected)
        => Assert.That(new HumanReadableFormatDecimalBytes(() => precision).Evaluate(value), Is.EqualTo(expected));

    [Test]
    [TestCase(1048576, "1.00 MiB")]
    [TestCase(1024, "1.00 KiB")]
    [TestCase(1000, "1000 B")]
    [TestCase(1536, "1.50 KiB")]
    [TestCase(0, "0 B")]
    [TestCase(-1048576, "-1.00 MiB")]
    public void HumanReadableFormatBinaryBytes_DefaultPrecision_Valid(decimal value, string expected)
        => Assert.That(new HumanReadableFormatBinaryBytes().Evaluate(value), Is.EqualTo(expected));

    [Test]
    [TestCase(1536, 0, "2 KiB")]
    [TestCase(1048576, 3, "1.000 MiB")]
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
