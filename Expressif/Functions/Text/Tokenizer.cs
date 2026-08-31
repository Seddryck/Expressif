using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Functions.Text;

internal interface ITokenizer
{
    string[] Execute(string value);
}

internal class Tokenizer : ITokenizer
{
    private char Separator { get; }
    public Tokenizer(char separator)
        => Separator = separator;

    public string[] Execute(string value) => value.Split(new char[] { Separator }, StringSplitOptions.RemoveEmptyEntries);
}

class WhitespaceTokenizer : ITokenizer
{
    public string[] Execute(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            var startTokens = new List<int>();
            var endTokens = new List<int>();
            bool tokenRunning = false;

            for (int i = 0; i < value.Length; i++)
            {
                if (char.IsLetterOrDigit(value[i]) || char.Parse("-") == value[i])
                {
                    if (!tokenRunning)
                        startTokens.Add(i);
                    tokenRunning = true;
                }
                else if (char.IsWhiteSpace(value[i]))
                {
                    if (tokenRunning)
                        endTokens.Add(i);
                    tokenRunning = false;
                }
            }
            if (tokenRunning)
                endTokens.Add(value.Length);

            var tokens = new List<string>();
            var boundedTokens = startTokens.Zip(endTokens, (start, end) => new { Start = start, End = end });
            foreach (var tokenBoundary in boundedTokens)
            {
                var substring = value[tokenBoundary.Start..tokenBoundary.End];
                if (!string.IsNullOrWhiteSpace(substring))
                    tokens.Add(substring.Trim());
            }
            return tokens.ToArray();
        }
        else
        {
            return [];
        }
    }
}

internal class LexicalTokenizer : ITokenizer
{
    public string[] Execute(string value)
    {
        var runes = value.EnumerateRunes().ToArray();
        var tokens = new List<string>();
        var current = new StringBuilder();

        for (var index = 0; index < runes.Length; index++)
        {
            var rune = runes[index];
            if (IsWordRune(rune))
            {
                current.Append(rune.ToString());
                continue;
            }

            var isInternalApostrophe = (rune.Value == '\'' || rune.Value == '’')
                && current.Length > 0
                && index + 1 < runes.Length
                && IsWordRune(runes[index + 1]);
            if (isInternalApostrophe)
            {
                current.Append(rune.ToString());
                continue;
            }

            FlushCurrent(tokens, current);
            if (!IsIgnored(rune))
                tokens.Add(rune.ToString());
        }

        FlushCurrent(tokens, current);
        return tokens.ToArray();
    }

    private static bool IsWordRune(Rune rune)
        => Rune.GetUnicodeCategory(rune) is UnicodeCategory.UppercaseLetter
            or UnicodeCategory.LowercaseLetter
            or UnicodeCategory.TitlecaseLetter
            or UnicodeCategory.ModifierLetter
            or UnicodeCategory.OtherLetter
            or UnicodeCategory.NonSpacingMark
            or UnicodeCategory.SpacingCombiningMark
            or UnicodeCategory.EnclosingMark
            or UnicodeCategory.DecimalDigitNumber
            or UnicodeCategory.LetterNumber
            or UnicodeCategory.OtherNumber;

    private static bool IsIgnored(Rune rune)
        => Rune.GetUnicodeCategory(rune) is UnicodeCategory.SpaceSeparator
            or UnicodeCategory.LineSeparator
            or UnicodeCategory.ParagraphSeparator
            or UnicodeCategory.Control
            or UnicodeCategory.Format;

    private static void FlushCurrent(List<string> tokens, StringBuilder current)
    {
        if (current.Length == 0)
            return;

        tokens.Add(current.ToString());
        current.Clear();
    }
}

internal class SeparatedWordTokenizer(char separator) : ITokenizer
{
    public string[] Execute(string value)
    {
        var tokens = new List<string>();
        var current = new StringBuilder();
        var runes = value.EnumerateRunes().ToArray();

        for (var index = 0; index < runes.Length; index++)
        {
            if (runes[index].Value == '\\' && index + 1 < runes.Length && runes[index + 1].Value == separator)
            {
                current.Append(runes[++index].ToString());
                continue;
            }

            if (runes[index].Value == separator)
                FlushCurrent(tokens, current);
            else
                current.Append(runes[index].ToString());
        }

        FlushCurrent(tokens, current);
        return tokens.ToArray();
    }

