namespace Expressif.Functions.Flow;

/// <summary>Returns a recovery result and terminates the current pipeline when the input is null; otherwise, passes the input through.</summary>
[Function(prefix: "", DynamicReason = "Output preserves the input type or depends on the recovery expression when the input is null.")]
[Scope("flow")]
public sealed class Catch : IPipelineControlFunction
{
    private readonly Func<IFunction> expression;

    /// <param name="expression">Recovery expression whose result becomes the final result of the current pipeline.</param>
    public Catch(Func<IFunction> expression) => this.expression = expression;

    public object? Evaluate(object? value) => Evaluate(value, out _);

    object? IPipelineControlFunction.Evaluate(object? value, out bool terminate) => Evaluate(value, out terminate);

    private object? Evaluate(object? value, out bool terminate)
    {
        terminate = new Predicates.Special.Null().Evaluate(value);
        return terminate
            ? expression.Invoke().Evaluate(EvaluationRuntime.Frame is { } frame ? frame.Current : value)
            : value;
    }
}
