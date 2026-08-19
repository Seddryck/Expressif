using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Expressif.Values.Casters;

public class DateOnlyCaster : ICaster<DateOnly>, IParser<DateOnly>
{
    public virtual bool TryCast(object obj, [NotNullWhen(true)] out DateOnly value)
        => obj switch
        {
            DateOnly date => (value = date) == value,
            DateTime dateTime when dateTime.TimeOfDay == TimeSpan.Zero
                => (value = DateOnly.FromDateTime(dateTime)) == value,
            string text => TryParse(text, out value),
            _ => (value = default) != value,
        };

    public virtual DateOnly Cast(object obj)
        => TryCast(obj, out var value)
            ? value
            : throw new InvalidCastException($"Cannot cast an object of type '{obj.GetType().FullName}' to type {nameof(DateOnly)} without loss.");

    public virtual bool TryParse(string text, [NotNullWhen(true)] out DateOnly value)
    {
        if (DateOnly.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out value))
            return true;

        return DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var dateTime)
            && dateTime.TimeOfDay == TimeSpan.Zero
            && (value = DateOnly.FromDateTime(dateTime)) == value;
    }

    public virtual DateOnly Parse(string text)
        => TryParse(text, out var value) ? value : throw new FormatException();
}
