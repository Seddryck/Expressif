using System;
using System.Threading;

namespace Expressif;

internal static class EvaluationRuntime
{
    private static readonly AsyncLocal<State?> CurrentState = new();

    public static EvaluationFrame? Frame => CurrentState.Value?.Frame;
    public static EvaluationContext? Context => CurrentState.Value?.Context;

    public static IDisposable Enter(EvaluationFrame frame, EvaluationContext context)
    {
        var previous = CurrentState.Value;
        CurrentState.Value = new State(frame, context);
        return new Scope(previous);
    }

    public static IDisposable Derive(object? input)
        => Derive(input, input);

    public static IDisposable Derive(object? input, object? currentInput)
    {
        var current = CurrentState.Value;
        if (current is null)
        {
            CurrentState.Value = new State(new EvaluationFrame(currentInput, input), EvaluationContext.Empty);
            return new Scope(null);
        }

        var previous = current;
        CurrentState.Value = new State(new EvaluationFrame(current.Frame.Scope.Derive(input) with { Current = currentInput }, current.Frame), current.Context, current.Bindings);
        return new Scope(previous);
    }

    public static IDisposable BindInput(object? input, IReadOnlyDictionary<string, object?> names)
    {
        var previous = CurrentState.Value;
        var bindings = new Dictionary<string, object?>(StringComparer.Ordinal);
        if (previous?.Bindings is { } inherited)
        {
            foreach (var binding in inherited)
                bindings.Add(binding.Key, binding.Value);
        }
        foreach (var binding in names)
            bindings[binding.Key] = binding.Value;
        CurrentState.Value = new State(
            new EvaluationFrame(input, input, parent: previous?.Frame) { IsInputBound = true },
            previous?.Context ?? EvaluationContext.Empty,
            bindings);
        return new Scope(previous);
    }

    public static bool TryGetBinding(string name, out object? value)
    {
        value = null;
        return CurrentState.Value?.Bindings?.TryGetValue(name, out value) == true;
    }

    public static object? EvaluateNested(Functions.IFunction expression, object? input)
        => EvaluateNested(expression, input, input);

    public static object? EvaluateNested(Functions.IFunction expression, object? input, object? currentInput)
    {
        if (expression is Functions.InputBoundFunction or Predicates.BooleanFunctionPredicate { IsInputBound: true })
            return expression.Evaluate(input);
        using var scope = Derive(input, currentInput);
        return expression.Evaluate(input);
    }

    private sealed record State(
        EvaluationFrame Frame,
        EvaluationContext Context,
        IReadOnlyDictionary<string, object?>? Bindings = null);

    private sealed class Scope(State? previous) : IDisposable
    {
        public void Dispose() => CurrentState.Value = previous;
    }
}
