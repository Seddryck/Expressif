using Expressif.Values.Special;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Functions.Text
{
    /// <summary>
    /// Returns the argument value without the specified character. If the argument and the parameter values are white-space characters then it returns `empty`.
    /// </summary>
    public class RemoveChars : BaseTextFunction
    {
        public Func<char> CharToRemove { get; }

        /// <param name="charToRemove">The char to be removed from the argument string.</param>
        public RemoveChars(Func<char> charToRemove)
            => CharToRemove = charToRemove;

        protected override object EvaluateString(string value)
        {
            var stringBuilder = new StringBuilder();
            foreach (var c in value)
                if (!c.Equals(CharToRemove.Invoke()))
                    stringBuilder.Append(c);
            return stringBuilder.ToString();
        }

        protected override object EvaluateBlank()
        {
            if (char.IsWhiteSpace(CharToRemove.Invoke()))
                return new Empty().Keyword;
            else
                return base.EvaluateBlank();
        }
    }

    /// <summary>
    /// Returns the argument value where a specific char has been replaced by another, both specified as parameters.
    /// </summary>
    public class ReplaceChars : BaseTextFunction
    {
        public Func<char> CharToReplace { get; }
        public Func<char> CharReplacing { get; }

        /// <param name="charToReplace">The char to be replaced from the argument string.</param>
        /// <param name="charReplacing">The replacing char from the argument string.</param>
        public ReplaceChars(Func<char> charToReplace, Func<char> charReplacing)
            => (CharToReplace, CharReplacing) = (charToReplace, charReplacing);

        protected override object EvaluateString(string value)
        {
            var charToReplace = CharToReplace.Invoke();
            var charReplacing = CharReplacing.Invoke();

            var stringBuilder = new StringBuilder();
            foreach (var c in value)
                if (!c.Equals(charToReplace))
                    stringBuilder.Append(c);
                else
                    stringBuilder.Append(charReplacing);
            return stringBuilder.ToString();
        }

        protected override object EvaluateBlank()
            => new Whitespace().Keyword;
    }
}
