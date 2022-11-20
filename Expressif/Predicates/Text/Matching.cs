using Expressif.Values;
using Expressif.Values.Resolvers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Text
{
    abstract class BaseTextPredicateMatching : BaseTextPredicate
    {
        protected CultureInfo CultureInfo { get; }

        public BaseTextPredicateMatching()
            : this(new LiteralScalarResolver<string>(string.Empty)) { }
        public BaseTextPredicateMatching(IScalarResolver<string> culture)
            => CultureInfo = GetCulture(culture.Execute() ?? string.Empty);

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

    class MatchesNumeric : BaseTextPredicateMatching
    {
        public MatchesNumeric()
            : base() { }
        public MatchesNumeric(IScalarResolver<string> culture)
            : base(culture) { }

        protected override bool EvaluateUncasted(object value)
            => value switch
            {
                Byte => true,
                SByte => true,
                Int16 => true,
                Int32 => true,
                Int64 => true,
                #if NET7_0_OR_GREATER
                Int128 => true,
                #endif
                UInt16 => true,
                UInt32 => true,
                UInt64 => true,
                #if NET7_0_OR_GREATER
                UInt128 => true,
                Half => true,
                #endif
                Single=> true,
                Double => true,
                Decimal => true,
                _ => base.EvaluateUncasted(value),
            };

        protected override bool EvaluateBaseText(string value)
            => decimal.TryParse(value, NumberStyles.Number & ~NumberStyles.AllowThousands, CultureInfo.NumberFormat, out var _);
    }

    class MatchesDate : BaseTextPredicateMatching
    {
        public MatchesDate()
            : base() { }
        public MatchesDate(IScalarResolver<string> culture)
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

        protected virtual bool EvaluateDateTime(DateTime dt)
            => dt.Equals(dt.Date);
    }

    class MatchesDateTime : BaseTextPredicateMatching
    {
        private string Pattern { get => CultureInfo.DateTimeFormat.ShortDatePattern + " " + CultureInfo.DateTimeFormat.LongTimePattern; }

        public MatchesDateTime()
            : base() { }
        public MatchesDateTime(IScalarResolver<string> culture)
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

    class MatchesTime : BaseTextPredicateMatching
    {
        public MatchesTime()
            : base() { }
        public MatchesTime(IScalarResolver<string> culture)
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
