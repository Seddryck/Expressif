using Expressif.Values;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Expressif.Predicates.Text
{
    [Predicate(false)]
    abstract class BaseTextPredicateSubstring : BaseTextPredicateReference
    {
        protected StringComparison Comparison { get; }

        public BaseTextPredicateSubstring(IScalarResolver<string> reference)
            : this(reference, StringComparer.InvariantCultureIgnoreCase) { }

        public BaseTextPredicateSubstring(IScalarResolver<string> reference, StringComparer comparer)
            : base(reference) { Comparison = GetComparison(comparer); }

        protected virtual StringComparison GetComparison(StringComparer comparer)
        {
            var map = new Dictionary<StringComparer, StringComparison>
            {
                { StringComparer.InvariantCulture, StringComparison.InvariantCulture },
                { StringComparer.InvariantCultureIgnoreCase, StringComparison.InvariantCultureIgnoreCase },
            };
            if (map.TryGetValue(comparer, out var comparison))
                return comparison;
            else
                throw new ArgumentException(nameof(comparer));
        }
    }

    /// <summary>
    /// Returns `true` if the value passed as argument starts with the text value passed as parameter. Returns `false` otherwise.
    /// </summary>
    class StartsWith : BaseTextPredicateSubstring
    {
        /// <param name="reference">A string to be compared to the argument value</param>
        public StartsWith(IScalarResolver<string> reference)
            : base(reference, StringComparer.InvariantCultureIgnoreCase) { }

        /// <param name="reference">A string to be compared to the argument value</param>
        /// <param name="comparer">A definition of the parameters of the comparison (case-sensitivity, culture-sensitivity)</param>
        public StartsWith(IScalarResolver<string> reference, StringComparer comparer)
            : base(reference, comparer) { }

        protected override bool EvaluateText(string value, string reference)
            => value.StartsWith(reference, Comparison);
    }

    /// <summary>
    /// Returns `true` if the value passed as argument ends with the text value passed as parameter. Returns `false` otherwise.
    /// </summary>
    class EndsWith : BaseTextPredicateSubstring
    {
        /// <param name="reference">A string to be compared to the argument value</param>
        public EndsWith(IScalarResolver<string> reference)
            : base(reference, StringComparer.InvariantCultureIgnoreCase) { }

        /// <param name="reference">A string to be compared to the argument value</param>
        /// <param name="comparer">A definition of the parameters of the comparison (case-sensitivity, culture-sensitivity)</param>
        public EndsWith(IScalarResolver<string> reference, StringComparer comparer)
            : base(reference, comparer) { }

        protected override bool EvaluateText(string value, string reference)
            => value.EndsWith(reference, Comparison);
    }

    /// <summary>
    /// Returns `true` if the value passed as argument contains, anywhere in the string, the text value passed as parameter. Returns `false` otherwise.
    /// </summary>
    class Contains : BaseTextPredicateSubstring
    {
        /// <param name="reference">A string to be compared to the argument value</param>
        public Contains(IScalarResolver<string> reference)
            : base(reference, StringComparer.InvariantCultureIgnoreCase) { }

        /// <param name="reference">A string to be compared to the argument value</param>
        /// <param name="comparer">A definition of the parameters of the comparison (case-sensitivity, culture-sensitivity)</param>
        public Contains(IScalarResolver<string> reference, StringComparer comparer)
            : base(reference, comparer) { }

        protected override bool EvaluateText(string value, string reference)
            => value.Contains(reference, Comparison);
    }

    /// <summary>
    /// Returns `true` if the value passed as argument validate the regex passed as parameter. Returns `false` otherwise.
    /// </summary>
    class MatchesRegex : BaseTextPredicateSubstring
    {
        /// <param name="regex">A string to be compared to the argument value</param>
        public MatchesRegex(IScalarResolver<string> regex)
            : base(regex, StringComparer.InvariantCultureIgnoreCase) { }

        /// <param name="regex">A string to be compared to the argument value</param>
        /// <param name="comparer">A definition of the parameters of the comparison (case-sensitivity, culture-sensitivity)</param>
        public MatchesRegex(IScalarResolver<string> regex, StringComparer comparer)
            : base(regex, comparer) { }

        protected override bool EvaluateText(string value, string reference)
        {
            var regexOption = Comparison == StringComparison.InvariantCultureIgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
            var regex = new Regex(reference, regexOption);
            return regex.IsMatch(value.ToString());
        }
    }

}
