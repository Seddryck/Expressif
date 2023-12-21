using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Values;
using Expressif.Values.Casters;
using Expressif.Values.Special;

namespace Expressif.Functions.Temporal;
public abstract class BaseDatePartFunction : BaseTemporalFunction
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
/// returns a textual value at format YYYY representing the year of the date passed as the argument
/// </summary>
public class Year : BaseDatePartFunction
{
    protected override object EvaluateDateTime(DateTime value) => value.Year.ToString("D4");
    protected override object? EvaluateInteger(int numeric) => numeric.ToString("D4");
    protected override object? EvaluateYearMonth(YearMonth yearMonth) => yearMonth.Year.ToString("D4");
}

/// <summary>
/// returns a textual value at format MM representing the month of the date passed as the argument
/// </summary>
public class Month : BaseDatePartFunction
{
    protected override object EvaluateDateTime(DateTime value) => value.Month.ToString("D2");
    protected override object? EvaluateInteger(int numeric)
        => numeric >=1 && numeric <=12  
            ? numeric.ToString("D2")
            : throw new ArgumentOutOfRangeException($"The value of month cannot be less than 1 or more than 12. Current value is '{numeric}'.");
    protected override object? EvaluateYearMonth(YearMonth yearMonth) => yearMonth.Month.ToString("D2");
}

/// <summary>
/// returns a textual value at format MM-DD representing the month and day of the date passed as the argument
/// </summary>
public class MonthDay : BaseDatePartFunction
{
    protected override object EvaluateDateTime(DateTime value) => $"{value.Month:D2}-{value.Day:D2}";
}

/// <summary>
/// returns a textual value at format YYYY-Www representing the year and week number (according to ISO 8601) of the date passed as the argument
/// </summary>
public class YearWeek : BaseDatePartFunction
{
    protected override object EvaluateDateTime(DateTime value)
        => $"{ISOWeek.GetYear(value):D4}-W{ISOWeek.GetWeekOfYear(value):D2}";
}

/// <summary>
/// returns a textual value at format YYYY-Www-D representing the year and week number (according to ISO 8601),
/// and the day number (1 being Monday) of the date passed as the argument
/// </summary>
public class YearWeekDay : BaseDatePartFunction
{
    protected override object EvaluateDateTime(DateTime value)
        => $"{ISOWeek.GetYear(value):D4}-W{ISOWeek.GetWeekOfYear(value):D2}-{(value.DayOfWeek==0 ? 7 : (int)value.DayOfWeek)}";
}

/// <summary>
///returns a textual value at format YYYY-ddd representing the year,
/// and the day number of the date passed as the argument (both according to ISO 8601)
/// </summary>
public class YearDay : BaseDatePartFunction
{
    protected override object EvaluateDateTime(DateTime value)
    {
        var year = ISOWeek.GetYear(value);
        var day = value.Subtract(ISOWeek.GetYearStart(year)).Days + 1;
        return $"{year:D4}-{day:D3}";
    }
}
