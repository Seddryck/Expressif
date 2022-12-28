using Expressif.Functions.Numeric;
using Expressif.Values;
using Expressif.Values.Casters;
using Expressif.Values.Resolvers;
using Expressif.Values.Special;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;

namespace Expressif.Functions.Text
{
    [Function]
    public abstract class BaseTextFunction : IFunction
    {
        public object? Evaluate(object? value)
        {
            return value switch
            {
                null => EvaluateNull(),
                Null => EvaluateNull(),
                DBNull _ => EvaluateNull(),
                Empty _ => EvaluateEmpty(),
                Whitespace _ => EvaluateBlank(),
                string s => EvaluateHighLevelString(s),
                _ => EvaluateUncasted(value),
            };
        }

        private object EvaluateUncasted(object value)
        {
            var caster = new TextCaster();
            var str = caster.Execute(value);
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

            if (value.StartsWith("(") && value.EndsWith(")"))
                return EvaluateSpecial(value);

            return EvaluateString(value);
        }

        protected virtual object EvaluateNull() => new Null().Keyword;
        protected virtual object EvaluateEmpty() => new Empty().Keyword;
        protected virtual object EvaluateBlank() => new Whitespace().Keyword;
        protected virtual object EvaluateSpecial(string value) => value;
        protected abstract object EvaluateString(string value);
    }

    /// <summary>
    /// Returns the argument value except if this value only contains white-space characters then it returns `empty`.
    /// </summary>
    [Function(prefix:"", aliases: new[] {"blank-to-empty"})]
    public class WhitespacesToEmpty : BaseTextFunction
    {
        protected override object EvaluateBlank() => new Empty().Keyword;
        protected override object EvaluateString(string value) => value;
    }

    /// <summary>
    /// Returns the argument value except if this value only contains white-space characters then it returns `null`.
    /// </summary>
    [Function(prefix: "", aliases: new[] { "blank-to-null" })]
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

    /// <summary>
    /// Returns the length of the argument value. If the value is `null` or `empty` then it returns `0`. If the value is `blank` then it returns `-1`. 
    /// </summary>
    public class Length : BaseTextFunction
    {
        protected override object EvaluateSpecial(string value) => -1;
        protected override object EvaluateBlank() => -1;
        protected override object EvaluateEmpty() => 0;
        protected override object EvaluateNull() => 0;
        protected override object EvaluateString(string value) => value.Length;
    }
}
