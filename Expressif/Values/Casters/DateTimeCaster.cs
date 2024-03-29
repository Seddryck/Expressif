﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Values.Casters;

public class DateTimeCaster : ICaster<DateTime>, IParser<DateTime>
{
    protected readonly DateTimeStyles Style = DateTimeStyles.AllowWhiteSpaces
                                            | DateTimeStyles.NoCurrentDateDefault
                                            | DateTimeStyles.AdjustToUniversal;

    protected readonly DateTimeFormatInfo Format = CultureInfo.InvariantCulture.DateTimeFormat;

    public virtual bool TryCast(object obj, [NotNullWhen(true)] out DateTime value)
        => obj switch
        {
            YearMonth yearMonth => (value = new DateTime(yearMonth.Year, yearMonth.Month, 1)) == value,
            DateTime dt => (value = dt) == value,
            DateOnly d => (value = d.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)) == value,
            string s => TryParse(s, out value),
            _ => (value = default) != value
        };

    public virtual DateTime Cast(object obj)
        => TryCast(obj, out var dt)
            ? dt
            : throw new InvalidCastException($"Cannot cast an object of type '{obj.GetType().FullName}' to type {nameof(DateTime)}. The type {nameof(DateTime)} can only be casted from types YearMonth, DateOnly and String. The expect string format is '{Format.FullDateTimePattern}'");

    public virtual bool TryParse(string text, [NotNullWhen(true)] out DateTime value)
        => DateTime.TryParse(text, Format, Style, out value);

    public virtual DateTime Parse(string text)
        => TryParse(text, out var value) ? value : throw new FormatException();
}
