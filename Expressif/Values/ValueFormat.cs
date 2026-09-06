namespace Expressif.Values;

/// <summary>Specifies how an Expressif value is formatted for display.</summary>
public enum ValueFormat
{
    /// <summary>Formats structured values on a single line.</summary>
    Compact,

    /// <summary>Formats each structured element on its own indented line.</summary>
    Pretty,
}
