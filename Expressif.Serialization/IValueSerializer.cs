using Expressif.Values;

namespace Expressif.Serialization;

/// <summary>Serializes Expressif result values.</summary>
public interface IValueSerializer
{
    /// <summary>Serializes a value using the requested presentation style.</summary>
    string Serialize(object? value, ValueFormat style = ValueFormat.Compact, string indentation = "  ");
}
