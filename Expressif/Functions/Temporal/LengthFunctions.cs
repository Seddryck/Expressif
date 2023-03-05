using Expressif.Predicates.Special;
using Expressif.Values;
using Expressif.Values.Casters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Functions.Temporal
{
    public abstract class BaseTemporalLengthFunction : BaseTemporalFunction
    {
        protected override object? EvaluateUncasted(object value)
        {
            if (new Null().Equals(value))
                return EvaluateNull();

            if (new IntegerCaster().TryCast(value, out var integer))
                return EvaluateInteger(integer);

            if (new YearMonthCaster().TryCast(value, out var yearMonth))
                return EvaluateYearMonth(yearMonth);

            if (new DateTimeCaster().TryCast(value, out var dateTime))
                return EvaluateDateTime(dateTime);

            return null;
        }

        protected virtual object? EvaluateInteger(int numeric) => null;
        protected virtual object? EvaluateYearMonth(YearMonth yearMonth) => null;
    }

    /// <summary>
    /// Returns the count of days within the year of the dateTime value passed as the argument.
    /// If the argument is not a dateTime but an integer, returns count of days of the corresponding year.
    /// It returns 365 or 366 (for leap years).
    /// </summary>
    public class LengthOfYear : BaseTemporalLengthFunction
    {
        protected override object EvaluateDateTime(DateTime value)
            => 365 + (DateTime.IsLeapYear(value.Year) ? 1 : 0);

        protected override object EvaluateInteger(int value)
            => 365 + (DateTime.IsLeapYear(value) ? 1 : 0);
    }

    /// <summary>
    /// returns the count of days within the month of the dateTime value passed as the argument. 
    /// If the argument is not a dateTime but a text at format "YYYY-MM", it returns count of days of the month represented by this value. 
    /// It returns a value between 28 and 31 (depending of leap year and month).
    /// </summary>
    public class LengthOfMonth : BaseTemporalLengthFunction
    {
        private static readonly int[] LongMonths = { 1, 3, 5, 7, 8, 10, 12 };
        protected override object EvaluateDateTime(DateTime value)
            => EvaluateYearMonth(new YearMonth(value.Year, value.Month));

        protected override object EvaluateYearMonth(YearMonth yearMonth)
            => (yearMonth.Month == 2)
                ? 28 + (DateTime.IsLeapYear(yearMonth.Year) ? 1 : 0)
                : 30 + (LongMonths.Contains(yearMonth.Month) ? 1 : 0);
    }
}