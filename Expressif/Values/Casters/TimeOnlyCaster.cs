using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Expressif.Values.Casters;

public class TimeOnlyCaster : ICaster<TimeOnly>, IParser<TimeOnly>
{
    public virtual bool TryCast(object obj, [NotNullWhen(true)] out TimeOnly value)
        => obj switch
        {
            TimeOnly time => (value = time) == value,
            DateTime dateTime => (value = TimeOnly.FromDateTime(dateTime)) == value,
            string text => TryParse(text, out value),
            _ => (value = default) != value,
        };

    public virtual TimeOnly Cast(object obj)
        => TryCast(obj, out var value)
            ? value
            : throw new InvalidCastException($"Cannot cast an object of type '{obj.GetType().FullName}' to type {nameof(TimeOnly)}.");

    public virtual bool TryParse(string text, [NotNullWhen(true)] out TimeOnly value)
        => TimeOnly.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out value);

    public virtual TimeOnly Parse(string text)
        => TryParse(text, out var value) ? value : throw new FormatException();
}
