using System;
using Expressif.Library.Text;

namespace Expressif.Library.Text.Selection;

[Scope("text/selection")]
public abstract class BaseTextLength : BaseTextFunction
{
    public Func<int> Length { get; }

    public BaseTextLength(Func<int> length)
        => Length = length;
}

/// <summary>
/// Returns the first chars of the argument value. The length of the string returned is maximum the parameter value, if the argument string is smaller then the full string is returned.
/// </summary>
public class FirstChars : BaseTextLength
{
    /// <param name="length">An integer value between 0 and +Infinity, defining the length of the substring to return.</param>
    public FirstChars(Func<int> length)
        : base(length) { }

    protected override object EvaluateString(string value)
    {
        var length = Length.Invoke();
        return value.Length >= length ? value[..length] : value;
    }
}

/// <summary>
/// Returns the last chars of the argument value. The length of the string returned is maximum the parameter value, if the argument string is smaller then the full string is returned.
/// </summary>
public class LastChars : BaseTextLength
{
    /// <param name="length">An integer value between 0 and +Infinity, defining the length of the substring to return.</param>
    public LastChars(Func<int> length)
        : base(length) { }

    protected override object EvaluateString(string value)
    {
        var length = Length.Invoke();
        return value.Length >= length ? value.Substring(value.Length - length, length) : value;
    }
}

/// <summary>
/// Returns the last chars of the argument value. The length of the string omitted at the beginning of the argument value is equal to the parameter value. If the length of the argument value is smaller or equal to the parameter value then the functions returns `empty`.
/// </summary>
public class SkipFirstChars : BaseTextLength
{
    /// <param name="length">An integer value between 0 and +Infinity, defining the length of the substring to skip.</param>
    public SkipFirstChars(Func<int> length)
        : base(length) { }

    protected override object EvaluateString(string value)
    {
        var length = Length.Invoke();
        return value.Length <= length ? Expressif.Values.Special.Empty.Keyword : value[length..];
    }
}

/// <summary>
/// Returns the first chars of the argument value. The length of the string omitted at the end of the argument value is equal to the parameter value. If the length of the argument value is smaller or equal to the parameter value then the functions returns `empty`.
/// </summary>
public class SkipLastChars : BaseTextLength
{
    /// <param name="length">An integer value between 0 and +Infinity, defining the length of the substring to skip.</param>
    public SkipLastChars(Func<int> length)
        : base(length) { }

    protected override object EvaluateString(string value)
    {
        var length = Length.Invoke();
        return value.Length <= length ? Expressif.Values.Special.Empty.Keyword : value[..^length];
    }
}
