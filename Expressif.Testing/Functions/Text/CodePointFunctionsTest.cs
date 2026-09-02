using Expressif.Functions.Text;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Functions.Text;

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
