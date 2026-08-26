using Expressif.Cli.Inputs;
using PocketCsvReader;

namespace Expressif.Cli.Infrastructure;

internal sealed class CsvSourceProfileBuilder(IInputValueParser values)
{
    private delegate void ApplyOption(CsvDialectSettings settings, object? value, string name);

    private static readonly IReadOnlyDictionary<string, ApplyOption> OptionDefinitions =
        new Dictionary<string, ApplyOption>(StringComparer.Ordinal)
        {
            ["delimiter"] = static (settings, value, name) => settings.Delimiter = RequiredChar(value, name),
            ["line-terminator"] = static (settings, value, name) => settings.LineTerminator = RequiredText(value, name),
            ["quote-char"] = static (settings, value, name) => settings.QuoteChar = OptionalChar(value, name),
            ["double-quote"] = static (settings, value, name) => settings.DoubleQuote = RequiredBoolean(value, name),
            ["escape-char"] = static (settings, value, name) => settings.EscapeChar = OptionalChar(value, name),
            ["header"] = static (settings, value, name) => settings.SetHeader(RequiredBoolean(value, name)),
            ["header-rows"] = static (settings, value, name) => settings.HeaderRows = RequiredRows(value, name),
            ["header-join"] = static (settings, value, name) => settings.HeaderJoin = RequiredText(value, name),
            ["header-repeat"] = static (settings, value, name) => settings.HeaderRepeat = RequiredBoolean(value, name),
            ["comment-char"] = static (settings, value, name) => settings.CommentChar = OptionalChar(value, name),
            ["comment-rows"] = static (settings, value, name) => settings.CommentRows = RequiredRows(value, name),
            ["null-sequence"] = static (settings, value, name) => settings.NullSequence = RequiredText(value, name),
            ["missing-cell"] = static (settings, value, name) => settings.MissingCell = RequiredText(value, name),
            ["skip-initial-space"] = static (settings, value, name) => settings.SkipInitialSpace = RequiredBoolean(value, name),
            ["array-delimiter"] = static (settings, value, name) => settings.ArrayDelimiter = OptionalChar(value, name),
            ["array-prefix"] = static (settings, value, name) => settings.ArrayPrefix = OptionalChar(value, name),
            ["array-suffix"] = static (settings, value, name) => settings.ArraySuffix = OptionalChar(value, name),
        };

    private static readonly string ValidOptions = string.Join(", ", OptionDefinitions.Keys);

    public (CsvProfile Profile, bool HeadersAreRows) Build(IReadOnlyList<string> options)
    {
        var settings = CsvDialectSettings.CreateDefault();
        foreach (var text in options)
            Apply(settings, Parse(text));
        return settings.Build();
    }

    private static void Apply(CsvDialectSettings settings, CsvSourceOption option)
    {
        if (!OptionDefinitions.TryGetValue(option.Name, out var apply))
        {
            throw new FormatException(
                $"Unknown CSV source option '{option.Name}' with value '{option.SuppliedValue}'. " +
                $"Valid source options: {ValidOptions}.");
        }

        try
        {
            apply(settings, option.Value, option.Name);
        }
        catch (FormatException exception)
        {
            throw InvalidSourceOption(option.Name, option.SuppliedValue, exception.Message);
        }
    }

    private CsvSourceOption Parse(string text)
    {
        var separator = text.IndexOf('=');
        if (separator <= 0)
            throw new FormatException($"Invalid source option '{text}'. Expected <name>=<value>.");

        var name = text[..separator].Trim();
        var suppliedValue = text[(separator + 1)..];
        try
        {
            return new CsvSourceOption(name, suppliedValue, values.ParseStrict(suppliedValue));
        }
        catch (FormatException exception)
        {
            throw InvalidSourceOption(name, suppliedValue, exception.Message);
        }
    }

    private static FormatException InvalidSourceOption(string name, string value, string reason)
        => new($"Invalid CSV source option '{name}' with value '{value}': {reason}");

    private static bool RequiredBoolean(object? value, string name)
        => value is bool result ? result : throw new FormatException($"'{name}' requires a boolean.");

    private static string RequiredText(object? value, string name)
        => value is string result ? result : throw new FormatException($"'{name}' requires text.");

