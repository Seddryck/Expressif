using Expressif.Bindings;

namespace Expressif.Functions;

internal sealed class InputBoundFunction(InputBoundExpression binding, Func<object?, object?> body) : IFunction
{
    public object? Evaluate(object? value)
    {
        var names = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var name in binding.Names)
            names.Add(name, value);
        using var scope = EvaluationRuntime.BindInput(value, names);
        return body(value);
    }
}
