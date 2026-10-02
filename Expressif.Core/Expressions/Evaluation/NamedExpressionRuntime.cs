using System.Threading;

namespace Expressif;

internal delegate object? NamedExpressionInvoker(object? input, IReadOnlyList<object?> arguments);

internal static class NamedExpressionRuntime
{
    private static readonly AsyncLocal<IReadOnlyDictionary<string, NamedExpressionInvoker>?> Current = new();

    public static IDisposable Enter(IReadOnlyDictionary<string, NamedExpressionInvoker> definitions)
    {
        var previous = Current.Value;
        Current.Value = definitions;
        return new Scope(previous);
    }

    public static object? Invoke(string name, object? input, IReadOnlyList<object?> arguments)
    {
        var definitions = Current.Value;
        if (definitions is null || !definitions.TryGetValue(name, out var definition))
            throw new InvalidOperationException($"Named expression '{name}' is not available in the current document.");
        return definition(input, arguments);
    }

    private sealed class Scope(IReadOnlyDictionary<string, NamedExpressionInvoker>? previous) : IDisposable
    {
        public void Dispose() => Current.Value = previous;
    }
}