    private static char RequiredChar(object? value, string name)
        => OptionalChar(value, name) ?? throw new FormatException($"'{name}' cannot be null.");

    private static char? OptionalChar(object? value, string name)
    {
        if (value is null)
            return null;
        if (value is string { Length: 1 } text)
            return text[0];
        throw new FormatException($"'{name}' requires a single character or null.");
    }

    private static int[] RequiredRows(object? value, string name)
    {
        if (value is not object?[] values || values.Length == 0)
            throw InvalidRows(name);

        var rows = new int[values.Length];
        for (var i = 0; i < values.Length; i++)
        {
            rows[i] = ToRowNumber(values[i]);
            if (rows[i] < 1)
                throw InvalidRows(name);
        }
        return rows;
    }

    private static int ToRowNumber(object? value)
        => value switch
        {
            int integer => integer,
            decimal numeric when numeric == decimal.Truncate(numeric)
                                 && numeric <= int.MaxValue
                                 && numeric >= int.MinValue => (int)numeric,
            _ => 0,
        };

    private static FormatException InvalidRows(string name)
        => new($"'{name}' requires a non-empty array of one-based row indexes.");

    private sealed record CsvSourceOption(string Name, string SuppliedValue, object? Value);

    private sealed class CsvDialectSettings
    {
        private readonly CsvProfile baseline;

        private CsvDialectSettings(CsvProfile baseline)
        {
            this.baseline = baseline;
            var defaults = baseline.Dialect;
            Header = defaults.Header;
            HeaderRows = defaults.HeaderRows;
            HeaderJoin = defaults.HeaderJoin;
            HeaderRepeat = defaults.HeaderRepeat;
            CommentRows = defaults.CommentRows;
            CommentChar = defaults.CommentChar;
            Delimiter = defaults.Delimiter;
            LineTerminator = defaults.LineTerminator;
            QuoteChar = defaults.QuoteChar;
            DoubleQuote = defaults.DoubleQuote;
            EscapeChar = defaults.EscapeChar;
            NullSequence = defaults.NullSequence;
            MissingCell = defaults.MissingCell;
            SkipInitialSpace = defaults.SkipInitialSpace;
            ArrayDelimiter = defaults.ArrayDelimiter;
            ArrayPrefix = defaults.ArrayPrefix;
            ArraySuffix = defaults.ArraySuffix;
        }

        public bool Header { get; private set; }
        public int[] HeaderRows { get; set; }
        public string HeaderJoin { get; set; }
        public bool HeaderRepeat { get; set; }
        public int[]? CommentRows { get; set; }
        public char? CommentChar { get; set; }
        public char Delimiter { get; set; }
        public string LineTerminator { get; set; }
        public char? QuoteChar { get; set; }
        public bool DoubleQuote { get; set; }
        public char? EscapeChar { get; set; }
        public string? NullSequence { get; set; }
        public string? MissingCell { get; set; }
        public bool SkipInitialSpace { get; set; }
        public char? ArrayDelimiter { get; set; }
        public char? ArrayPrefix { get; set; }
        public char? ArraySuffix { get; set; }
        public bool HeadersAreRows { get; private set; } = true;

        public static CsvDialectSettings CreateDefault() => new(CsvProfile.CommaDoubleQuote);

        public void SetHeader(bool header)
        {
            Header = header;
            HeadersAreRows = header;
        }

        public (CsvProfile Profile, bool HeadersAreRows) Build()
        {
            try
            {
                var dialect = new DialectDescriptor(
                    Header, HeaderRows, HeaderJoin, HeaderRepeat, CommentRows, CommentChar,
                    Delimiter, LineTerminator, QuoteChar, DoubleQuote, EscapeChar, NullSequence,
                    MissingCell, SkipInitialSpace, ArrayDelimiter, ArrayPrefix, ArraySuffix);
                var profile = new CsvProfile(dialect, baseline.Schema, baseline.Resource, baseline.Parsers);
                return (profile, HeadersAreRows);
            }
            catch (Exception exception) when (exception is not OutOfMemoryException)
            {
                throw new FormatException($"Invalid CSV source-option combination: {exception.Message}", exception);
            }
        }
    }
}
