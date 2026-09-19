using System.Buffers;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Expressif.Functions;

namespace Expressif.Predicates.Text;

/// <summary>
/// Returns true when the input contains exactly one Unicode currency symbol. Returns false for null, empty, blank, and all other text.
/// </summary>
[Predicate(appendIs: false, prefix: "")]
public class IsCurrencySymbol : BaseTextPredicateWithoutReference, IFunction<string?, bool>
{
    public bool Evaluate(string? value) => value is not null && EvaluateBaseText(value);

    protected override bool EvaluateText(string value)
        => Rune.DecodeFromUtf16(value, out var rune, out var consumed) == OperationStatus.Done
            && consumed == value.Length
            && Rune.GetUnicodeCategory(rune) == UnicodeCategory.CurrencySymbol;
}

/// <summary>
/// Returns true when the trimmed input is an amount with one Unicode currency symbol at the beginning or end, optional comma grouping, and an optional dot decimal fraction. Returns false for null, empty, blank, and malformed amounts.
/// </summary>
[Predicate(appendIs: false, prefix: "")]
public class MatchesCurrency : BaseTextPredicateWithoutReference, IFunction<string?, bool>
{
    private static readonly Regex AmountPattern = new(
        @"\A[+-]?(?:[0-9]+|[0-9]{1,3}(?:,[0-9]{3})+)(?:\.[0-9]+)?\z",
        RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);

    public bool Evaluate(string? value) => value is not null && EvaluateBaseText(value);

    protected override bool EvaluateText(string value)
    {
        var text = value.AsSpan().Trim();
        if (Rune.DecodeFromUtf16(text, out var first, out var prefixLength) == OperationStatus.Done
            && Rune.GetUnicodeCategory(first) == UnicodeCategory.CurrencySymbol)
            text = text[prefixLength..].Trim();
        else if (Rune.DecodeLastFromUtf16(text, out var last, out var suffixLength) == OperationStatus.Done
            && Rune.GetUnicodeCategory(last) == UnicodeCategory.CurrencySymbol)
            text = text[..^suffixLength].Trim();
        else
            return false;

        return AmountPattern.IsMatch(text);
    }
}
