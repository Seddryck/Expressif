using Expressif.Values;

namespace Expressif.Serialization;

/// <summary>Serializes Expressif result values.</summary>
public interface IValueSerializer
{
    /// <summary>Serializes a value using the requested presentation style.</summary>
    string Serialize(object? value, ValueFormat style = ValueFormat.Compact, string indentation = "  ");

    /// <summary>Serializes a value using the requested presentation options.</summary>
    string Serialize(object? value, ValueFormattingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return Serialize(value, options.Format, options.Indentation);
    }
}
