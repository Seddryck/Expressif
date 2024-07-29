using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Expressif.Values.Special;
using Sprache;

namespace Expressif.Functions.Text;
public abstract class BaseTextRetain : BaseTextFunction
{
    protected override object? EvaluateBlank() => new Empty().Keyword;

    protected override object? EvaluateString(string value)

    {
        Span<char> span = value.ToCharArray();
        Span<char> result = stackalloc char[value.Length];
        int index = 0;

        foreach (var c in span)
            if (IsRetainable(c))
                result[index++] = c;

        if (index == 0)
            return new Empty().Keyword;

        return new string(result.Slice(0, index));
    }

    protected abstract bool IsRetainable(char c);
}

/// <summary>
/// Returns the input string with all non-numeric characters removed, leaving only digits (0-9).. If the argument is `null`, it returns `null`.
/// </summary>
public class RetainNumeric : BaseTextRetain
{
    protected override bool IsRetainable(char c)
        => char.IsDigit(c);
}

/// <summary>
/// Returns the input string with all characters removed except for digits (0-9) and the symbols `+`, `-`, `,` and `.` If the argument is `null`, it returns `null`.
/// </summary>
public class RetainNumericSymbol : RetainNumeric
{
    protected override bool IsRetainable(char c)
        => base.IsRetainable(c) || c.Equals('+') || c.Equals('-') || c.Equals('.') || c.Equals(',');
}

/// <summary>
/// Returns the input string with all characters removed except for letters (A-Z, a-z). If the argument is `null`, it returns `null`.
/// </summary>
public class RetainAlpha : BaseTextRetain
{
    protected override bool IsRetainable(char c)
        => char.IsLetter(c);
}


/// <summary>
/// Returns the input string with all characters removed except for letters (A-Z, a-z) and digits (0-9). If the argument is `null`, it returns `null`.
/// </summary>
public class RetainAlphaNumeric : BaseTextRetain
{
    protected override bool IsRetainable(char c)
        => char.IsLetterOrDigit(c);
}
