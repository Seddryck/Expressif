using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Expressif.Values.Special;

public class Any : BaseSpecial
{
    private const string ANY_KEYWORD_DEFAULT = "(any)";

    public Any()
        : this(ANY_KEYWORD_DEFAULT) { }

    public Any(string keyword)
        : base(keyword) { }

    public override bool Equals(object? value)
        => value switch
        {
            _ => true,
        };

    public override int GetHashCode() => ANY_KEYWORD_DEFAULT.GetHashCode();
}
