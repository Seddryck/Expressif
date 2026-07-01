using System;

namespace Expressif.Accumulators;

public abstract class BaseAccumulator : IAccumulator
{
    public virtual void Initialize()
    { }

    public abstract void Accumulate(object? item);

    public abstract object? GetValue();
}
