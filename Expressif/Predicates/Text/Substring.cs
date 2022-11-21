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

    class StartsWith : BaseTextPredicateSubstring
    {
        public StartsWith(IScalarResolver<string> reference)
            : base(reference, StringComparer.InvariantCultureIgnoreCase) { }

        public StartsWith(IScalarResolver<string> reference, StringComparer comparer)
            : base(reference,comparer) {}

        protected override bool EvaluateText(string value, string reference)
            => value.StartsWith(reference, Comparison);
    }

    class EndsWith : BaseTextPredicateSubstring
    {
        public EndsWith(IScalarResolver<string> reference)
            : base(reference, StringComparer.InvariantCultureIgnoreCase) { }

        public EndsWith(IScalarResolver<string> reference, StringComparer comparer)
            : base(reference, comparer) { }

        protected override bool EvaluateText(string value, string reference)
            => value.EndsWith(reference, Comparison);
    }

    class Contains : BaseTextPredicateSubstring
    {
        public Contains(IScalarResolver<string> reference)
            : base(reference, StringComparer.InvariantCultureIgnoreCase) { }

        public Contains(IScalarResolver<string> reference, StringComparer comparer)
            : base(reference, comparer) { }

        protected override bool EvaluateText(string value, string reference)
            => value.Contains(reference, Comparison);
    }
    class MatchesRegex : BaseTextPredicateSubstring
    {
        public MatchesRegex(IScalarResolver<string> regex)
            : base(regex, StringComparer.InvariantCultureIgnoreCase) { }
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
