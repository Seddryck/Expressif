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
public abstract class BaseTimePartTextualFunction : BaseTemporalFunction
{ }

/// <summary>
/// returns a textual value at format hh (24 hours format) representing the hours of the dateTime passed as the argument
/// </summary>
public class Hour : BaseDatePartTextualFunction
{
    protected override object EvaluateDateTime(DateTime value) => value.Hour.ToString("D2");
}

/// <summary>
/// returns a textual value at format hh:mm (24 hours format) representing the hours and minutes of the dateTime passed as the argument
/// </summary>
public class HourMinute : BaseDatePartTextualFunction
{
    protected override object EvaluateDateTime(DateTime value) => $"{value.Hour:D2}:{value.Minute:D2}";
}

/// <summary>
/// returns a textual value at format hh:mm:ss (24 hours format) representing the hours, minutes, and seconds of the dateTime passed as the argument
/// </summary>
public class HourMinuteSecond : BaseDatePartTextualFunction
{
    protected override object EvaluateDateTime(DateTime value) => $"{value.Hour:D2}:{value.Minute:D2}:{value.Second:D2}";
}
