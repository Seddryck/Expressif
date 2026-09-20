using System.Collections;
using Expressif.Functions;

namespace Expressif.Functions.Accumulation;

[Scope("array")]
public abstract class BaseAccumulator : IAccumulator
{
    object? IFunction<IEnumerable, object?>.Evaluate(IEnumerable value)
        => Evaluate(value);

    public virtual object? Evaluate(object? value)
        => value is IEnumerable enumerable && value is not string
            ? Evaluate(enumerable)
            : null;

    public object? Evaluate(IEnumerable value)
    {
        Initialize();
        foreach (var item in value)
            Accumulate(item);

        return GetValue();
    }

    public virtual void Initialize()
    { }

    public abstract void Accumulate(object? item);

    public abstract object? GetValue();
}
