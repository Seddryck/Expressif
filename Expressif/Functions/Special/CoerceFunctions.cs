using Expressif.Values.Casters;

namespace Expressif.Functions.Special;

[Function]
public abstract class BaseCoerceValueFunction<T> : IFunction<object?, T?>
    where T : struct
{
    private Caster Caster { get; } = new();

    public T? Evaluate(object? value)
        => Caster.TryCast<T>(value, out var result) ? result : null;

    object? IFunction.Evaluate(object? value) => Evaluate(value);
}

/// <summary>
/// Attempts to convert the input to a numeric value. Returns `null` when the input cannot be converted without loss.
/// </summary>
[Function]
public sealed class CoerceNumeric : BaseCoerceValueFunction<decimal>
{ }

/// <summary>
/// Attempts to convert the input to an integer value. Returns `null` when the input cannot be converted without loss.
/// </summary>
[Function]
public sealed class CoerceInt : BaseCoerceValueFunction<int>
{ }

/// <summary>
/// Attempts to convert the input to a text value. Returns `null` when the input cannot be converted.
/// </summary>
[Function]
public sealed class CoerceText : IFunction<object?, string?>
{
    private Caster Caster { get; } = new();

    public string? Evaluate(object? value)
        => Caster.TryCast<string>(value, out var result) ? result : null;

    object? IFunction.Evaluate(object? value) => Evaluate(value);
}

/// <summary>
/// Attempts to convert the input to a boolean value. Returns `null` when the input cannot be converted.
/// </summary>
[Function]
public sealed class CoerceBoolean : BaseCoerceValueFunction<bool>
{ }

/// <summary>
/// Attempts to convert the input to a date value. Returns `null` when the input cannot be converted without loss.
/// </summary>
[Function]
public sealed class CoerceDate : BaseCoerceValueFunction<DateOnly>
{ }

/// <summary>
/// Attempts to convert the input to a time value. Returns `null` when the input cannot be converted without loss.
/// </summary>
[Function]
public sealed class CoerceTime : BaseCoerceValueFunction<TimeOnly>
{ }

/// <summary>
/// Attempts to convert the input to a date-time value. Returns `null` when the input cannot be converted.
/// </summary>
[Function]
public sealed class CoerceDateTime : BaseCoerceValueFunction<DateTime>
{ }
