using System.Collections;
using Expressif.Values.Casters;
using Expressif.Values.Special;

namespace Expressif.Functions.Text;

/// <summary>
/// Base class for character-by-character text casing transformations. For these functions, `null`, `DBNull`, and `(null)` return `null`; `(empty)` returns `(empty)`; and blank values return `(blank)`.
/// </summary>
public abstract class BaseTextCasing : BaseTextFunction
{
    protected static string[] SplitWordsBySpace(string value)
        => string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split(' ', StringSplitOptions.RemoveEmptyEntries);

    protected static string CapitalizeWord(string word)
        => string.IsNullOrEmpty(word)
            ? string.Empty
            : char.ToUpperInvariant(word[0]) + (word.Length > 1 ? word[1..].ToLowerInvariant() : string.Empty);

    protected static bool ShouldPreserveWordCasing(string word)
        => word.Contains('.')
            || word.Contains('&')
            || word.Skip(1).Any(char.IsUpper);

    protected override object? EvaluateArray(IEnumerable? array)
    {
        if (array == null || !array.Cast<object>().Any())
            return null;

        if (array is IEnumerable<string?> stringArray)
        {
            var nonNullWords = stringArray.Where(x => !string.IsNullOrWhiteSpace(x)).Cast<string>();
            if (!nonNullWords.Any())
                return new Empty();
            return EvaluateString(string.Join(" ", nonNullWords));
        }
        var caster = new TextCaster();
        return EvaluateString(string.Join(" ", (array.Cast<object>().Select(caster.Cast))));
    }

    protected override object? EvaluateNull() => null;
}

/// <summary>
/// Returns the input text converted to lowercase using invariant culture rules. Returns `null` when the input is `null`, `DBNull`, `(null)`, or a zero-length array.
/// </summary>
public class Lower : BaseTextCasing
{
    protected override object EvaluateString(string value) => value.ToLowerInvariant();
}

/// <summary>
/// Returns the input text converted to uppercase using invariant culture rules. Returns `null` when the input is `null`, `DBNull`, `(null)`, or a zero-length array.
/// </summary>
public class Upper : BaseTextCasing
{
    protected override object EvaluateString(string value) => value.ToUpperInvariant();
}

/// <summary>
/// Returns the input text with lowercase characters converted to uppercase and uppercase characters converted to lowercase. Returns `null` when the input is `null`, `DBNull`, `(null)`, or a zero-length array.
/// </summary>
[Function(prefix: "", aliases: ["text-to-swap-case"])]
public class SwapCase : BaseTextCasing
{
    protected override object EvaluateString(string value)
        => string.Concat(value.Select(c => char.IsLetter(c)
            ? (char.IsUpper(c) ? char.ToLowerInvariant(c) : char.ToUpperInvariant(c))
            : c));
}

/// <summary>
/// Returns the input text in sentence case by capitalizing the first word while preserving the remaining content. Words containing dots, ampersands, or uppercase letters beyond the first character are treated as already correctly cased and preserved as-is (for example `example.com`, `AT&amp;T`, and `iTunes`). Returns `null` when the input is `null`, `DBNull`, `(null)`, or a zero-length array.
/// </summary>
[Function(prefix: "", aliases: ["text-to-sentence-case", "capitalize"])]
public class SentenceCase : BaseTextCasing
{
    protected override object EvaluateString(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var firstWordStart = 0;
        while (firstWordStart < value.Length && char.IsWhiteSpace(value[firstWordStart]))
            firstWordStart++;

        if (firstWordStart >= value.Length)
            return value;

        var firstWordEnd = firstWordStart;
        while (firstWordEnd < value.Length && !char.IsWhiteSpace(value[firstWordEnd]))
            firstWordEnd++;

        var firstWord = value[firstWordStart..firstWordEnd];
        if (ShouldPreserveWordCasing(firstWord))
            return value;

        var capitalized = CapitalizeWord(firstWord);
        return value[..firstWordStart] + capitalized + value[firstWordEnd..];
    }
}

