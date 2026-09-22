namespace Expressif.Values.Types;

/// <summary>The common type family for non-structured values.</summary>
[ExpressifType(Name = "scalar")]
public sealed class ScalarTypeDescriptor : ITypeDescriptor
{
    public Type? RuntimeType => null;
}

/// <summary>A text value represents a sequence of characters.</summary>
[ExpressifType(Parent = "scalar", LiteralSyntax = "A value enclosed in double quotes", LiteralExamples = ["\"hello\""])]
public sealed class TextTypeDescriptor : ExpressifTypeDefinition<string> { }

/// <summary>A boolean value is either true or false.</summary>
[ExpressifType(Parent = "scalar", LiteralSyntax = "#true or #false", LiteralExamples = ["#true", "#false"])]
public sealed class BooleanTypeDescriptor : ExpressifTypeDefinition<bool> { }

/// <summary>The common type family for integer and decimal values.</summary>
[ExpressifType(Parent = "scalar", LiteralExamples = ["10", "-10", "10.1", "-10.1"])]
public sealed class NumericTypeDescriptor : ExpressifTypeDefinition<decimal> { }

/// <summary>An integer is a numeric value written without a decimal separator.</summary>
[ExpressifType(Parent = "numeric", LiteralSyntax = "Digits, optionally preceded by a sign", LiteralExamples = ["10", "-10"])]
public sealed class IntegerTypeDescriptor : ExpressifTypeDefinition<int> { }

/// <summary>A decimal is a numeric value written with a decimal separator.</summary>
[ExpressifType(Parent = "numeric", LiteralSyntax = "Digits with a . decimal separator, optionally preceded by a sign", LiteralExamples = ["10.1", "-10.1"])]
public sealed class DecimalTypeDescriptor : ExpressifTypeDefinition<decimal> { }

/// <summary>The common type family for date and time values.</summary>
[ExpressifType(Parent = "scalar")]
public sealed class TemporalTypeDescriptor : ITypeDescriptor
{
    public Type? RuntimeType => null;
}

/// <summary>A calendar date without a time component.</summary>
[ExpressifType(Parent = "temporal", LiteralSyntax = "#\"yyyy-MM-dd\":date", LiteralExamples = ["#\"2025-12-16\":date"])]
public sealed class DateTypeDescriptor : ExpressifTypeDefinition<DateOnly> { }

/// <summary>A calendar date and time.</summary>
[ExpressifType(Name = "datetime", Parent = "temporal", LiteralSyntax = "#\"yyyy-MM-ddTHH:mm:ss\":datetime", LiteralExamples = ["#\"2025-12-16T14:30:00\":datetime"])]
public sealed class DateTimeTypeDescriptor : ExpressifTypeDefinition<DateTime> { }

/// <summary>A time of day without a date component.</summary>
[ExpressifType(Parent = "temporal", LiteralSyntax = "#\"HH:mm:ss\":time", LiteralExamples = ["#\"14:30:00\":time"])]
public sealed class TimeTypeDescriptor : ExpressifTypeDefinition<TimeOnly> { }

/// <summary>An elapsed duration expressed using ISO 8601 notation.</summary>
[ExpressifType(Parent = "scalar")]
public sealed class DurationTypeDescriptor : ExpressifTypeDefinition<TimeSpan> { }

/// <summary>The common type family for values containing other values.</summary>
[ExpressifType]
public sealed class StructuredTypeDescriptor : ITypeDescriptor
{
    public Type? RuntimeType => null;
}

/// <summary>An array contains a sequence of values.</summary>
[ExpressifType(Parent = "structured", LiteralSyntax = "Values enclosed in braces and separated by commas", LiteralExamples = ["{1, 2, 3}"])]
public sealed class ArrayTypeDescriptor : ExpressifTypeDefinition<object[]> { }

/// <summary>Null represents the absence of a value.</summary>
[ExpressifType(Name = "null", Parent = "scalar", LiteralSyntax = "#null", LiteralExamples = ["#null"])]
public sealed class NullTypeDescriptor : ITypeDescriptor
{
    public Type? RuntimeType => null;
}
