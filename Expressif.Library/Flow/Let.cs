namespace Expressif.Library.Flow;

/// <summary>Evaluates named bindings once and preserves the pipeline input for subsequent stages.</summary>
[Function(prefix: "", DynamicReason = "Output preserves the pipeline input type; lexical bindings do not transform the input.")]
[Scope("flow")]
public sealed class Let : IFunction
{
    private Func<NamedExpressionEvaluator[]> Bindings { get; }

    /// <param name="bindings">One or more named expressions whose results become lexical values.</param>
    public Let(Func<NamedExpressionEvaluator[]> bindings) => Bindings = bindings;

    public object? Evaluate(object? value)
    {
        var names = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var binding in Bindings.Invoke())
        {
            using var scope = EvaluationRuntime.IsolateBindings();
            names.Add(binding.Name, EvaluationRuntime.CaptureDeferredResult(binding.Evaluator.Invoke(value)));
        }
        EvaluationRuntime.ExtendBindings(names);
        return value;
    }
}
