using System.Collections.ObjectModel;

namespace Expressif;

/// <summary>Provides immutable host values and once-per-evaluation providers.</summary>
public sealed class EvaluationContext
{
    private readonly IReadOnlyDictionary<string, IEvaluationRegistration> registrations;

    public static EvaluationContext Empty { get; } = new(
        new Dictionary<string, IEvaluationRegistration>(StringComparer.OrdinalIgnoreCase));

    private EvaluationContext(IDictionary<string, IEvaluationRegistration> registrations)
        => this.registrations = new ReadOnlyDictionary<string, IEvaluationRegistration>(
            new Dictionary<string, IEvaluationRegistration>(registrations, StringComparer.OrdinalIgnoreCase));

    /// <summary>Creates a mutable builder for an immutable evaluation configuration.</summary>
    public static EvaluationContextBuilder CreateBuilder() => new();

    internal IReadOnlyDictionary<string, object?> Materialize(object? input)
    {
        var start = new EvaluationStartContext(input);
        return new ReadOnlyDictionary<string, object?>(registrations.ToDictionary(
            registration => registration.Key,
            registration => registration.Value.Resolve(start),
            StringComparer.OrdinalIgnoreCase));
    }

    private interface IEvaluationRegistration
    {
        object? Resolve(EvaluationStartContext context);
    }

    private sealed record ValueRegistration<T>(T? Value) : IEvaluationRegistration
    {
        public object? Resolve(EvaluationStartContext context) => Value;
    }

    private sealed record ProviderRegistration<T>(Func<EvaluationStartContext, T?> Provider) : IEvaluationRegistration
    {
        public object? Resolve(EvaluationStartContext context) => Provider(context);
    }

    /// <summary>Builds an immutable snapshot of registered host values and providers.</summary>
    public sealed class EvaluationContextBuilder
    {
        private readonly Dictionary<string, IEvaluationRegistration> registrations =
            new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Adds a value available to every evaluation.</summary>
        public EvaluationContextBuilder AddValue<T>(string name, T? value)
        {
            Add(name, new ValueRegistration<T>(value));
            return this;
        }

        /// <summary>Adds a provider evaluated exactly once at the start of each top-level evaluation.</summary>
        public EvaluationContextBuilder AddProvider<T>(string name, Func<EvaluationStartContext, T?> provider)
        {
            ArgumentNullException.ThrowIfNull(provider);
            Add(name, new ProviderRegistration<T>(provider));
            return this;
        }

        /// <summary>Creates an immutable registration snapshot.</summary>
        public EvaluationContext Build() => new(registrations);

        private void Add(string name, IEvaluationRegistration registration)
        {
            var normalized = NormalizeName(name);
            if (!registrations.TryAdd(normalized, registration))
                throw new ArgumentException($"An evaluation value named '{normalized}' is already registered.", nameof(name));
        }
    }

    internal static string NormalizeName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var normalized = name[0] == '@' ? name[1..] : name;
        if (string.IsNullOrWhiteSpace(normalized))
            throw new ArgumentException("Evaluation value names cannot be empty.", nameof(name));
        return normalized;
    }
}

/// <summary>Describes the input supplied to one top-level evaluation.</summary>
public readonly record struct EvaluationStartContext(object? Input);

/// <summary>Provides a read-only view of the current argument-evaluation scope.</summary>
public readonly struct ArgumentEvaluationContext
{
    internal ArgumentEvaluationContext(object? current, object? root, object? enclosingRoot)
        => (Current, Root, EnclosingRoot) = (current, root, enclosingRoot);

    public object? Current { get; }
    public object? Root { get; }
    public object? EnclosingRoot { get; }

    public T? GetVariable<T>(string name)
        => TryGetVariable<T>(name, out var value)
            ? value
            : throw new KeyNotFoundException($"Evaluation value '{EvaluationContext.NormalizeName(name)}' was not found.");

    public bool TryGetVariable<T>(string name, out T? value)
    {
        if (EvaluationRuntime.TryGetVariable(name, out var raw) && raw is null)
        {
            value = default;
            return true;
        }
        if (raw is T typed)
        {
            value = typed;
            return true;
        }
        value = default;
        return false;
    }

    internal static ArgumentEvaluationContext CurrentScope
    {
        get
        {
            var frame = EvaluationRuntime.Frame;
            return new(
                EvaluationRuntime.ArgumentInput ?? frame?.Current,
                frame?.Scope.Resolve(Semantics.FieldReferenceKind.ExpressionRoot, null, null),
                frame?.Scope.Resolve(Semantics.FieldReferenceKind.EnclosingExpressionRoot, null, null));
        }
    }
}

/// <summary>Creates typed deferred arguments for expression and predication builders.</summary>
public static class Argument
{
    public static Bindings.IParameter From<T>(Func<ArgumentEvaluationContext, T?> provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        return new Bindings.ArgumentProviderParameter(scope => provider(scope));
    }
}
