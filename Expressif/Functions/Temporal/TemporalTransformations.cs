using Expressif.Values;
using Expressif.Values.Casters;
using Expressif.Values.Special;
using System;


namespace Expressif.Functions.Temporal
{
    abstract class AbstractTemporalTransformation : IFunction
    {

        public object? Evaluate(object value)
        {
            return value switch
            {
                null => EvaluateNull(),
                DBNull _ => EvaluateNull(),
                DateOnly date => EvaluateDateTime(date.ToDateTime(TimeOnly.MinValue)),
                DateTime dt => EvaluateDateTime(dt),
                DateTimeOffset dto => EvaluateDateTime(dto.UtcDateTime),
                _ => EvaluateUncasted(value),
            };
        }

        protected virtual object? EvaluateUncasted(object value)
        {
            if (new Null().Equals(value))
                return EvaluateNull();

            var caster = new DateTimeCaster();
            var dateTime = caster.Execute(value);
            return EvaluateDateTime(dateTime);
        }

        protected virtual object? EvaluateNull() => null;
        protected abstract object EvaluateDateTime(DateTime value);
    }

    class DateTimeToDate : AbstractTemporalTransformation
    {
        protected override object EvaluateDateTime(DateTime value) => value.Date;
    }

    class DateToAge : AbstractTemporalTransformation
    {
        protected override object EvaluateNull() => 0;
        protected override object EvaluateDateTime(DateTime value)
        {
            // Save today's date.
            var today = DateTime.Today;
            // Calculate the age.
            var age = today.Year - value.Year;
            // Go back to the year the person was born in case of a leap year
            return value.AddYears(age) > today ? age-- : age;
        }
    }

    class DateTimeToFirstOfMonth : AbstractTemporalTransformation
    {
        protected override object EvaluateDateTime(DateTime value) => new DateTime(value.Year, value.Month, 1);
    }

    class DateTimeToFirstOfYear : AbstractTemporalTransformation
    {
        protected override object EvaluateDateTime(DateTime value) => new DateTime(value.Year, 1, 1);
    }

    class DateTimeToLastOfMonth : AbstractTemporalTransformation
    {
        protected override object EvaluateDateTime(DateTime value) => new DateTime(value.Year, value.Month, 1).AddMonths(1).AddDays(-1);
    }

    class DateTimeToLastOfYear : AbstractTemporalTransformation
    {
        protected override object EvaluateDateTime(DateTime value) => new DateTime(value.Year, 12, 31);
    }

    class DateTimeToNextDay : AbstractTemporalTransformation
    {
        protected override object EvaluateDateTime(DateTime value) => value.AddDays(1);
    }

    class DateTimeToNextMonth : AbstractTemporalTransformation
    {
        protected override object EvaluateDateTime(DateTime value) => value.AddMonths(1);
    }

    class DateTimeToNextYear : AbstractTemporalTransformation
    {
        protected override object EvaluateDateTime(DateTime value) => value.AddYears(1);
    }

    class DateTimeToPreviousDay : AbstractTemporalTransformation
    {
        protected override object EvaluateDateTime(DateTime value) => value.AddDays(-1);
    }

    class DateTimeToPreviousMonth : AbstractTemporalTransformation
    {
        protected override object EvaluateDateTime(DateTime value) => value.AddMonths(-1);
    }

    class DateTimeToPreviousYear : AbstractTemporalTransformation
    {
        protected override object EvaluateDateTime(DateTime value) => value.AddYears(-1);
    }

    class DateTimeToClip : AbstractTemporalTransformation
    {
        public IScalarResolver<DateTime> Min { get; }
        public IScalarResolver<DateTime> Max { get; }

        public DateTimeToClip(IScalarResolver<DateTime> min, IScalarResolver<DateTime> max)
            => (Min, Max) = (min, max);

        protected override object EvaluateDateTime(DateTime value)
            => (value < Min.Execute()) ? Min.Execute() : (value > Max.Execute()) ? Max.Execute() : value;
    }

