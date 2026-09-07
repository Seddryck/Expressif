using Expressif.Bindings;

namespace Expressif.Functions;

internal sealed class InputBoundFunction(InputBoundExpression binding, Func<object?, object?> body) : IFunction
{
    public object? Evaluate(object? value)
    {
        var names = new Dictionary<string, object?>(StringComparer.Ordinal);
        if (binding.IsPositional)
        {
            if (value is not Values.IPositionalValue positional)
            {
                throw new ArgumentException("Positional input binding requires a tuple, pair, group, or vector; received "
                    + (value?.GetType().Name ?? "null") + ".");
            }
            if (positional.Arity != binding.Names.Count)
                throw new ArgumentException($"Positional input binding expects {binding.Names.Count} components but received {positional.Arity}.");
            for (var index = 0; index < binding.Names.Count; index++)
                names.Add(binding.Names[index], positional.GetPosition(index));
        }
        else
        {
            foreach (var name in binding.Names)
                names.Add(name, value);
        }
        using var scope = EvaluationRuntime.BindInput(value, names);
        return EvaluationRuntime.CaptureDeferredResult(body(value));
    }
}
