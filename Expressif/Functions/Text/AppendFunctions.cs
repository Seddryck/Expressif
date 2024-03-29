﻿using Expressif.Values.Special;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Functions.Text;

public abstract class BaseTextAppend : BaseTextFunction
{
    public Func<string> Append { get; }
    public BaseTextAppend(Func<string> append)
        => Append = append;

    protected override object EvaluateEmpty() => Append.Invoke() ?? string.Empty;
    protected override object EvaluateBlank() => Append.Invoke() ?? string.Empty;
}

public abstract class BaseTextAppendNonNullable : BaseTextAppend
{
    public BaseTextAppendNonNullable(Func<string> append)
        : base(append) { }

    protected override object EvaluateNull()
    {
        var value = Append.Invoke();
        if (new Null().Equals(value))
            return string.Empty;
        else
            return value;
    }
}

/// <summary>
/// Returns the argument value preceeded by the parameter value. If the argument is `null`, it returns `null`.
/// </summary>
public class Prefix : BaseTextAppend
{
    /// <param name="prefix">The text to append</param>
    public Prefix(Func<string> prefix)
        : base(prefix) { }
    protected override object EvaluateString(string value) => $"{Append.Invoke()}{value}";
}

/// <summary>
/// Returns the argument value followed by the parameter value. If the argument is `null`, it returns `null`.
/// </summary>
public class Suffix : BaseTextAppend
{
    /// <param name="suffix">The text to append</param>
    public Suffix(Func<string> suffix)
        : base(suffix) { }
    protected override object EvaluateString(string value) => $"{value}{Append.Invoke()}";
}

/// <summary>
/// Returns the argument value followed by the parameter value. If the argument is `null`, it returns the text specified as the parameter.
/// </summary>
public class Append : BaseTextAppendNonNullable
{
    /// <param name="text">The text to append</param>
    public Append(Func<string> text)
        : base(text) { }
    protected override object EvaluateString(string value) => $"{value}{Append.Invoke()}";
}

/// <summary>
/// Returns the argument value preceeded by the parameter value. If the argument is `null`, it returns the text specified as the parameter.
/// </summary>
public class Prepend : BaseTextAppendNonNullable
{
    /// <param name="text">The text to prepend</param>
    public Prepend(Func<string> text)
        : base(text) { }
    protected override object EvaluateString(string value) => $"{Append.Invoke()}{value}";
}

#region Space

/// <summary>
/// Returns the argument value preceeded by a space character. If the argument is `null`, it returns `null`.
/// </summary>
public class PrefixSpace : Prefix
{
    public PrefixSpace()
        : base(() => ((char)32).ToString()) { }
}

/// <summary>
/// Returns the argument value followed by a space character. If the argument is `null`, it returns `null`.
/// </summary>
public class SuffixSpace : Suffix
{
    public SuffixSpace()
        : base(() => ((char)32).ToString()) { }
}

/// <summary>
/// Returns the argument value followed by a space character. If the argument is `null`, it returns the text specified as the parameter.
/// </summary>
public class AppendSpace : Append
{
    public AppendSpace()
        : base(() => ((char)32).ToString()) { }
}

/// <summary>
/// Returns the argument value preceeded by a space character. If the argument is `null`, it returns the text specified as the parameter.
/// </summary>
public class PrependSpace : Prepend
{
    public PrependSpace()
        : base(() => ((char)32).ToString()) { }
}

#endregion

#region NewLine

/// <summary>
/// Returns the argument value preceeded by a space character. If the argument is `null`, it returns `null`.
/// </summary>
public class PrefixNewLine : Prefix
{
    public PrefixNewLine()
        : base(() => Environment.NewLine) { }
}

/// <summary>
/// Returns the argument value followed by a space character. If the argument is `null`, it returns `null`.
/// </summary>
public class SuffixNewLine : Suffix
{
    public SuffixNewLine()
        : base(() => Environment.NewLine) { }
}

/// <summary>
/// Returns the argument value followed by a space character. If the argument is `null`, it returns the text specified as the parameter.
/// </summary>
public class AppendNewLine : Append
{
    public AppendNewLine()
        : base(() => Environment.NewLine) { }
}

/// <summary>
/// Returns the argument value preceeded by a space character. If the argument is `null`, it returns the text specified as the parameter.
/// </summary>
public class PrependNewLine : Prepend
{
    public PrependNewLine()
        : base(() => Environment.NewLine) { }
}

#endregion

/// <summary>
/// Returns the argument value with a subset of the string substitued by a another string.
/// </summary>
public class ReplaceSlice : BaseTextAppend
{
    public Func<int> Start { get; }
    public Func<int> Length { get; }

    /// <param name="start">The position to start to replace</param>
    /// <param name="length">The length to replace</param>
    /// <param name="append">The text to append when the slice has been removed</param>
    public ReplaceSlice(Func<int> start, Func<int> length, Func<string> append)
        : base(append) { (Start, Length) = (start, length); }
    protected override object EvaluateString(string value)
    {
        var start = Start.Invoke();
        var length = Length.Invoke();

        if (length<0)
        {
            start += length;
            length = Math.Abs(length);
        }

        if (start>= value.Length)
            return $"{value}{Append.Invoke()}";
        if (start + length <=0)
            return $"{Append.Invoke()}{value}";

        var text = new StringBuilder();
        if (start >= 0)
            text.Append(value[..start]);
        text.Append(Append.Invoke());
        if (start+length<=value.Length)
            text.Append(value[(start + length) ..]);

        return text.ToString();
    }
}