    class DateTimeToSetTime : AbstractTemporalTransformation
    {
        public IScalarResolver<string> Instant { get; }

        public DateTimeToSetTime(IScalarResolver<string> instant)
            => Instant = instant;

        protected override object EvaluateDateTime(DateTime value)
        {
            var time = TimeSpan.Parse(Instant.Execute()!);
            return new DateTime(value.Year, value.Month, value.Day, time.Hours, time.Minutes, time.Seconds);
        }
    }

    class NullToDate : AbstractTemporalTransformation
    {
        public IScalarResolver<DateTime> Default { get; }

        public NullToDate(IScalarResolver<DateTime> dt)
            => Default = dt;

        protected override object EvaluateNull() => Default.Execute();
        protected override object EvaluateDateTime(DateTime value) => value;
    }

    class InvalidToDate : AbstractTemporalTransformation
    {
        public IScalarResolver<DateTime> Default { get; }

        public InvalidToDate(IScalarResolver<DateTime> dt)
            => Default = dt;

        protected override object EvaluateNull() => new Null();
        protected override object EvaluateDateTime(DateTime value) => value;
        protected override object? EvaluateUncasted(object value)
        {
            if (new Null().Equals(value))
                return EvaluateNull();

            var caster = new DateTimeCaster();
            
            try { return caster.Execute(value);} 
            catch { return Default.Execute(); }
        }
    }

    class DateTimeToFloorHour : AbstractTemporalTransformation
    {
        protected override object EvaluateDateTime(DateTime value)
            => value.AddTicks(-1 * (value.Ticks % TimeSpan.TicksPerHour));
    }

    class DateTimeToCeilingHour : AbstractTemporalTransformation
    {
        protected override object EvaluateDateTime(DateTime value)
            => value.AddTicks(TimeSpan.TicksPerHour - (value.Ticks % TimeSpan.TicksPerHour == 0 ? TimeSpan.TicksPerHour : value.Ticks % TimeSpan.TicksPerHour));
    }

    class DateTimeToFloorMinute : AbstractTemporalTransformation
    {
        protected override object EvaluateDateTime(DateTime value)
            => value.AddTicks(-1 * (value.Ticks % TimeSpan.TicksPerMinute));
    }

    class DateTimeToCeilingMinute : AbstractTemporalTransformation
    {
        protected override object EvaluateDateTime(DateTime value)
            => value.AddTicks(TimeSpan.TicksPerMinute - (value.Ticks % TimeSpan.TicksPerMinute == 0 ? TimeSpan.TicksPerMinute : value.Ticks % TimeSpan.TicksPerMinute));
    }

    class DateTimeToAdd : AbstractTemporalTransformation
    {
        public IScalarResolver<int> Times { get; }
        public IScalarResolver<string> TimeSpan { get; }

        public DateTimeToAdd(IScalarResolver<string> timeSpan, IScalarResolver<int> times)
            => (TimeSpan, Times) = (timeSpan, times);

        public DateTimeToAdd(IScalarResolver<string> timeSpan)
            : this(timeSpan, new LiteralScalarResolver<int>(1)) { }

        protected override object EvaluateDateTime(DateTime value)
            => value.AddTicks(System.TimeSpan.Parse(TimeSpan.Execute()!).Ticks * Times.Execute());
    }

    class DateTimeToSubtract : DateTimeToAdd
    {
        public DateTimeToSubtract(IScalarResolver<string> timeSpan, IScalarResolver<int> times)
            : base(timeSpan, times) { }

        public DateTimeToSubtract(IScalarResolver<string> timeSpan)
            : base(timeSpan) { }

        protected override object EvaluateDateTime(DateTime value)
            => value.AddTicks(System.TimeSpan.Parse(TimeSpan.Execute()!).Ticks * Times.Execute() * -1);
    }
}
