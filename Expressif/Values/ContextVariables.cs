using Expressif.Values.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Values;

public class ContextVariables
{
    private IDictionary<string, Func<object?>> Variables { get; } = new Dictionary<string, Func<object?>>();

    public void Add(string name, Func<object?> value)
    {
        name = name.StartsWith('@') ? name[1..] : name;
        if (Variables.ContainsKey(name))
            throw new VariableAlreadyExistingException(name);
        Variables.Add(name, value);
    }

    public void Add<T>(string name, object value)
    {
        var resolver = new LiteralScalarResolver<T>(value);
        Add(name, () => resolver.Execute());
    }

    public void Set(string name, Func<object?> value)
    {
        name = name.StartsWith('@') ? name[1..] : name;
        if (Variables.ContainsKey(name))
            Variables[name] = value;
        else
            Variables.Add(name, value);
    }

    public void Set<T>(string name, object value)
    {
        var resolver = new LiteralScalarResolver<T>(value);
        Set(name, () => resolver.Execute());
    }

    public void Remove(string name)
    {
        name = name.StartsWith('@') ? name[1..] : name;
        if (Variables.ContainsKey(name))
            Variables.Remove(name);
    }

    public int Count { get => Variables.Count; }

    public object? this[string name]
    {
        get
        {
            name = name.StartsWith('@') ? name[1..] : name;
            if (Variables.TryGetValue(name, out var value))
                return value.Invoke();
            throw new UnexpectedVariableException(name);
        }
    }
}
