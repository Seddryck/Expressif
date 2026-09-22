using System;
using System.Collections.Generic;
using System.Text;

namespace Expressif.Values.Special;

public sealed class Null
{
    private const string NULL_KEYWORD_DEFAULT = "(null)";
    public static Null Instance { get; } = new();
    private Null() { }
    public static string Keyword => NULL_KEYWORD_DEFAULT;
    public static bool operator ==(Null? left, Null? right) => ReferenceEquals(left, right);
    public static bool operator !=(Null? left, Null? right) => !(left == right);

    public override bool Equals(object? value)
        => value switch
        {
            Null => true,
            null => true,
            DBNull _ => true,
            string v => SpecialValue.Matches(v, Keyword),
            _ => false,
        };

    public override int GetHashCode() => NULL_KEYWORD_DEFAULT.GetHashCode();
}