    private static void FlushCurrent(List<string> tokens, StringBuilder current)
    {
        if (current.Length == 0)
            return;

        tokens.Add(current.ToString());
        current.Clear();
    }
}

internal class CaseWordTokenizer(bool discardBoundaries, bool preserveApostrophes, bool respectEscapes) : ITokenizer
{
    public string[] Execute(string value)
    {
        var tokens = new List<string>();
        var current = new StringBuilder();
        var runes = value.EnumerateRunes().ToArray();
        Rune? previousWordRune = null;

        for (var index = 0; index < runes.Length; index++)
        {
            var rune = runes[index];
            if (respectEscapes && rune.Value == '\\' && index + 1 < runes.Length && !IsWordRune(runes[index + 1]))
            {
                current.Append(runes[++index].ToString());
                previousWordRune = null;
                continue;
            }

            if (IsWordRune(rune))
            {
                if (StartsCaseBoundary(previousWordRune, rune, runes, index))
                    FlushCurrent(tokens, current);

                current.Append(rune.ToString());
                if (!IsMark(rune))
                    previousWordRune = rune;
                continue;
            }

            var isInternalApostrophe = preserveApostrophes
                && (rune.Value == '\'' || rune.Value == '’')
                && current.Length > 0
                && index + 1 < runes.Length
                && IsWordRune(runes[index + 1]);
            if (isInternalApostrophe)
            {
                current.Append(rune.ToString());
                previousWordRune = null;
                continue;
            }

            FlushCurrent(tokens, current);
            previousWordRune = null;
            if (!discardBoundaries)
                current.Append(rune.ToString());
        }

        FlushCurrent(tokens, current);
        return tokens.ToArray();
    }

    private static bool StartsCaseBoundary(Rune? previous, Rune current, Rune[] runes, int index)
    {
        if (previous is null || !IsUpper(current))
            return false;

        if (IsLower(previous.Value) || IsNumber(previous.Value))
            return true;

        return IsUpper(previous.Value)
            && index + 1 < runes.Length
            && IsLower(runes[index + 1]);
    }

    private static bool IsWordRune(Rune rune)
        => Rune.GetUnicodeCategory(rune) is UnicodeCategory.UppercaseLetter
            or UnicodeCategory.LowercaseLetter
            or UnicodeCategory.TitlecaseLetter
            or UnicodeCategory.ModifierLetter
            or UnicodeCategory.OtherLetter
            or UnicodeCategory.NonSpacingMark
            or UnicodeCategory.SpacingCombiningMark
            or UnicodeCategory.EnclosingMark
            or UnicodeCategory.DecimalDigitNumber
            or UnicodeCategory.LetterNumber
            or UnicodeCategory.OtherNumber;

    private static bool IsUpper(Rune rune)
        => Rune.GetUnicodeCategory(rune) is UnicodeCategory.UppercaseLetter or UnicodeCategory.TitlecaseLetter;

    private static bool IsLower(Rune rune)
        => Rune.GetUnicodeCategory(rune) == UnicodeCategory.LowercaseLetter;

    private static bool IsNumber(Rune rune)
        => Rune.GetUnicodeCategory(rune) is UnicodeCategory.DecimalDigitNumber
            or UnicodeCategory.LetterNumber
            or UnicodeCategory.OtherNumber;

    private static bool IsMark(Rune rune)
        => Rune.GetUnicodeCategory(rune) is UnicodeCategory.NonSpacingMark
            or UnicodeCategory.SpacingCombiningMark
            or UnicodeCategory.EnclosingMark;

    private static void FlushCurrent(List<string> tokens, StringBuilder current)
    {
        if (current.Length == 0)
            return;

        tokens.Add(current.ToString());
        current.Clear();
    }
}
