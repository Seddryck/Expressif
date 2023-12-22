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
public abstract class BaseTimePartChangeFunction : BaseTemporalFunction
{ }

/// <summary>
/// returns a temporal value corresponding to the same instant of the argument value but with a new value for the second part.
/// </summary>
public class ChangeOfHour : BaseDatePartNumericFunction
{
    public Func<int> Hour { get; }
    public ChangeOfHour(Func<int> hour)
        => Hour = hour;

    protected override object EvaluateDateTime(DateTime value)
    {
        var newHour = Hour.Invoke();
        if (newHour < 0 || newHour > 23)
            return new Null();
        return new DateTime(value.Year, value.Month, value.Day, newHour, value.Minute, value.Second, value.Millisecond);
    }
}

/// <summary>
/// returns a temporal value corresponding to the same instant of the argument value but with a new value for the second part.
/// </summary>
public class ChangeOfMinute : BaseDatePartNumericFunction
{
    public Func<int> Minute { get; }
    public ChangeOfMinute(Func<int> minute)
        => Minute = minute;

    protected override object EvaluateDateTime(DateTime value)
    {
        var newMinute = Minute.Invoke();
        if (newMinute < 0 || newMinute > 59)
            return new Null();
        return new DateTime(value.Year, value.Month, value.Day, value.Hour, newMinute, value.Second, value.Millisecond);
    }
}

/// <summary>
/// returns a temporal value corresponding to the same instant of the argument value but with a new value for the second part.
/// </summary>
public class ChangeOfSecond : BaseDatePartNumericFunction
{
    public Func<int> Second { get; }
    public ChangeOfSecond(Func<int> second)
        => Second = second;

    protected override object EvaluateDateTime(DateTime value)
    {
        var newSecond = Second.Invoke();
        if (newSecond < 0 || newSecond > 59)
            return new Null();
        return new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, newSecond, value.Millisecond);
    }
}
