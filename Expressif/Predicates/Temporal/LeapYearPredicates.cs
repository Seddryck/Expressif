using Expressif.Functions.Numeric;
using Expressif.Predicates.Special;
using Expressif.Values;
using Expressif.Values.Casters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Temporal;

public abstract class BaseTemporalLeapYearPredicate : BaseDateTimePredicate
{
    public override bool Evaluate(object? value)
    {
        return value switch
        {
            null => EvaluateNull(),
            DBNull => EvaluateNull(),
            DateTime dt => EvaluateDateTime(dt),
            DateOnly d => EvaluateDate(d),
            YearMonth ym => EvaluateYearMonth(ym),
            int year => EvaluateYear(year),
            _ => EvaluateUncasted(value),
        };
    }

    protected override bool EvaluateUncasted(object value)
    {
        if (new IntegerCaster().TryCast(value, out var year))
            return EvaluateYear(year);
        if (new YearMonthCaster().TryCast(value, out var ym))
            return EvaluateYearMonth(ym);
        return base.EvaluateUncasted(value);
    }

    protected abstract bool EvaluateYearMonth(YearMonth ym);
    protected abstract bool EvaluateYear(int year);
}

/// <summary>
/// Returns true if the year of the dateTime value passed as the argument is a leap year. If the argument is not a dateTime but a numeric, returns true if the integer part of this value corresponds to a year that is a leap year. Returns false otherwise.
/// </summary>
public class LeapYear : BaseTemporalLeapYearPredicate
{
    protected override bool EvaluateDateTime(DateTime dt)
        => EvaluateYear(dt.Year);
    protected override bool EvaluateYear(int year)
        => DateTime.IsLeapYear(year);
    protected override bool EvaluateYearMonth(YearMonth ym)
        => EvaluateYear(ym.Year);
}
