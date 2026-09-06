namespace Expressif.Serialization;

/// <summary>A typed CSV dialect option, with optional original text for diagnostics.</summary>
public sealed record CsvSourceOption(string Name, object? Value, string? OriginalText = null)
{
    public string SuppliedValue => OriginalText ?? Convert.ToString(Value, System.Globalization.CultureInfo.InvariantCulture) ?? "null";
}
