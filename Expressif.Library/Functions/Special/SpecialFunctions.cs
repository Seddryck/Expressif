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
public abstract class BaseSpecialFunction : IFunction<object?, string>
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

    string IFunction<object?, string>.Evaluate(object? value) => (string)Evaluate(value)!;

    private string EvaluateUncasted(object value)
    {
        var caster = new TextCaster();
        var str = caster.Cast(value);
        return EvaluateHighLevelString(str);
    }

    protected virtual string EvaluateHighLevelString(string value)
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

    protected abstract string EvaluateNull();
    protected abstract string EvaluateEmpty();
    protected abstract string EvaluateBlank();
    protected abstract string EvaluateAny();
    protected abstract string EvaluateValue();
    protected abstract string EvaluateString(string value);
}

/// <summary>
/// Returns the value passed as argument, except if the value is `null` then it returns `value`.
/// </summary>
public class NullToValue : BaseSpecialFunction
{
    protected override string EvaluateNull() => new Value().Keyword;
    protected override string EvaluateEmpty() => new Empty().Keyword;
    protected override string EvaluateBlank() => new Whitespace().Keyword;
    protected override string EvaluateAny() => new Value().Keyword;
    protected override string EvaluateValue() => new Value().Keyword;
    protected override string EvaluateString(string value) => value;
}

/// <summary>
/// Returns `any`.
/// </summary>
public class AnyToAny : BaseSpecialFunction
{
    protected override string EvaluateNull() => new Any().Keyword;
    protected override string EvaluateEmpty() => new Any().Keyword;
    protected override string EvaluateBlank() => new Any().Keyword;
    protected override string EvaluateAny() => new Any().Keyword;
    protected override string EvaluateValue() => new Any().Keyword;
    protected override string EvaluateString(string value) => new Any().Keyword;
}

/// <summary>
/// Returns `value` except if the argument value is `null` then it returns `null`.
/// </summary>
public class ValueToValue : BaseSpecialFunction
{
    protected override string EvaluateNull() => new Null().Keyword;
    protected override string EvaluateEmpty() => new Value().Keyword;
    protected override string EvaluateBlank() => new Value().Keyword;
    protected override string EvaluateAny() => new Value().Keyword;
    protected override string EvaluateValue() => new Value().Keyword;
    protected override string EvaluateString(string value) => new Value().Keyword;
}

/// <summary>
/// Returns the first non-null result from two or more expressions evaluated from left to right against the same input. Returns <see langword="null"/> when every expression evaluates to <see langword="null"/>.
/// </summary>
[Function(prefix: "")]
public class Coalesce : IFunction
{
    public IReadOnlyList<Func<object?, object?>> Expressions { get; }

    /// <param name="expressions">Two or more candidate expressions evaluated from left to right against the same input.</param>
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
