using Expressif.Values.Special;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Functions.Text;


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
        => value.Length >= Length.Invoke() ? value[..Length.Invoke()] : value;
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
        => value.Length >= Length.Invoke() ? value.Substring(value.Length - Length.Invoke(), Length.Invoke()) : value;
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
        => value.Length <= Length.Invoke() ? new Empty().Keyword : value[Length.Invoke()..];
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
        => value.Length <= Length.Invoke() ? new Empty().Keyword : value[..^(Length.Invoke())];
}

/// <summary>
/// Returns the argument value without all leading or trailing white-space characters.
/// </summary>
public class Trim : BaseTextFunction
{
    protected override object EvaluateBlank() => new Empty().Keyword;
    protected override object EvaluateString(string value) => value.Trim();
}
