using System.Collections;
using Expressif.Functions;

namespace Expressif.Functions.Accumulation;

public interface IAccumulator : IFunction<IEnumerable, object?>
{
    void Initialize();
    void Accumulate(object? item);
    object? GetValue();
}
