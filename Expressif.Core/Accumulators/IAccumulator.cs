using System;

namespace Expressif.Accumulators;

public interface IAccumulator
{
    void Initialize();
    void Accumulate(object? item);
    object? GetValue();
}
