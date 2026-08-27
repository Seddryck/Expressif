using System.Globalization;
using System.Text;
using Expressif.Values.Special;

namespace Expressif.Functions.Text;

/// <summary>
/// Returns a lowercase, separator-normalized slug, removing Latin diacritics without transliterating non-Latin scripts. Returns empty text when the input is `null`, empty, or blank.
/// </summary>
[Function(prefix: "", aliases: ["text-to-slug"])]
[Scope("text/normalization")]
public class Slug : BaseTextFunction
{
    protected override object EvaluateNull() => new Empty().Keyword;

    protected override object EvaluateBlank() => new Empty().Keyword;

    protected override object EvaluateString(string value)
    {
        var result = new StringBuilder(value.Length);
        var decomposed = value.Normalize(NormalizationForm.FormD);
        var appendSeparator = false;
        var previousBaseWasLatin = false;

        foreach (var rune in decomposed.EnumerateRunes())
        {
            var category = Rune.GetUnicodeCategory(rune);
            if (category is UnicodeCategory.NonSpacingMark
                or UnicodeCategory.SpacingCombiningMark
                or UnicodeCategory.EnclosingMark)
            {
                if (!previousBaseWasLatin && result.Length > 0 && !appendSeparator)
                    result.Append(rune);

                continue;
            }

            if (category is UnicodeCategory.UppercaseLetter
                or UnicodeCategory.LowercaseLetter
                or UnicodeCategory.TitlecaseLetter
                or UnicodeCategory.ModifierLetter
                or UnicodeCategory.OtherLetter
                or UnicodeCategory.DecimalDigitNumber
                or UnicodeCategory.LetterNumber
                or UnicodeCategory.OtherNumber)
            {
                if (appendSeparator && result.Length > 0)
                    result.Append('-');

                var lowercase = Rune.ToLowerInvariant(rune);
                result.Append(lowercase);
                appendSeparator = false;
                previousBaseWasLatin = IsLatin(lowercase);
                continue;
            }

            appendSeparator = result.Length > 0;
            previousBaseWasLatin = false;
        }

        return result.ToString().Normalize(NormalizationForm.FormC);
    }

    private static bool IsLatin(Rune rune)
        => rune.Value is >= 0x0041 and <= 0x024F
            or >= 0x1E00 and <= 0x1EFF
            or >= 0x2C60 and <= 0x2C7F
            or >= 0xA720 and <= 0xA7FF
            or >= 0xAB30 and <= 0xAB6F
            or >= 0xFF21 and <= 0xFF5A;
}
