using System;
using System.Linq;
using System.Text;
using Expressif.Library.Text;
using Expressif.Values.Special;

namespace Expressif.Library.Text.Filtering;

/// <summary>
/// Returns only those characters specified in the parameter, in the order, they were originally entered in the input value.
/// </summary>
[Scope("text/filtering")]
public class FilterChars : BaseTextFunction
{
    public Func<char[]> Filter { get; }

    /// <param name="filter">The chars to filter from the argument string.</param>
    public FilterChars(Func<char[]> filter)
        => (Filter) = (filter);

    /// <param name="filter">The string containing the chars to filter from the argument string.</param>
    public FilterChars(Func<string> filter)
    {
        static Func<char[]> toCharArray(string str) => str.ToCharArray;
        Filter = toCharArray(filter.Invoke());
    }

    protected override object? EvaluateString(string value)
    {
        var filter = Filter.Invoke();

        var stringBuilder = new StringBuilder();
        foreach (var c in value)
        {
            if (filter.Contains(c))
                stringBuilder.Append(c);
        }

        return stringBuilder.ToString();
    }

    protected override object? EvaluateBlank()
        => new Whitespace().Keyword;
}
