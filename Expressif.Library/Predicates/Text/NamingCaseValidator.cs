using System.Text;
using Expressif.Functions.Text;

namespace Expressif.Predicates.Text;

internal static class NamingCaseValidator
{
    internal static bool IsSeparated(string value, char separator)
    {
        var first = true;
        var segmentStart = true;
        foreach (var rune in value.EnumerateRunes())
        {
            if (first && !Rune.IsLetter(rune))
                return false;
            first = false;
            if (rune.Value == separator)
            {
                if (segmentStart)
                    return false;
                segmentStart = true;
                continue;
            }

            if (!CaseWordTokenizer.IsWordRune(rune)
                || CaseWordTokenizer.IsUpper(rune)
                || (segmentStart && CaseWordTokenizer.IsMark(rune)))
                return false;
            segmentStart = false;
        }

        return !segmentStart;
    }

    internal static bool IsCased(string value, bool upperInitial)
    {
        var first = true;
        foreach (var rune in value.EnumerateRunes())
        {
            if (first && !(upperInitial ? CaseWordTokenizer.IsUpper(rune) : CaseWordTokenizer.IsLower(rune)))
                return false;
            if (!CaseWordTokenizer.IsWordRune(rune))
                return false;
            first = false;
        }

        return !first;
    }
}
