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
    abstract class BaseSubstringFunction : BaseTextFunction
    {
        public Func<string> Substring { get; }
        public Func<int> Count { get; }
        public BaseSubstringFunction(Func<string> substring, Func<int> count)
            => (Substring, Count) = (substring, count);
    }

    /// <summary>
    /// Returns the substring of the argument string, containing all the characters immediately following the first occurrence of the string passed in parameter. If the parameter value is `null` or `empty` then the argument value is returned.
    /// </summary>
    class AfterSubstring : BaseSubstringFunction
    {
        /// <param name="substring">The string to seek.</param>
        public AfterSubstring(Func<string> substring)
            : this(substring, () => 0) { }

        /// <param name="substring">The string to seek.</param>
        /// <param name="count">The number of character positions to examine.</param>
        public AfterSubstring(Func<string> substring, Func<int> count)
            : base(substring, count) { }

        protected override object EvaluateString(string value)
        {
            var substring = Substring.Invoke();
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
            while (index != -1 && i <= Count.Invoke());
            
            if (index == -1)
                return new Null().Keyword;

            return value[(index + substring.Length)..value.Length];
        }
    }

    /// <summary>
    /// Returns the substring of the argument string, containing all the characters immediately preceding the first occurrence of the string passed in parameter. If the parameter value is `null` or `empty` then the function returns `empty`.
    /// </summary>
    class BeforeSubstring : BaseSubstringFunction
    {
        /// <param name="substring">The string to seek.</param>
        public BeforeSubstring(Func<string> substring)
            : this(substring, () => 0) { }

        /// <param name="substring">The string to seek.</param>
        /// <param name="count">The number of character positions to examine.</param>
        public BeforeSubstring(Func<string> substring, Func<int> count)
            : base(substring, count) { }

        protected override object EvaluateString(string value)
        {
            var substring = Substring.Invoke();
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
            while (index != -1 && i <= Count.Invoke());

            if (index == -1)
                return new Null().Keyword;

            return value[..index];
        }
    }
}
