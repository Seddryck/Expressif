using System.Buffers;
using System.Text;
using Expressif.Functions.Numeric;

namespace Expressif.Functions.Text;

/// <summary>
/// Returns the Unicode code point represented by a single Unicode scalar value. Returns `null` for any other input.
/// </summary>
[Function(prefix: "")]
[Scope("text/conversion")]
public sealed class CodePoint : BaseTextFunction<int?>
{
    protected override object? EvaluateNull() => null;
    protected override object? EvaluateEmpty() => null;
    protected override object? EvaluateBlank() => null;
    protected override object? EvaluateSpecial(string value) => null;

    protected override object? EvaluateString(string value)
    {
        var status = Rune.DecodeFromUtf16(value, out var rune, out var charsConsumed);
        return status == OperationStatus.Done && charsConsumed == value.Length
            ? rune.Value
            : null;
    }
}

/// <summary>
/// Returns the text corresponding to an integer Unicode scalar value. Returns `null` for any other input.
/// </summary>
[Function(prefix: "")]
[Scope("text/conversion")]
public sealed class FromCodePoint : FormatFunction
{
    protected override string? EvaluateNumeric(decimal numeric)
    {
        if (numeric != decimal.Truncate(numeric)
            || numeric < 0
            || numeric > 0x10FFFF
            || numeric is >= 0xD800 and <= 0xDFFF)
            return null;

        return Rune.TryCreate((int)numeric, out var rune) ? rune.ToString() : null;
    }
}