/// <summary>
/// Returns the input text in title case, capitalizing words while keeping small words lowercase only when they are neither first nor last and do not follow a colon. The first and last words are always capitalized, and a small word after a colon is capitalized. Words containing dots, ampersands, or uppercase letters beyond the first character are treated as already correctly cased and preserved as-is (for example `example.com`, `Q&amp;A`, and `iTunes`). Returns `null` when the input is `null`, `DBNull`, `(null)`, or a zero-length array.
/// </summary>
[Function(prefix: "", aliases: ["text-to-title-case"])]
public class TitleCase : BaseTextCasing
{
    private static readonly string[] Default_Small_Words = ["a", "an", "and", "as", "at", "but", "by", "en", "for", "if", "in", "of", "on", "or", "the", "to", "vs"];

    protected override object EvaluateString(string value)
    {
        var words = SplitWordsBySpace(value);
        return string.Join(" ", BuildTitleCaseWords(words));
    }

    private static IEnumerable<string> BuildTitleCaseWords(string[] words)
    {
        for (var i = 0; i < words.Length; i++)
        {
            var word = words[i];
            if (ShouldPreserveWordCasing(word))
            {
                yield return word;
                continue;
            }

            var lower = word.ToLowerInvariant();
            var isFirstWord = i == 0;
            var isLastWord = i == words.Length - 1;
            var isWordAfterColon = i > 0 && words[i - 1].EndsWith(':');

            if (!isFirstWord
                && !isLastWord
                && !isWordAfterColon
                && Default_Small_Words.Contains(lower, StringComparer.Ordinal))
                yield return lower;
            else
                yield return CapitalizeWord(word);
        }
    }
}

/// <summary>
/// Base class for word-based casing transformations. For these functions, `null`, `(null)`, `(blank)`, blank strings, and empty inputs all return `(empty)`.
/// </summary>
public abstract class BaseTextWordCasing : BaseTextCasing
{
    protected override object? EvaluateBlank() => new Empty();
    protected override object? EvaluateNull() => new Empty();

    protected override object? EvaluateString(string value)
        => EvaluateArray(SplitWordsBySpace(value));

    protected override object? EvaluateArray(IEnumerable? array)
    {
        if (array == null || !array.Cast<object>().Any())
            return new Empty();

        if (array is IEnumerable<string?> stringArray)
        {
            var nonNullWords = stringArray.Where(x => !string.IsNullOrWhiteSpace(x)).Cast<string>();
            if (!nonNullWords.Any())
                return new Empty();
            return EvaluateArrayString(nonNullWords);
        }
        var caster = new TextCaster();
        return EvaluateArrayString(array.Cast<object>().Select(caster.Cast));
    }

    protected abstract string EvaluateArrayString(IEnumerable<string> words);
}

/// <summary>
/// Returns the input text in PascalCase, capitalizing each word and removing separators. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array.
/// </summary>
[Function(prefix: "", aliases: ["text-to-pascal-case"])]
public class PascalCase : BaseTextWordCasing
{
    protected override string EvaluateArrayString(IEnumerable<string> words)
        => string.Concat(words.Select(CapitalizeWord));
}

/// <summary>
/// Returns the input text in camelCase, lowercasing the first word and capitalizing subsequent words without separators. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array.
/// </summary>
[Function(prefix: "", aliases: ["text-to-camel-case"])]
public class CamelCase : BaseTextWordCasing
{
    protected override string EvaluateArrayString(IEnumerable<string> words)
    {
        var first = words.First().ToLowerInvariant();
        var rest = words.Skip(1).Select(CapitalizeWord);
        return first + string.Concat(rest);
    }
}

/// <summary>
/// Returns the input text in kebab-case, lowercasing words and joining them with hyphens. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array.
/// </summary>
[Function(prefix: "", aliases: ["text-to-kebab-case"])]
public class KebabCase : BaseTextWordCasing
{
    protected override string EvaluateArrayString(IEnumerable<string> words)
        => string.Join("-", words.Select(x => x.ToLowerInvariant()));
}

/// <summary>
/// Returns the input text in snake_case, lowercasing words and joining them with underscores. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array.
/// </summary>
[Function(prefix: "", aliases: ["text-to-snake-case"])]
public class SnakeCase : BaseTextWordCasing
{
    protected override string EvaluateArrayString(IEnumerable<string> words)
        => string.Join("_", words.Select(x => x.ToLowerInvariant()));
}

