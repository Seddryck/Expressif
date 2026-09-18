using Expressif.Values;

namespace Expressif.Functions.Text;

/// <summary>Parses JSON text into native records, arrays, and scalar values.</summary>
[Function(prefix: "", DynamicReason = "Output depends on the JSON value parsed at runtime.")]
[Scope("text")]
public sealed class ParseJson : Function<string?, object?>
{
    public override object? Evaluate(string? value)
        => value is null ? null : JsonValueParser.Read(value);
}
