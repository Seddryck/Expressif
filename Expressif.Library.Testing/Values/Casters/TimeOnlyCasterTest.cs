using Expressif.Values.Casters;

namespace Expressif.Testing.Values.Casters;

[TestFixture]
public class TimeOnlyCasterTest
{
    [TestCase("14:30:00", true)]
    [TestCase("00:00:00", true)]
    [TestCase("invalid", false)]
    public void TryCast_Text_ReturnsExpectedResult(string value, bool expected)
        => Assert.That(new TimeOnlyCaster().TryCast(value, out _), Is.EqualTo(expected));

    [Test]
    public void TryCast_DateTime_PreservesTimeOfDay()
    {
        var success = new TimeOnlyCaster().TryCast(new DateTime(2026, 8, 19, 14, 30, 0), out var value);

        Assert.Multiple(() =>
        {
            Assert.That(success, Is.True);
            Assert.That(value, Is.EqualTo(new TimeOnly(14, 30, 0)));
        });
    }

    [Test]
    public void TryCast_DateOnly_ReturnsMidnight()
    {
        var success = new TimeOnlyCaster().TryCast(new DateOnly(2026, 9, 1), out var value);

        Assert.Multiple(() =>
        {
            Assert.That(success, Is.True);
            Assert.That(value, Is.EqualTo(TimeOnly.MinValue));
        });
    }
}
