using System;
using System.Collections.Generic;
using System.Text;

namespace Expressif.Values.Special;

public sealed class Whitespace
{
    private const string WHITESPACE_KEYWORD_DEFAULT = "(blank)";

    public static Whitespace Instance { get; } = new();
    private Whitespace() { }
    public string Keyword => WHITESPACE_KEYWORD_DEFAULT;
    public static bool operator ==(Whitespace left, object? right) => left.Equals(right);
    public static bool operator !=(Whitespace left, object? right) => !left.Equals(right);

    public override bool Equals(object? value)
        => value switch
        {
            Whitespace => true,
            string v => string.IsNullOrWhiteSpace(v) || SpecialValue.Matches(v, Keyword),
            _ => false,
        };

    public override int GetHashCode() => WHITESPACE_KEYWORD_DEFAULT.GetHashCode();
}
