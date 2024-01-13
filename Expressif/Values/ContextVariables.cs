using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace Expressif.Values;

public class ContextVariables
{
    private IDictionary<string, object?> Variables { get; }

    public ContextVariables()
        : this(new Dictionary<string, object?>()) { }

    public ContextVariables(IDictionary<string, object?> variables)
        => Variables = variables;

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

    public ICollection<string> Keys => Variables.Keys;

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

    protected virtual object? Evaluate(object? value)
    {
        if (value is null)
            return null;
        if (value.GetType() == typeof(Func<>))
            return ((Func<object>) value).Invoke();
        else
            return value;
    }

    public bool Contains(string name)
        => Variables.ContainsKey(name.StartsWith('@') ? name[1..] : name);
}