/// <summary>
/// Returns the input text in camel_Snake case, lowercasing the first word, capitalizing subsequent words, and joining them with underscores. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array.
/// </summary>
[Function(prefix: "", aliases: ["text-to-camel-snake-case"])]
public class CamelSnakeCase : BaseTextWordCasing
{
    protected override string EvaluateArrayString(IEnumerable<string> words)
    {
        var first = words.First().ToLowerInvariant();
        var rest = words.Skip(1).Select(CapitalizeWord);
        return string.Join("_", new[] { first }.Concat(rest));
    }
}

/// <summary>
/// Returns the input text in Pascal_Snake case, capitalizing each word and joining them with underscores. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array.
/// </summary>
[Function(prefix: "", aliases: ["text-to-pascal-snake-case"])]
public class PascalSnakeCase : BaseTextWordCasing
{
    protected override string EvaluateArrayString(IEnumerable<string> words)
        => string.Join("_", words.Select(CapitalizeWord));
}

/// <summary>
/// Returns the input text in dot.case, lowercasing words and joining them with periods. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array.
/// </summary>
[Function(prefix: "", aliases: ["text-to-dot-case"])]
public class DotCase : BaseTextWordCasing
{
    protected override string EvaluateArrayString(IEnumerable<string> words)
        => string.Join(".", words.Select(x => x.ToLowerInvariant()));
}

/// <summary>
/// Returns the input text in SCREAMING_SNAKE_CASE, uppercasing words and joining them with underscores. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array.
/// </summary>
[Function(prefix: "", aliases: ["text-to-screaming-snake-case"])]
public class ScreamingSnakeCase : BaseTextWordCasing
{
    protected override string EvaluateArrayString(IEnumerable<string> words)
        => string.Join("_", words.Select(x => x.ToUpperInvariant()));
}

/// <summary>
/// Returns the input text in Train-Case, capitalizing each word and joining them with hyphens. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array.
/// </summary>
[Function(prefix: "", aliases: ["text-to-train-case"])]
public class TrainCase : BaseTextWordCasing
{
    protected override string EvaluateArrayString(IEnumerable<string> words)
        => string.Join("-", words.Select(CapitalizeWord));
}

/// <summary>
/// Returns the input text in flatcase, lowercasing words and concatenating them without separators. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array.
/// </summary>
[Function(prefix: "", aliases: ["text-to-flat-case"])]
public class FlatCase : BaseTextWordCasing
{
    protected override string EvaluateArrayString(IEnumerable<string> words)
        => string.Concat(words.Select(x => x.ToLowerInvariant()));
}

/// <summary>
/// Returns the input text in ALLCAPS case, uppercasing words and concatenating them without separators. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array.
/// </summary>
[Function(prefix: "", aliases: ["text-to-allcaps-case"])]
public class AllcapsCase : BaseTextWordCasing
{
    protected override string EvaluateArrayString(IEnumerable<string> words)
        => string.Concat(words.Select(x => x.ToUpperInvariant()));
}

/// <summary>
/// Returns the input text in COBOL-CASE, uppercasing words and joining them with hyphens. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array.
/// </summary>
[Function(prefix: "", aliases: ["text-to-cobol-case"])]
public class CobolCase : BaseTextWordCasing
{
    protected override string EvaluateArrayString(IEnumerable<string> words)
        => string.Join("-", words.Select(x => x.ToUpperInvariant()));
}

/// <summary>
/// Returns the input text in path/case, lowercasing words and joining them with slashes. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array.
/// </summary>
[Function(prefix: "", aliases: ["text-to-path-case"])]
public class PathCase : BaseTextWordCasing
{
    protected override string EvaluateArrayString(IEnumerable<string> words)
        => string.Join("/", words.Select(x => x.ToLowerInvariant()));
}

/// <summary>
/// Returns the input text in namespace::case, lowercasing words and joining them with double colons. Returns empty text when the input is `null`, `empty`, `blank`, or a zero-length array.
/// </summary>
[Function(prefix: "", aliases: ["text-to-namespace-case"])]
public class NamespaceCase : BaseTextWordCasing
{
    protected override string EvaluateArrayString(IEnumerable<string> words)
        => string.Join("::", words.Select(x => x.ToLowerInvariant()));
}
