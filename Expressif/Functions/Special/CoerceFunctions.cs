using Expressif.Values.Casters;
using Expressif.Values;
using System.Numerics;

namespace Expressif.Functions.Special;

[Function]
public abstract class BaseCoerceValueFunction<T> : IFunction<object?, T?>
    where T : struct
{
    private Caster Caster { get; } = new();

    public virtual T? Evaluate(object? value)
        => EvaluateTyped(value);

    protected T? EvaluateTyped<TIn>(TIn value)
        => Caster.TryCast<T>(value, out var result) ? result : null;

    object? IFunction.Evaluate(object? value) => Evaluate(value);
}

/// <summary>
/// Attempts to convert the input to a numeric value. Returns `null` when the input cannot be converted.
/// </summary>
[Function]
public sealed class CoerceNumeric : BaseCoerceValueFunction<decimal>,
    IFunction<bool, decimal?>,
    IFunction<string, decimal?>
{
    public override decimal? Evaluate(object? value)
        => NumericCoercion.TryToDecimal(value, out var result) ? result : null;

    decimal? IFunction<bool, decimal?>.Evaluate(bool value) => EvaluateTyped(value);
    decimal? IFunction<string, decimal?>.Evaluate(string value) => EvaluateTyped(value);
}

public sealed class CoerceNumeric<T> : Function<T, decimal?>
    where T : INumber<T>
{
    public override decimal? Evaluate(T value)
        => NumericCoercion.TryToDecimal(value, out var result) ? result : null;
}

/// <summary>
/// Attempts to convert the input to an integer value. Returns `null` when the input cannot be converted without loss.
/// </summary>
[Function]
public sealed class CoerceInt : BaseCoerceValueFunction<int>,
    IFunction<bool, int?>,
    IFunction<string, int?>
{
    public override int? Evaluate(object? value)
        => NumericCoercion.TryToInt(value, out var result) ? result : null;

    int? IFunction<bool, int?>.Evaluate(bool value) => EvaluateTyped(value);
    int? IFunction<string, int?>.Evaluate(string value) => EvaluateTyped(value);
}

public sealed class CoerceInt<T> : Function<T, int?>
    where T : INumber<T>
{
    public override int? Evaluate(T value)
        => NumericCoercion.TryToInt(value, out var result) ? result : null;
}

/// <summary>
/// Attempts to convert the input to a text value. Returns `null` when the input cannot be converted.
/// </summary>
[Function]
public sealed class CoerceText : IFunction<object?, string?>,
    IFunction<string, string?>,
    IFunction<bool, string?>,
    IFunction<DateOnly, string?>,
    IFunction<DateTime, string?>,
    IFunction<YearMonth, string?>
{
    private Caster Caster { get; } = new();

    public string? Evaluate(object? value)
        => NumericCoercion.TryToText(value, out var result) ? result : null;

    private string? EvaluateTyped<TIn>(TIn value)
        => Caster.TryCast<string>(value, out var result) ? result : null;

    object? IFunction.Evaluate(object? value) => Evaluate(value);
    string? IFunction<string, string?>.Evaluate(string value) => value;
    string? IFunction<bool, string?>.Evaluate(bool value) => EvaluateTyped(value);
    string? IFunction<DateOnly, string?>.Evaluate(DateOnly value) => EvaluateTyped(value);
    string? IFunction<DateTime, string?>.Evaluate(DateTime value) => EvaluateTyped(value);
    string? IFunction<YearMonth, string?>.Evaluate(YearMonth value) => EvaluateTyped(value);
}

public sealed class CoerceText<T> : Function<T, string?>
    where T : INumber<T>
{
    public override string? Evaluate(T value)
        => NumericCoercion.TryToText(value, out var result) ? result : null;
}

/// <summary>
/// Attempts to convert the input to a boolean value. Returns `null` when the input cannot be converted.
/// </summary>
[Function]
public sealed class CoerceBoolean : BaseCoerceValueFunction<bool>,
    IFunction<bool, bool?>,
    IFunction<string, bool?>
{
    public override bool? Evaluate(object? value)
        => NumericCoercion.TryToBoolean(value, out var result) ? result : null;

    bool? IFunction<bool, bool?>.Evaluate(bool value) => value;
    bool? IFunction<string, bool?>.Evaluate(string value) => EvaluateTyped(value);
}

public sealed class CoerceBoolean<T> : Function<T, bool?>
    where T : INumber<T>
{
    public override bool? Evaluate(T value)
        => NumericCoercion.TryToBoolean(value, out var result) ? result : null;
}

/// <summary>
/// Attempts to convert the input to a date value. Returns `null` when the input cannot be converted.
/// </summary>
[Function]
public sealed class CoerceDate : BaseCoerceValueFunction<DateOnly>,
    IFunction<DateOnly, DateOnly?>,
    IFunction<DateTime, DateOnly?>,
    IFunction<YearMonth, DateOnly?>,
    IFunction<string, DateOnly?>
{
    DateOnly? IFunction<DateOnly, DateOnly?>.Evaluate(DateOnly value) => value;
    DateOnly? IFunction<DateTime, DateOnly?>.Evaluate(DateTime value) => EvaluateTyped(value);
    DateOnly? IFunction<YearMonth, DateOnly?>.Evaluate(YearMonth value) => EvaluateTyped(value);
    DateOnly? IFunction<string, DateOnly?>.Evaluate(string value) => EvaluateTyped(value);
}

/// <summary>
/// Attempts to convert the input to a time value. Returns `null` when the input cannot be converted.
/// </summary>
[Function]
public sealed class CoerceTime : BaseCoerceValueFunction<TimeOnly>,
    IFunction<TimeOnly, TimeOnly?>,
    IFunction<DateTime, TimeOnly?>,
    IFunction<string, TimeOnly?>
{
    TimeOnly? IFunction<TimeOnly, TimeOnly?>.Evaluate(TimeOnly value) => value;
    TimeOnly? IFunction<DateTime, TimeOnly?>.Evaluate(DateTime value) => EvaluateTyped(value);
    TimeOnly? IFunction<string, TimeOnly?>.Evaluate(string value) => EvaluateTyped(value);
}

/// <summary>
/// Attempts to convert the input to a date-time value. Returns `null` when the input cannot be converted.
/// </summary>
[Function]
public sealed class CoerceDateTime : BaseCoerceValueFunction<DateTime>,
    IFunction<DateTime, DateTime?>,
    IFunction<DateOnly, DateTime?>,
    IFunction<YearMonth, DateTime?>,
    IFunction<string, DateTime?>
{
    DateTime? IFunction<DateTime, DateTime?>.Evaluate(DateTime value) => value;
    DateTime? IFunction<DateOnly, DateTime?>.Evaluate(DateOnly value) => EvaluateTyped(value);
    DateTime? IFunction<YearMonth, DateTime?>.Evaluate(YearMonth value) => EvaluateTyped(value);
    DateTime? IFunction<string, DateTime?>.Evaluate(string value) => EvaluateTyped(value);
}
