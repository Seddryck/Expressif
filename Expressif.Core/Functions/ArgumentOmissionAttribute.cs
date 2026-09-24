namespace Expressif.Functions;

/// <summary>Describes how an omitted constructor argument is represented at runtime.</summary>
internal enum ArgumentOmissionMode
{
    EmptyVariadic,
    Absent,
}

/// <summary>Declares an omission contract independently of evaluation and provider lifetime.</summary>
[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class ArgumentOmissionAttribute(ArgumentOmissionMode mode) : Attribute
{
    public ArgumentOmissionMode Mode { get; } = mode;
}
