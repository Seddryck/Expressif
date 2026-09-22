using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Functions;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class FunctionAttribute : Attribute
{
    public string? Name { get; set; }
    public string[] Aliases { get; }
    public string? Prefix { get; }
    public string? DynamicReason { get; set; }
    /// <summary>Gets or sets a value indicating whether positional spread arguments are supported.</summary>
    /// <value><see langword="true"/> when positional spread arguments are supported; otherwise, <see langword="false"/>.</value>
    public bool SupportsValueSpread { get; set; }

    public FunctionAttribute()
        : this(null, System.Array.Empty<string>()) { }

    public FunctionAttribute(string? prefix = null, string[]? aliases = null)
        => (Prefix, Aliases) = (prefix, aliases ?? System.Array.Empty<string>());
}
