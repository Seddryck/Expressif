using System;

namespace Expressif.Functions.Array;

public interface IAccumulator
{
    void Initialize();
    void Accumulate(object? item);
    object? GetValue();
}
