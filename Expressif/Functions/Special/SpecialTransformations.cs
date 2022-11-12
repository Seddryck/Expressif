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
    abstract class AbstractSpecialTransformation : IFunction
    {
        public object Evaluate(object value)
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

            if (new Any().Equals(value))
                return EvaluateAny();

            if (new Value().Equals(value))
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

    class NullToValue : AbstractSpecialTransformation
    {
        protected override object EvaluateNull() => new Value().Keyword;
        protected override object EvaluateEmpty() => new Empty().Keyword;
        protected override object EvaluateBlank() => new Whitespace().Keyword;
        protected override object EvaluateAny() => new Value().Keyword;
        protected override object EvaluateValue() => new Value().Keyword;
        protected override object EvaluateString(string value) => value;
    }

    class AnyToAny : AbstractSpecialTransformation
    {
        protected override object EvaluateNull() => new Any().Keyword;
        protected override object EvaluateEmpty() => new Any().Keyword;
        protected override object EvaluateBlank() => new Any().Keyword;
        protected override object EvaluateAny() => new Any().Keyword;
        protected override object EvaluateValue() => new Any().Keyword;
        protected override object EvaluateString(string value) => new Any().Keyword;
    }

    class ValueToValue : AbstractSpecialTransformation
    {
        protected override object EvaluateNull() => new Null().Keyword;
        protected override object EvaluateEmpty() => new Value().Keyword;
        protected override object EvaluateBlank() => new Value().Keyword;
        protected override object EvaluateAny() => new Value().Keyword;
        protected override object EvaluateValue() => new Value().Keyword;
        protected override object EvaluateString(string value) => new Value().Keyword;
    }
}
