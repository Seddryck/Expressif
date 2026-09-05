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
    {
        var current = CurrentState.Value;
        if (current is null)
        {
            CurrentState.Value = new State(new EvaluationFrame(input, input), EvaluationContext.Empty);
            return new Scope(null);
        }

        var previous = current;
        CurrentState.Value = new State(new EvaluationFrame(input, input, parent: current.Frame), current.Context);
        return new Scope(previous);
    }

    private sealed record State(EvaluationFrame Frame, EvaluationContext Context);

    private sealed class Scope(State? previous) : IDisposable
    {
        public void Dispose() => CurrentState.Value = previous;
    }
}
