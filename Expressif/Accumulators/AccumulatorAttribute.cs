using System;

namespace Expressif.Accumulators;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class AccumulatorAttribute : Attribute
{
    public string[] Aliases { get; }
    public string? Prefix { get; }

    public AccumulatorAttribute()
        : this(null, []) { }

    public AccumulatorAttribute(string? prefix = null, string[]? aliases = null)
        => (Prefix, Aliases) = (prefix, aliases ?? []);
}
