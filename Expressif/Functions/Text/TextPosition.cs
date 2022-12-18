using Expressif.Values;
using Expressif.Values.Special;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Functions.Text
{
    abstract class AbstractTextToPosition : BaseTextFunction
    {
        public IScalarResolver<string> Substring { get; }
        public AbstractTextToPosition(IScalarResolver<string> substring)
            => (Substring) = substring;
    }

    /// <summary>
    /// Returns the substring of the argument string, containing all the characters immediately following the first occurrence of the string passed in parameter. If the parameter value is `null` or `empty` then the argument value is returned.
    /// </summary>
    class TextToAfter : AbstractTextToPosition
    {
        public TextToAfter(IScalarResolver<string> substring)
            : base(substring) { }
        protected override object EvaluateString(string value)
        {
            var substring = Substring.Execute();
            if (string.IsNullOrEmpty(substring) || new Empty().Equals(substring) || new Null().Equals(substring))
                return value;

            if (!value.Contains(substring))
                return string.Empty;

            var index = value.IndexOf(substring) + substring.Length;
            return value[index .. value.Length];
        }
    }

    /// <summary>
    /// Returns the substring of the argument string, containing all the characters immediately preceding the first occurrence of the string passed in parameter. If the parameter value is `null` or `empty` then the function returns `empty`.
    /// </summary>
    class TextToBefore : AbstractTextToPosition
    {
        public TextToBefore(IScalarResolver<string> substring)
            : base(substring) { }
        protected override object EvaluateString(string value)
        {
            var substring = Substring.Execute();
            if (string.IsNullOrEmpty(substring) || new Empty().Equals(substring) || new Null().Equals(substring))
                return string.Empty;

            if (!value.Contains(substring))
                return string.Empty;

            var index = value.IndexOf(substring);
            return value[..index];
        }
    }
}
