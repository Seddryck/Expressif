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

namespace Expressif.Testing.Text.Conversion;

public class CodePointFunctionsTest
{
    [TestCase("\uD800")]
    [TestCase("\uDC00")]
    public void CodePoint_InvalidUtf16_ReturnsNull(string value)
        => Assert.That(new CodePoint().Evaluate(value), Is.Null);

    [Conformance]
    public void CodePoint_Valid(object? value, int? expected)
        => Assert.That(new CodePoint().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void CodePoint_Invalid(object? value, int? expected)
        => Assert.That(new CodePoint().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void FromCodePoint_Valid(object? value, string? expected)
        => Assert.That(new FromCodePoint().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void FromCodePoint_Invalid(object? value, string? expected)
        => Assert.That(new FromCodePoint().Evaluate(value), Is.EqualTo(expected));
}
