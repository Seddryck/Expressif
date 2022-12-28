using Expressif.Values.Special;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Functions.Text
{

    /// <summary>
    /// Returns the argument value formatted according to the mask specified as parameter. Each asterisk (`*`) of the mask is replaced by the corresponding character in the argument value. Other charachters of the mask are not substitued. If the length of the argument value is less than the count of charachetsr that must be replaced in the mask, the last asterisk characters are not replaced.
    /// </summary>
    [Function(prefix: "")]
    public class TextToMask : BaseTextFunction
    {
        private char maskChar { get; } = '*';
        public Func<string> Mask { get; }

        /// <param name="mask">The string representing the mask to apply to the argument string</param>
        public TextToMask(Func<string> mask)
            => Mask = mask;

        protected override object EvaluateString(string value)
        {
            var mask = Mask.Invoke() ?? string.Empty;
            var stringBuilder = new StringBuilder();
            var index = 0;
            foreach (var c in mask)
                if (c.Equals(maskChar))
                    stringBuilder.Append(index < value.Length ? value[index++] : maskChar);
                else
                    stringBuilder.Append(c);
            return stringBuilder.ToString();
        }

        protected override object EvaluateBlank()
            => Mask.Invoke() ?? string.Empty;
        protected override object EvaluateEmpty()
            => Mask.Invoke() ?? string.Empty;
    }

    /// <summary>
    /// Returns the value that passed to the function TextToMask will return the argument value. If the length of the mask and the length of the argument value are not equal the function returns `null`. If the non-asterisk characters are not matching between the mask and the argument value then the function also returns `null`.
    /// </summary>
    [Function(prefix: "")]
    public class MaskToText : BaseTextFunction
    {
        private char maskChar { get; } = '*';
        public Func<string> Mask { get; }

        /// <param name="mask">The string representing the mask to be unset from the argument string</param>
        public MaskToText(Func<string> mask)
            => Mask = mask;

        protected override object EvaluateString(string value)
        {
            var mask = Mask.Invoke() ?? string.Empty;
            var stringBuilder = new StringBuilder();
            if (mask.Length != value.Length)
                return new Null().Keyword;

            for (int i = 0; i < mask.Length; i++)
                if (mask[i].Equals(maskChar) && !value[i].Equals(maskChar))
                    stringBuilder.Append(value[i]);
                else if (!mask[i].Equals(value[i]))
                    return new Null().Keyword;
            return stringBuilder.ToString();
        }

        protected override object EvaluateBlank()
            => ((Mask.Invoke() ?? string.Empty).Replace(maskChar.ToString(), "").Length == 0) ? new Whitespace().Keyword : new Null().Keyword;
        protected override object EvaluateEmpty()
            => ((Mask.Invoke() ?? string.Empty).Replace(maskChar.ToString(), "").Length == 0) ? new Empty().Keyword : new Null().Keyword;
    }
}
