namespace Expressif.Serialization;

/// <summary>Specifies the serialization format for an Expressif result.</summary>
public enum ValueSerializationFormat
{
    /// <summary>Uses Expressif's display syntax.</summary>
    Raw,

    /// <summary>Uses JSON syntax.</summary>
    Json,
}
