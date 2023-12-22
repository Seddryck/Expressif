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
public abstract class BaseTimePartNumericFunction : BaseTemporalFunction
{ }

/// <summary>
/// returns a numeric value representing the hours of the date passed as the argument
/// </summary>
public class HourOfDay : BaseDatePartNumericFunction
{
    protected override object EvaluateDateTime(DateTime value) => value.Hour;
}

/// <summary>
/// returns a numeric value representing the minutes of the hour passed as the argument
/// </summary>
public class MinuteOfHour : BaseDatePartNumericFunction
{
    protected override object EvaluateDateTime(DateTime value) => value.Minute;
}

/// <summary>
/// returns a numeric value representing the minutes of the date passed as the argument
/// </summary>
public class MinuteOfDay : BaseDatePartNumericFunction
{
    protected override object EvaluateDateTime(DateTime value) => value.Hour * 60 + value.Minute;
}

/// <summary>
/// returns a numeric value representing the seconds of the minute of the date passed as the argument
/// </summary>
public class SecondOfMinute : BaseDatePartNumericFunction
{
    protected override object EvaluateDateTime(DateTime value) => value.Second;
}

/// <summary>
/// returns a numeric value representing the seconds of the hour of the date passed as the argument
/// </summary>
public class SecondOfHour : BaseDatePartNumericFunction
{
    protected override object EvaluateDateTime(DateTime value) => value.Minute * 60 + value.Second;
}

/// <summary>
/// returns a numeric value representing the seconds of the day of the date passed as the argument
/// </summary>
public class SecondOfDay : BaseDatePartNumericFunction
{
    protected override object EvaluateDateTime(DateTime value) => (value.Hour * 60 + value.Minute) * 60 + value.Second;
}
