using System.Collections;
using Expressif.Functions.Numeric;
using Expressif.Values;
using Expressif.Values.Casters;
using Expressif.Values.Special;

namespace Expressif.Functions.Text;

[Function]
public abstract class BaseTextFunction<TOut> : IFunction<string?, TOut?>
{
    TOut? IFunction<string?, TOut?>.Evaluate(string? value)
        => Evaluate((object?)value) is TOut result ? result : default;

    public object? Evaluate(object? value)
    {
        return value switch
        {
            null => EvaluateNull(),
            Null => EvaluateNull(),
            DBNull _ => EvaluateNull(),
            Empty _ => EvaluateEmpty(),
            Whitespace _ => EvaluateBlank(),
            IEnumerable array and not string => EvaluateArray(array),
            string s => EvaluateHighLevelString(s),
            _ => EvaluateUncasted(value),
        };
    }

    private object? EvaluateUncasted(object value)
    {
        var caster = new TextCaster();
        var str = caster.Cast(value);
        return EvaluateHighLevelString(str);
    }

    protected virtual object? EvaluateHighLevelString(string value)
    {
        if (new Empty().Equals(value))
            return EvaluateEmpty();

        if (new Null().Equals(value))
            return EvaluateNull();

        if (new Whitespace().Equals(value))
            return EvaluateBlank();

        if (value.StartsWith('(') && value.EndsWith(')'))
            return EvaluateSpecial(value);

        return EvaluateString(value);
    }

    protected virtual object? EvaluateNull() => new Null().Keyword;
    protected virtual object? EvaluateEmpty() => new Empty().Keyword;
    protected virtual object? EvaluateBlank() => new Whitespace().Keyword;
    protected virtual object? EvaluateSpecial(string value) => value;

    protected virtual object? EvaluateArray(IEnumerable array) => null;
    protected abstract object? EvaluateString(string value);
}

public abstract class BaseTextFunction : BaseTextFunction<string>
{ }

/// <summary>
/// Returns the argument value except if this value only contains white-space characters then it returns `empty`.
/// </summary>
[Function(prefix: "", aliases: ["blank-to-empty"])]
[Scope("text/normalization")]
public class WhitespacesToEmpty : BaseTextFunction
{
    protected override object EvaluateBlank() => new Empty().Keyword;
    protected override object EvaluateString(string value) => value;
}

/// <summary>
/// Returns the argument value except if this value only contains white-space characters then it returns `null`.
/// </summary>
[Function(prefix: "", aliases: ["blank-to-null"])]
[Scope("text/normalization")]
public class WhitespacesToNull : BaseTextFunction
{
    protected override object EvaluateBlank() => new Null().Keyword;
    protected override object EvaluateEmpty() => new Null().Keyword;
    protected override object EvaluateString(string value) => value;
}

/// <summary>
/// Returns the argument value except if this value is `empty` then it returns `null`.
/// </summary>
[Function(prefix: "")]
public class EmptyToNull : BaseTextFunction
{
    protected override object EvaluateEmpty() => new Null().Keyword;
    protected override object EvaluateString(string value) => value;
}

/// <summary>
/// Returns the argument value except if this value is `null` then it returns `empty`.
/// </summary>
[Function(prefix: "")]
public class NullToEmpty : BaseTextFunction
{
    protected override object EvaluateNull() => new Empty().Keyword;
    protected override object EvaluateString(string value) => value;
}
