using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Expressif.Values.Casters;

public class DateOnlyCaster : ICaster<DateOnly>, IParser<DateOnly>
{
    public virtual bool TryCast(object obj, [NotNullWhen(true)] out DateOnly value)
        => obj switch
        {
            DateOnly date => (value = date) == value,
            DateTime dateTime => (value = DateOnly.FromDateTime(dateTime)) == value,
            YearMonth yearMonth => (value = new DateOnly(yearMonth.Year, yearMonth.Month, 1)) == value,
            string text => TryParse(text, out value),
            _ => (value = default) != value,
        };

    public virtual DateOnly Cast(object obj)
        => TryCast(obj, out var value)
            ? value
            : throw new InvalidCastException($"Cannot cast an object of type '{obj.GetType().FullName}' to type {nameof(DateOnly)}.");

    public virtual bool TryParse(string text, [NotNullWhen(true)] out DateOnly value)
    {
        if (DateOnly.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out value))
            return true;

        if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var dateTime)
            && dateTime.TimeOfDay == TimeSpan.Zero)
        {
            value = DateOnly.FromDateTime(dateTime);
            return true;
        }

        return false;
    }

    public virtual DateOnly Parse(string text)
        => TryParse(text, out var value) ? value : throw new FormatException();
}
