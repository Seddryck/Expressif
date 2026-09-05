using Expressif.Functions.Text;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Functions.Text;

[TestFixture]
public class CharsTest
{
    [Conformance]
    public void Chars_Valid(string? value, string[]? expected)
        => Assert.That(new Chars().Evaluate(value), Is.EqualTo(expected));

    [Test]
    public void Evaluate_FollowedByChunk_PreservesFinalPartialChunk()
        => Assert.That(
            Expression.Create("chars | chunk(5)").Evaluate("1234567890123"),
            Is.EqualTo(new object[]
            {
                new[] { "1", "2", "3", "4", "5" },
                new[] { "6", "7", "8", "9", "0" },
                new[] { "1", "2", "3" },
            }));
}
