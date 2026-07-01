using System;

namespace Expressif.Functions.Array;

public abstract class BaseAccumulator : IAccumulator
{
    public virtual void Initialize()
    { }

    public abstract void Accumulate(object? item);

    public abstract object? GetValue();
}
