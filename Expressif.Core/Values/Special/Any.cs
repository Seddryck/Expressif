using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Expressif.Values.Special;

public sealed class Any
{
    private const string ANY_KEYWORD_DEFAULT = "(any)";
    public static Any Instance { get; } = new();
    private Any() { }
    public static string Keyword => ANY_KEYWORD_DEFAULT;
    public override bool Equals(object? value)
        => value switch
        {
            _ => true,
        };

    public override int GetHashCode() => ANY_KEYWORD_DEFAULT.GetHashCode();
}
