using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using Expressif.Values;
using Expressif.Values.Casters;
using Expressif.Values.Special;

namespace Expressif.Functions.Special;

[Function(prefix: "")]
public abstract class BaseSpecialFunction : IFunction
{
    public object? Evaluate(object? value)
    {
        return value switch
        {
            null => EvaluateNull(),
            DBNull _ => EvaluateNull(),
            Null => EvaluateNull(),
            Empty => EvaluateEmpty(),
            Whitespace => EvaluateBlank(),
            Any => EvaluateAny(),
            Value => EvaluateValue(),
            string s => EvaluateHighLevelString(s),
            _ => EvaluateUncasted(value),
        };
    }

    private object EvaluateUncasted(object value)
    {
        var caster = new TextCaster();
        var str = caster.Cast(value);
        return EvaluateHighLevelString(str);
    }

    protected virtual object EvaluateHighLevelString(string value)
    {
        if (new Empty().Equals(value))
            return EvaluateEmpty();

        if (new Null().Equals(value))
            return EvaluateNull();

        if (new Whitespace().Equals(value))
            return EvaluateBlank();

        if (new Any().Keyword.Equals(value))
            return EvaluateAny();

        if (new Value().Keyword.Equals(value))
            return EvaluateValue();

        return EvaluateString(value);
    }

    protected abstract object EvaluateNull();
    protected abstract object EvaluateEmpty();
    protected abstract object EvaluateBlank();
    protected abstract object EvaluateAny();
    protected abstract object EvaluateValue();
    protected abstract object EvaluateString(string value);
}

/// <summary>
/// Returns the value passed as argument, except if the value is `null` then it returns `value`.
/// </summary>
public class NullToValue : BaseSpecialFunction
{
    protected override object EvaluateNull() => new Value().Keyword;
    protected override object EvaluateEmpty() => new Empty().Keyword;
    protected override object EvaluateBlank() => new Whitespace().Keyword;
    protected override object EvaluateAny() => new Value().Keyword;
    protected override object EvaluateValue() => new Value().Keyword;
    protected override object EvaluateString(string value) => value;
}

/// <summary>
/// Returns `any`.
/// </summary>
public class AnyToAny : BaseSpecialFunction
{
    protected override object EvaluateNull() => new Any().Keyword;
    protected override object EvaluateEmpty() => new Any().Keyword;
    protected override object EvaluateBlank() => new Any().Keyword;
    protected override object EvaluateAny() => new Any().Keyword;
    protected override object EvaluateValue() => new Any().Keyword;
    protected override object EvaluateString(string value) => new Any().Keyword;
}

/// <summary>
/// Returns `value` except if the argument value is `null` then it returns `null`.
/// </summary>
public class ValueToValue : BaseSpecialFunction
{
    protected override object EvaluateNull() => new Null().Keyword;
    protected override object EvaluateEmpty() => new Value().Keyword;
    protected override object EvaluateBlank() => new Value().Keyword;
    protected override object EvaluateAny() => new Value().Keyword;
    protected override object EvaluateValue() => new Value().Keyword;
    protected override object EvaluateString(string value) => new Value().Keyword;
}

/// <summary>
/// Returns the first non-null result from two or more expressions evaluated from left to right against the same input. Returns <see langword="null"/> when every expression evaluates to <see langword="null"/>.
/// </summary>
[Function(prefix: "")]
public class Coalesce : IFunction
{
    public IReadOnlyList<Func<object?, object?>> Expressions { get; }

    /// <param name="expressions">The candidate expressions, with at least two required.</param>
    public Coalesce(IEnumerable<Func<object?, object?>> expressions)
    {
        Expressions = expressions.ToArray();
        if (Expressions.Count < 2)
            throw new MissingOrUnexpectedParametersFunctionException(nameof(Coalesce), Expressions.Count);
    }

    public object? Evaluate(object? value)
    {
        foreach (var expression in Expressions)
        {
            var result = expression.Invoke(value);
            if (result is not null && !new Null().Equals(result))
                return result;
        }

        return null;
    }
}
