using Expressif.Values;
using Expressif.Values.Casters;
using Expressif.Values.Resolvers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Text
{
    [Predicate(false)]
    public abstract class BaseTextPredicateMatching : BaseTextPredicate
    {
        protected CultureInfo CultureInfo { get; }

        public BaseTextPredicateMatching()
            : this(() => string.Empty) { }
        public BaseTextPredicateMatching(Func<string> culture)
            => CultureInfo = GetCulture(culture.Invoke() ?? string.Empty);

        protected virtual CultureInfo GetCulture(string culture)
        {
            if (!string.IsNullOrEmpty(culture))
                return (new CultureInfo(culture).Clone() as CultureInfo)!;

            var invariantCulture = (CultureInfo.InvariantCulture.Clone() as CultureInfo)!;
            invariantCulture.DateTimeFormat.ShortDatePattern = "yyyy/MM/dd";
            invariantCulture.DateTimeFormat.DateSeparator = "-";
            return invariantCulture;
        }
    }

    /// <summary>
    /// Returns `true` if the text value passed as argument is a valid representation of a numeric in the culture specified as parameter. Returns `false` otherwise. 
    /// </summary>
    public class MatchesNumeric : BaseTextPredicateMatching
    {
        public MatchesNumeric()
            : base() { }
        public MatchesNumeric(Func<string> culture)
            : base(culture) { }

        protected override bool EvaluateUncasted(object value)
            => TypeChecker.IsNumericType(value) || base.EvaluateUncasted(value);

        protected override bool EvaluateBaseText(string value)
            => decimal.TryParse(value, NumberStyles.Number & ~NumberStyles.AllowThousands, CultureInfo.NumberFormat, out var _);
    }

    /// <summary>
    /// Returns `true` if the text value passed as argument is a valid representation of a date in the culture specified as parameter. If the value is of type `DateTime` and the time part is set to midnight then it returns `true`. If the value is of type `Date`. Returns `false` otherwise. 
    /// </summary>
    public class MatchesDate : BaseTextPredicateMatching
    {
        public MatchesDate()
            : base() { }
        public MatchesDate(Func<string> culture)
            : base(culture) { }

        protected override bool EvaluateUncasted(object value)
            => value switch
            {
                DateTime dt => EvaluateDateTime(dt),
                DateOnly => true,
                TimeOnly => false,
                _ => base.EvaluateUncasted(value),
            };

        protected override bool EvaluateBaseText(string value)
            => DateTime.TryParseExact(value, CultureInfo.DateTimeFormat.ShortDatePattern, CultureInfo, DateTimeStyles.None, out var _);

        protected virtual bool EvaluateDateTime(System.DateTime dt)
            => dt.Equals(dt.Date);
    }

    /// <summary>
    /// Returns `true` if the text value passed as argument is a valid representation of a dateTime in the culture specified as parameter. The expected format is the concatenation of the ShortDatePattern, a space and the LongTimePattern. If the value is of type `DateTime`, it returns `true`. Returns `false` otherwise. 
    /// </summary>
    public class MatchesDateTime : BaseTextPredicateMatching
    {
        private string Pattern { get => CultureInfo.DateTimeFormat.ShortDatePattern + " " + CultureInfo.DateTimeFormat.LongTimePattern; }

        public MatchesDateTime()
            : base() { }
        public MatchesDateTime(Func<string> culture)
            : base(culture) { }

        protected override bool EvaluateUncasted(object value)
            => value switch
            {
                DateTime => true,
                DateOnly => true,
                TimeOnly => false,
                _ => base.EvaluateUncasted(value),
            };

        protected override bool EvaluateBaseText(string value)
            => DateTime.TryParseExact(value, Pattern, CultureInfo, DateTimeStyles.None, out var _);
    }

    /// <summary>
    /// Returns `true` if the text value passed as argument is a valid representation of a time in the culture specified as parameter. The expected format is the LongTimePattern. If the value is of type `TimeOnly`, it returns `true`. Returns `false` otherwise. 
    /// </summary>
    public class MatchesTime : BaseTextPredicateMatching
    {
        public MatchesTime()
            : base() { }
        public MatchesTime(Func<string> culture)
            : base(culture) { }

        protected override bool EvaluateUncasted(object value)
            => value switch
            {
                DateTime => false,
                DateOnly => false,
                TimeOnly => true,
                TimeSpan ts => EvaluateTimeSpan(ts),
                _ => base.EvaluateUncasted(value),
            };

        protected override bool EvaluateBaseText(string value)
            => DateTime.TryParseExact(value, CultureInfo.DateTimeFormat.LongTimePattern, CultureInfo, DateTimeStyles.None, out var _);

        protected virtual bool EvaluateTimeSpan(TimeSpan ts)
            => ts.TotalHours < 24;
    }
}
