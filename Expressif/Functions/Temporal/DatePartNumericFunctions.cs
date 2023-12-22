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
public abstract class BaseDatePartNumericFunction : BaseTemporalFunction
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
/// returns a numeric value representing the year of the date passed as the argument
/// </summary>
public class YearOfEra : BaseDatePartNumericFunction
{
    protected override object EvaluateDateTime(DateTime value) => value.Year;
    protected override object? EvaluateInteger(int numeric) => numeric;
    protected override object? EvaluateYearMonth(YearMonth yearMonth) => yearMonth.Year;
}

/// <summary>
/// returns a numeric value representing the month of the date passed as the argument
/// </summary>
public class MonthOfYear : BaseDatePartNumericFunction
{
    protected override object EvaluateDateTime(DateTime value) => value.Month;
    protected override object? EvaluateYearMonth(YearMonth yearMonth) => yearMonth.Month;
}

/// <summary>
/// returns a numeric value representing the day of the week (1 being Monday and 7 being Sunday) of the date passed as the argument
/// </summary>
public class DayOfWeek : BaseDatePartNumericFunction
{
    protected override object EvaluateDateTime(DateTime value) => value.DayOfWeek == 0 ? 7 : (int)value.DayOfWeek;
}

/// <summary>
/// returns a numeric value representing the day of the month of the date passed as the argument
/// </summary>
public class DayOfMonth : BaseDatePartNumericFunction
{
    protected override object EvaluateDateTime(DateTime value) => value.Day;
}

/// <summary>
/// returns a numeric value representing the day position within the year of the date passed as the argument
/// </summary>
public class DayOfYear : BaseDatePartNumericFunction
{
    protected override object EvaluateDateTime(DateTime value) => value.DayOfYear;
}

/// <summary>
/// returns a numeric value representing the day position within the year of the date passed as the argument
/// </summary>
public class IsoDayOfYear : BaseDatePartNumericFunction
{
    protected override object EvaluateDateTime(DateTime value)
        => value.Subtract(ISOWeek.GetYearStart(ISOWeek.GetYear(value))).Days + 1;
}

/// <summary>
/// returns a textual value at format YYYY-Www representing the year and week number (according to ISO 8601) of the date passed as the argument
/// </summary>
public class IsoWeekOfYear : BaseDatePartNumericFunction
{
    protected override object EvaluateDateTime(DateTime value)
        => ISOWeek.GetWeekOfYear(value);
}

