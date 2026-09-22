using Expressif.Library.Text.Casing;
using Expressif.Library.Text.Character;
using Expressif.Library.Text.Concatenation;
using Expressif.Library.Text.Conversion;
using Expressif.Library.Text.Counting;
using Expressif.Library.Text.Encoding;
using Expressif.Library.Text.Filtering;
using Expressif.Library.Text.Masking;
using Expressif.Library.Text.Normalization;
using Expressif.Library.Text.Padding;
using Expressif.Library.Text.Partitioning;
using Expressif.Library.Text.Selection;
using Expressif.Library.Text.Tokenization;
using Expressif.Library.Temporal;
using Expressif.Library.Text;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Text.Character;

[TestFixture]
public class CharsTest
{
    [Conformance]
    public void Chars_Valid(string? value, string[]? expected)
        => Assert.That(new Chars().Evaluate(value), Is.EqualTo(expected));

    [Test]
    public void Evaluate_FollowedByChunk_PreservesFinalPartialChunk()
        => Assert.That(
            TestExpression.Create("chars | chunk(5)").Evaluate("1234567890123"),
            Is.EqualTo(new object[]
            {
                new[] { "1", "2", "3", "4", "5" },
                new[] { "6", "7", "8", "9", "0" },
                new[] { "1", "2", "3" },
            }));
}
