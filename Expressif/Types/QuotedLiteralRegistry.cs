using System.Globalization;
using Expressif.Values;

namespace Expressif.Types;

public interface IQuotedLiteralParser
{
    string TypeName { get; }

    Type RuntimeType { get; }

    bool TryParse(string representation, out object? value);

    string Format(object value);
}

public sealed class QuotedLiteralRegistry
{
    private readonly IReadOnlyDictionary<string, IQuotedLiteralParser> parsers;

    public QuotedLiteralRegistry(IEnumerable<IQuotedLiteralParser> parsers)
    {
        ArgumentNullException.ThrowIfNull(parsers);
        this.parsers = parsers.ToDictionary(parser => parser.TypeName, StringComparer.OrdinalIgnoreCase);
    }

    public static QuotedLiteralRegistry Default { get; } = new(
        [new DateLiteralParser(), new DateTimeLiteralParser(), new TimeLiteralParser()]);

    public (object? Value, string TypeName) Parse(string representation, string? typeName = null)
    {
        ArgumentNullException.ThrowIfNull(representation);
        if (!string.IsNullOrEmpty(typeName))
            return ParseExplicit(representation, typeName);

        var matches = parsers.Values
            .Select(parser => (Parser: parser, Accepted: parser.TryParse(representation, out var value), Value: value))
            .Where(match => match.Accepted)
            .OrderBy(match => match.Parser.TypeName, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        return matches switch
        {
            [] => throw new InvalidQuotedLiteralException(representation),
            [var match] => (match.Value, match.Parser.TypeName),
            _ => throw new AmbiguousQuotedLiteralException(representation, matches.Select(match => match.Parser.TypeName)),
        };
    }

    public string Serialize(object value, string? typeName = null)
    {
        ArgumentNullException.ThrowIfNull(value);
        var parser = string.IsNullOrEmpty(typeName)
            ? parsers.Values.SingleOrDefault(candidate => candidate.RuntimeType == value.GetType())
            : ResolveParser(typeName);
        if (parser is null)
            throw new NotSupportedException($"Literal value type '{value.GetType().Name}' cannot be serialized.");
        if (!parser.RuntimeType.IsInstanceOfType(value))
            throw new ArgumentException($"Value is not compatible with quoted literal type ':{parser.TypeName}'.", nameof(value));

        return $"#\"{RecordSyntax.EscapeDoubleQuoted(parser.Format(value))}\":{parser.TypeName}";
    }

    public bool CanSerialize(object value)
        => parsers.Values.Any(parser => parser.RuntimeType == value.GetType());

    private (object? Value, string TypeName) ParseExplicit(string representation, string typeName)
    {
        var parser = ResolveParser(typeName);
        return parser.TryParse(representation, out var value)
            ? (value, parser.TypeName)
            : throw new InvalidQuotedLiteralException(representation, parser.TypeName);
    }

    private IQuotedLiteralParser ResolveParser(string typeName)
    {
        if (!TypeRegistry.TryResolve(typeName, out var descriptor))
            throw new UnknownExpressifTypeException(typeName);
        return parsers.TryGetValue(descriptor.Name, out var parser)
            ? parser
            : throw new UnsupportedQuotedLiteralTypeException(typeName);
    }

    private sealed class DateLiteralParser : IQuotedLiteralParser
    {
        public string TypeName => "date";
        public Type RuntimeType => typeof(DateOnly);
        public bool TryParse(string representation, out object? value)
        {
            var accepted = DateOnly.TryParseExact(representation, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var parsed);
            value = accepted ? parsed : null;
            return accepted;
        }
        public string Format(object value) => ((DateOnly)value).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    private sealed class DateTimeLiteralParser : IQuotedLiteralParser
    {
        private static readonly string[] Formats = ["yyyy-MM-dd'T'HH:mm:ss", "yyyy-MM-dd HH:mm:ss"];
        public string TypeName => "datetime";
        public Type RuntimeType => typeof(DateTime);
        public bool TryParse(string representation, out object? value)
        {
            var accepted = DateTime.TryParseExact(representation, Formats, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var parsed);
            value = accepted ? parsed : null;
            return accepted;
        }
        public string Format(object value) => ((DateTime)value).ToString("yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture);
    }

    private sealed class TimeLiteralParser : IQuotedLiteralParser
    {
        public string TypeName => "time";
        public Type RuntimeType => typeof(TimeOnly);
        public bool TryParse(string representation, out object? value)
        {
            var accepted = TimeOnly.TryParseExact(representation, "HH:mm:ss", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var parsed);
            value = accepted ? parsed : null;
            return accepted;
        }
        public string Format(object value) => ((TimeOnly)value).ToString("HH:mm:ss", CultureInfo.InvariantCulture);
    }
}

public sealed class InvalidQuotedLiteralException : ExpressifException
{
    public InvalidQuotedLiteralException(string representation, string? typeName = null)
        : base(typeName is null
            ? $"Quoted typed literal '#\"{representation}\"' is invalid because no registered literal parser accepts it."
            : $"Quoted typed literal '#\"{representation}\":{typeName}' is not a valid :{typeName} representation.") { }
}

public sealed class AmbiguousQuotedLiteralException : ExpressifException
{
    public AmbiguousQuotedLiteralException(string representation, IEnumerable<string> typeNames)
        : base($"Quoted typed literal '#\"{representation}\"' is ambiguous between {string.Join(", ", typeNames.Select(name => $":{name}"))}; add an explicit type suffix.") { }
}

public sealed class UnsupportedQuotedLiteralTypeException : ExpressifException
{
    public UnsupportedQuotedLiteralTypeException(string typeName)
        : base($"Expressif type ':{typeName}' does not support quoted literals.") { }
}
