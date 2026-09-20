namespace Expressif.Functions;

public sealed class LexicallyBoundContextFunction(IFunction expression) : IFunction
{
    public object? Evaluate(object? value)
        => EvaluationRuntime.EvaluateNested(expression, value);
}
