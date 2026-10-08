namespace Expressif.Functions;

internal sealed class LexicallyBoundContextFunction(IFunction expression) : IFunction
{
    public object? Evaluate(object? value)
        => EvaluationRuntime.EvaluateNested(expression, value);
}
