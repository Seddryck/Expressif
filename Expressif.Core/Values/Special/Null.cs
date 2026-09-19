using System;
using System.Collections.Generic;
using System.Text;

namespace Expressif.Values.Special;

public class Null : BaseSpecial
{
    private const string NULL_KEYWORD_DEFAULT = "(null)";

    public Null()
        : this(NULL_KEYWORD_DEFAULT) { }

    public Null(string keyword)
        : base(keyword) { }

    public override bool Equals(object? value)
        => value switch
        {
            Null => true,
            null => true,
            DBNull _ => true,
            string v => AdvancedMatch(v),
            _ => false,
        };

    public override int GetHashCode() => NULL_KEYWORD_DEFAULT.GetHashCode();
}
