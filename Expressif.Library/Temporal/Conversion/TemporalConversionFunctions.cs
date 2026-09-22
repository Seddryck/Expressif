using System;
using System.Globalization;
using Expressif.Values;
using Expressif.Values.Casters;
using Expressif.Values.Special;
using Expressif.Library.Temporal;

namespace Expressif.Library.Temporal.Conversion;

/// <summary>
/// Returns the date at midnight of the argument dateTime.
/// </summary>
[Function(prefix: "", aliases: ["dateTime-to-date"])]
[Scope("temporal/conversion")]
public class DateTimeToDate : BaseTemporalFunction
{
    protected override object EvaluateDateTime(DateTime value) => value.Date;
}

/// <summary>
/// Returns the dateTime argument except if the value is `null` then it returns the parameter value.
/// </summary>
[Function(prefix: "")]
[Scope("temporal/conversion")]
public class NullToDate : BaseTemporalFunction
{
    public Func<DateTime> Default { get; }

    /// <param name="default">The dateTime to be returned if the argument is `null`.</param>
    public NullToDate(Func<DateTime> @default)
        => Default = @default;

    protected override object EvaluateNull() => Default.Invoke();
    protected override object EvaluateDateTime(DateTime value) => value;
}

/// <summary>
/// Returns the dateTime argument except if the value is not a valid dateTime then it returns the parameter value.
/// </summary>
[Function(prefix: "")]
[Scope("temporal/conversion")]
public class InvalidToDate : BaseTemporalFunction
{
    public Func<DateTime> Default { get; }

    /// <param name="default">The dateTime to be returned if the argument is not a valid dateTime.</param>
    public InvalidToDate(Func<DateTime> @default)
        => Default = @default;

    protected override object EvaluateNull() => Expressif.Values.Special.Null.Instance;
    protected override object EvaluateDateTime(DateTime value) => value;
    protected override object? EvaluateUncasted(object value)
    {
        if (Expressif.Values.Special.Null.Instance.Equals(value))
            return EvaluateNull();

        var caster = new DateTimeCaster();

        try { return caster.Cast(value); }
        catch { return Default.Invoke(); }
    }
}
