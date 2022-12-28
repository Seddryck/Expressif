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

        /// <param name="charToRemove">The char to be removed from the argument string</param>
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

}
