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
    abstract class AbstractTextPosition : BaseTextFunction
    {
        public IScalarResolver<string> Substring { get; }
        public IScalarResolver<int> Count { get; }
        public AbstractTextPosition(IScalarResolver<string> substring, IScalarResolver<int> count)
            => (Substring, Count) = (substring, count);
    }

    /// <summary>
    /// Returns the substring of the argument string, containing all the characters immediately following the first occurrence of the string passed in parameter. If the parameter value is `null` or `empty` then the argument value is returned.
    /// </summary>
    class TextToAfter : AbstractTextPosition
    {
        /// <param name="substring">The string to seek.</param>
        public TextToAfter(IScalarResolver<string> substring)
            : this(substring, new LiteralScalarResolver<int>(1)) { }

        /// <param name="substring">The string to seek.</param>
        /// <param name="count">The number of character positions to examine.</param>
        public TextToAfter(IScalarResolver<string> substring, IScalarResolver<int> count)
            : base(substring, count) { }

        protected override object EvaluateString(string value)
        {
            var substring = Substring.Execute();
            if (string.IsNullOrEmpty(substring) || new Empty().Equals(substring) || new Null().Equals(substring))
                return value;

            if (!value.Contains(substring))
                return string.Empty;

            var index = value.IndexOf(substring, 0, Count.Execute()) + substring.Length;
            return value[index .. value.Length];
        }
    }

    /// <summary>
    /// Returns the substring of the argument string, containing all the characters immediately preceding the first occurrence of the string passed in parameter. If the parameter value is `null` or `empty` then the function returns `empty`.
    /// </summary>
    class TextToBefore : AbstractTextPosition
    {
        /// <param name="substring">The string to seek.</param>
        public TextToBefore(IScalarResolver<string> substring)
            : this(substring, new LiteralScalarResolver<int>(1)) { }

        /// <param name="substring">The string to seek.</param>
        /// <param name="count">The number of character positions to examine.</param>
        public TextToBefore(IScalarResolver<string> substring, IScalarResolver<int> count)
            : base(substring, count) { }

        protected override object EvaluateString(string value)
        {
            var substring = Substring.Execute();
            if (string.IsNullOrEmpty(substring) || new Empty().Equals(substring) || new Null().Equals(substring))
                return string.Empty;

            if (!value.Contains(substring))
                return string.Empty;

            var index = value.IndexOf(substring, 0, Count.Execute());
            return value[..index];
        }
    }
}
