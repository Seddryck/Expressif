using Expressif.Values.Casters;

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
}
