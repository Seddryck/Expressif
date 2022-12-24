using Expressif.Values;
using Expressif.Values.Casters;
using Expressif.Values.Special;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;

namespace Expressif.Functions.Special
{

    [Function(prefix: "")]
    abstract class BaseSpecialFunction : IFunction
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
    class NullToValue : BaseSpecialFunction
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
    class AnyToAny : BaseSpecialFunction
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
    class ValueToValue : BaseSpecialFunction
    {
        protected override object EvaluateNull() => new Null().Keyword;
        protected override object EvaluateEmpty() => new Value().Keyword;
        protected override object EvaluateBlank() => new Value().Keyword;
        protected override object EvaluateAny() => new Value().Keyword;
        protected override object EvaluateValue() => new Value().Keyword;
        protected override object EvaluateString(string value) => new Value().Keyword;
    }
}
