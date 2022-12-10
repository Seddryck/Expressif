using Expressif.Values;
using Expressif.Values.Special;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Functions.Text
{
    abstract class AbstractTextToPosition : AbstractTextTransformation
    {
        public IScalarResolver<string> Substring { get; }
        public AbstractTextToPosition(IScalarResolver<string> substring)
            => (Substring) = substring;
    }

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

    class TextToBefore: AbstractTextToPosition
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
