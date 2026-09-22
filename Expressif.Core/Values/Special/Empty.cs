using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Expressif.Values.Special;

public sealed class Empty
{
    private const string EMPTY_KEYWORD_DEFAULT = "(empty)";

    public static Empty Instance { get; } = new();
    private Empty() { }
    public string Keyword => EMPTY_KEYWORD_DEFAULT;
    public static bool operator ==(Empty left, object? right) => left.Equals(right);
    public static bool operator !=(Empty left, object? right) => !left.Equals(right);

    public override bool Equals(object? value)
        => value switch
        {
            Empty => true,
            string v => string.IsNullOrEmpty(v) || SpecialValue.Matches(v, Keyword),
            _ => false,
        };

    public override int GetHashCode() => EMPTY_KEYWORD_DEFAULT.GetHashCode();
}
