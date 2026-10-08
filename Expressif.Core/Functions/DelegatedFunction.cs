namespace Expressif.Functions;

/// <summary>Adapts an evaluation delegate to an Expressif function.</summary>
internal sealed class DelegatedFunction(Func<object?, object?> function) : IFunction
{
    public object? Evaluate(object? value) => function.Invoke(value);
}
