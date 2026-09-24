namespace Expressif.Functions;

/// <summary>Describes how positional arguments are packed for a constructor parameter.</summary>
internal enum ArgumentPackingMode
{
    Single,
    Variadic,
}

/// <summary>Declares a positional variadic parameter and whether its arguments permit value spread.</summary>
[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class ArgumentPackingAttribute(ArgumentPackingMode mode) : Attribute
{
    public ArgumentPackingMode Mode { get; } = mode;
    public bool AllowSpread { get; set; }
}
