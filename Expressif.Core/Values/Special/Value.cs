using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Expressif.Values.Special;

public sealed class Value
{
    private const string VALUE_KEYWORD_DEFAULT = "(value)";

    public static Value Instance { get; } = new();
    private Value() { }
    public string Keyword => VALUE_KEYWORD_DEFAULT;
    public static bool operator ==(Value left, object? right) => left.Equals(right);
    public static bool operator !=(Value left, object? right) => !left.Equals(right);

    public override bool Equals(object? value)
        => value switch
        {
            Null => false,
            null => false,
            DBNull => false,
            string v => AdvancedMatch(v),
            _ => true,
        };

    private bool AdvancedMatch(string value)
        => SpecialValue.Matches(value, Keyword) || !Null.Instance.Equals(value);

    public override int GetHashCode() => VALUE_KEYWORD_DEFAULT.GetHashCode();
}
