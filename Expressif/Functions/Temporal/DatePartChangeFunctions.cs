using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions.Special;
using Expressif.Values;
using Expressif.Values.Casters;
using Expressif.Values.Special;

namespace Expressif.Functions.Temporal;
public abstract class BaseDatePartChangeFunction : BaseTemporalFunction
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
/// returns a temporal value corresponding to the same day and month of the argument value but of the year passed as the parameter.
/// If the original date was the 29th of February and the year passed as a parameter is not a leap year then it returns the 28th of February.
/// </summary>
public class ChangeOfYear : BaseDatePartNumericFunction
{
    public Func<int> Year { get; }
    public ChangeOfYear(Func<int> year)
        => Year = year;

    protected override object EvaluateDateTime(DateTime value)
    {
        var newYear = Year.Invoke();
        if (newYear < 1 || newYear > 9999)
            return new Null();
        var newDay = value.Month == 2 && value.Day == 29 && !DateTime.IsLeapYear(newYear) ? 28 : value.Day;
        return new DateTime(newYear, value.Month, newDay, value.Hour, value.Minute, value.Second, value.Millisecond);
    }
    protected override object? EvaluateInteger(int numeric) => Year.Invoke();
    protected override object? EvaluateYearMonth(YearMonth yearMonth) => new YearMonth(Year.Invoke(), yearMonth.Month);
}

/// <summary>
/// returns a temporal value corresponding to the same day and year of the argument value but of the month passed as the parameter.
/// If the original day is 29, 30, or 31 and the new month passed as a parameter has fewer days then it returns the last day of the corresponding month.
/// </summary>
public class ChangeOfMonth : BaseDatePartChangeFunction
{
    public Func<int> Month { get; }
    public ChangeOfMonth(Func<int> month)
        => Month = month;

    protected override object EvaluateDateTime(DateTime value)
    {
        var newMonth = Month.Invoke();
        if (newMonth < 1 || newMonth > 12)
            return new Null();
        var lastDayOfMonth = new DateTime(value.Year, newMonth, 1).AddMonths(1).AddDays(-1).Day;
        var newDay = value.Day > lastDayOfMonth ? lastDayOfMonth : value.Day;
        return new DateTime(value.Year, newMonth, newDay, value.Hour, value.Minute, value.Second, value.Millisecond);
    }
    protected override object? EvaluateYearMonth(YearMonth yearMonth)
    {
        var newMonth = Month.Invoke();
        if (newMonth < 1 || newMonth > 12)
            return new Null();
        return new YearMonth(yearMonth.Year, newMonth);
    }
}
