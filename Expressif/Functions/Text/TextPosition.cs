using Expressif.Values;
using Expressif.Values.Resolvers;
using Expressif.Values.Special;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Functions.Text
{
    abstract class BasePositionFunction : BaseTextFunction
    {
        public IScalarResolver<string> Substring { get; }
        public IScalarResolver<int> Count { get; }
        public BasePositionFunction(IScalarResolver<string> substring, IScalarResolver<int> count)
            => (Substring, Count) = (substring, count);
    }

    /// <summary>
    /// Returns the substring of the argument string, containing all the characters immediately following the first occurrence of the string passed in parameter. If the parameter value is `null` or `empty` then the argument value is returned.
    /// </summary>
    class After : BasePositionFunction
    {
        /// <param name="substring">The string to seek.</param>
        public After(IScalarResolver<string> substring)
            : this(substring, new LiteralScalarResolver<int>(0)) { }

        /// <param name="substring">The string to seek.</param>
        /// <param name="count">The number of character positions to examine.</param>
        public After(IScalarResolver<string> substring, IScalarResolver<int> count)
            : base(substring, count) { }

        protected override object EvaluateString(string value)
        {
            var substring = Substring.Execute();
            if (string.IsNullOrEmpty(substring) || new Empty().Equals(substring) || new Null().Equals(substring))
                return value;

            if (!value.Contains(substring))
                return string.Empty;

            var i = 0;
            var index = substring.Length * -1;
            do
            {
                index += substring.Length;
                index = value.IndexOf(substring, index);
                i += 1;
            }
            while (index != -1 && i <= Count.Execute());
            
            if (index == -1)
                return new Null().Keyword;

            return value[(index + substring.Length)..value.Length];
        }
    }

    /// <summary>
    /// Returns the substring of the argument string, containing all the characters immediately preceding the first occurrence of the string passed in parameter. If the parameter value is `null` or `empty` then the function returns `empty`.
    /// </summary>
    class Before : BasePositionFunction
    {
        /// <param name="substring">The string to seek.</param>
        public Before(IScalarResolver<string> substring)
            : this(substring, new LiteralScalarResolver<int>(0)) { }

        /// <param name="substring">The string to seek.</param>
        /// <param name="count">The number of character positions to examine.</param>
        public Before(IScalarResolver<string> substring, IScalarResolver<int> count)
            : base(substring, count) { }

        protected override object EvaluateString(string value)
        {
            var substring = Substring.Execute();
            if (string.IsNullOrEmpty(substring) || new Empty().Equals(substring) || new Null().Equals(substring))
                return string.Empty;

            if (!value.Contains(substring))
                return string.Empty;

            var i = 0;
            var index = substring.Length * -1;
            do
            {
                index += substring.Length;
                index = value.IndexOf(substring, index);
                i += 1;
            }
            while (index != -1 && i <= Count.Execute());

            if (index == -1)
                return new Null().Keyword;

            return value[..index];
        }
    }
}
