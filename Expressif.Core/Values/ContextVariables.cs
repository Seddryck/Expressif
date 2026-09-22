using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Expressif.Values;

public sealed class ContextVariables
{
    private IDictionary<string, object?> Variables { get; }
    private IReadOnlyCollection<string> VariableNames { get; }

    public ContextVariables()
        : this(new Dictionary<string, object?>()) { }

    public ContextVariables(IDictionary<string, object?> variables)
        => (Variables, VariableNames) = (variables, new KeyCollection(variables));

    public void Add<T>(string name, object? value)
    {
        name = name.StartsWith('@') ? name[1..] : name;
        if (Variables.ContainsKey(name))
            throw new VariableAlreadyExistingException(name);
        Variables.Add(name, value);
    }

    public void Set(string name, object? value)
    {
        name = name.StartsWith('@') ? name[1..] : name;
        if (Variables.ContainsKey(name))
            Variables[name] = value;
        else
            Variables.Add(name, value);
    }

    public void Remove(string name)
    {
        name = name.StartsWith('@') ? name[1..] : name;
        if (Variables.ContainsKey(name))
            Variables.Remove(name);
    }

    public int Count => Variables.Count;

    public IReadOnlyCollection<string> Keys => VariableNames;

    public object? this[string name]
        => TryGetValue(name, out var value)
            ? value
            : throw new UnexpectedVariableException(name);

    public bool TryGetValue(string name, [NotNullWhen(true)] out object? value)
    {
        var response = Variables.TryGetValue(name.StartsWith('@') ? name[1..] : name, out var result);
        value = response ? Evaluate(result) : null;
        return response;
    }

    private static object? Evaluate(object? value)
    {
        if (value is null)
            return null;
        if (value is Delegate @delegate
                && value.GetType().Name == typeof(Func<>).Name
                && value.GetType().GenericTypeArguments.Length == 1
        )
            return @delegate.DynamicInvoke();
        else
            return value;
    }

    public bool Contains(string name)
        => Variables.ContainsKey(name.StartsWith('@') ? name[1..] : name);

    private sealed class KeyCollection(IDictionary<string, object?> variables) : IReadOnlyCollection<string>
    {
        public int Count => variables.Keys.Count;
        public IEnumerator<string> GetEnumerator() => variables.Keys.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
