using System;
using System.Collections.Generic;
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

    protected abstract object? EvaluateInteger(int numeric);
    protected abstract object? EvaluateYearMonth(YearMonth yearMonth);
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

