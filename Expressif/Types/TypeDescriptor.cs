using Expressif.Values;

namespace Expressif.Types;

public sealed record TypeLiteralMetadata(string? Syntax, string[] Examples);

public sealed record TypeDescriptor(
    string Name,
    string Summary,
    string? Parent,
    TypeLiteralMetadata? Literal,
    IReadOnlyDictionary<string, string> Bindings);

public static class TypeRegistry
{
    private static readonly TypeDescriptor[] Descriptors =
    [
        Create("scalar", "The common type family for non-structured values."),
        Create("text", "A text value represents a sequence of characters.", "scalar",
            "A value enclosed in double quotes", ["\"hello\""], typeof(string)),
        Create("boolean", "A boolean value is either true or false.", "scalar",
            "#true or #false", ["#true", "#false"], typeof(bool)),
        Create("numeric", "The common type family for integer and decimal values.", "scalar",
            null, ["10", "-10", "10.1", "-10.1"], typeof(decimal)),
        Create("integer", "An integer is a numeric value written without a decimal separator.", "numeric",
            "Digits, optionally preceded by a sign", ["10", "-10"], typeof(int)),
        Create("decimal", "A decimal is a numeric value written with a decimal separator.", "numeric",
            "Digits with a . decimal separator, optionally preceded by a sign", ["10.1", "-10.1"], typeof(decimal)),
        Create("temporal", "The common type family for date and time values.", "scalar"),
        Create("date", "A calendar date without a time component.", "temporal",
            "#\"yyyy-MM-dd\"", ["#\"2025-12-16\""], typeof(DateOnly)),
        Create("datetime", "A calendar date and time.", "temporal",
            "#\"yyyy-MM-ddTHH:mm:ss\"", ["#\"2025-12-16T14:30:00\""], typeof(DateTime)),
        Create("time", "A time of day without a date component.", "temporal",
            "#\"HH:mm:ss\"", ["#\"14:30:00\""], typeof(TimeOnly)),
        Create("duration", "An elapsed duration expressed using ISO 8601 notation.", "temporal",
            runtimeType: typeof(TimeSpan)),
        Create("year-month", "A calendar year and month.", "temporal", runtimeType: typeof(YearMonth)),
        Create("weekday", "A day of the week.", "temporal", runtimeType: typeof(Weekday)),
        Create("structured", "The common type family for values containing other values."),
        Create("array", "An array contains a sequence of values.", "structured",
            "Values enclosed in braces and separated by commas", ["{1, 2, 3}"], typeof(object[])),
        Create("tuple", "A tuple contains a fixed sequence of positional values.", "structured",
            "T followed by parenthesized comma-separated values", ["T(\"Alice\", 42)"], typeof(TupleValue)),
        Create("record", "A record contains named fields.", "structured",
            "Named fields enclosed in braces", ["{name := \"Alice\", age := 42}"], typeof(RecordValue)),
        Create("null", "Null represents the absence of a value.", "scalar",
            "#null", ["#null"]),
    ];

    private static readonly IReadOnlyDictionary<string, TypeDescriptor> ByName = BuildLookup();

    public static IReadOnlyList<TypeDescriptor> All => Descriptors;

    public static bool TryResolve(string name, out TypeDescriptor descriptor)
        => ByName.TryGetValue(name, out descriptor!);

    public static TypeDescriptor Resolve(string name)
        => TryResolve(name, out var descriptor)
            ? descriptor
            : throw new UnknownExpressifTypeException(name);

    private static TypeDescriptor Create(
        string name,
        string summary,
        string? parent = null,
        string? syntax = null,
        string[]? examples = null,
        Type? runtimeType = null)
        => new(
            name,
            summary,
            parent,
            examples is null ? null : new TypeLiteralMetadata(syntax, examples),
            runtimeType is null
                ? new Dictionary<string, string>()
                : new Dictionary<string, string> { ["dotnet"] = runtimeType.FullName! });

    private static IReadOnlyDictionary<string, TypeDescriptor> BuildLookup()
    {
        var lookup = Descriptors.ToDictionary(descriptor => descriptor.Name, StringComparer.OrdinalIgnoreCase);
        lookup.Add("date-time", lookup["datetime"]);
        return lookup;
    }
}

public sealed class TypeIntrospector
{
    public IEnumerable<TypeDescriptor> Describe() => TypeRegistry.All;
}

public sealed class UnknownExpressifTypeException : Exception
{
    public UnknownExpressifTypeException(string name)
        : base($"Unknown Expressif type literal ':{name}'.") { }
}
