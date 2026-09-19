using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Operators;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class OperatorAttribute : Attribute
{
    public string[] Aliases { get; }

    public OperatorAttribute()
        : this([]) { }

    public OperatorAttribute(string[]? aliases = null)
        => (Aliases) = aliases ?? [];
}
