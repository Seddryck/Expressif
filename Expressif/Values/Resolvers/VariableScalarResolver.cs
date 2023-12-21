using Expressif.Values.Casters;
using System;
using System.ComponentModel;

namespace Expressif.Values.Resolvers;

internal class VariableScalarResolver<T> : IScalarResolver<T>
{
    private string Name { get; }
    private ContextVariables Variables { get; }

    public VariableScalarResolver(string name, ContextVariables variables)
        => (Name, Variables) = (name, variables);

    public T? Execute() => new Caster().Cast<T>(Variables[Name]);
    object? IScalarResolver.Execute() => Execute();
}
