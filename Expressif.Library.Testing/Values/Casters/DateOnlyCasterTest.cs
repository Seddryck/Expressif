using Expressif.Values.Casters;
using Expressif.Values;

namespace Expressif.Testing.Values.Casters;

[TestFixture]
public class DateOnlyCasterTest
{
    [TestCase("2026-08-19", true)]
    [TestCase("2026-08-19 00:00:00", true)]
    [TestCase("2026-08-19 12:30:00", false)]
    [TestCase("invalid", false)]
    public void TryCast_Text_ReturnsExpectedResult(string value, bool expected)
        => Assert.That(new DateOnlyCaster().TryCast(value, out _), Is.EqualTo(expected));

    [Test]
    public void TryCast_DateTime_ReturnsDateComponent()
    {
        Assert.That(new DateOnlyCaster().TryCast(new DateTime(2026, 8, 19, 12, 30, 0), out var value), Is.True);
        Assert.That(value, Is.EqualTo(new DateOnly(2026, 8, 19)));
    }

    [Test]
    public void TryCast_YearMonth_ReturnsFirstDay()
    {
        Assert.That(new DateOnlyCaster().TryCast(new YearMonth(2026, 8), out var value), Is.True);
        Assert.That(value, Is.EqualTo(new DateOnly(2026, 8, 1)));
    }
}
