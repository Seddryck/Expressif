using System;
using System.ComponentModel;

namespace Expressif.Values.Resolvers
{
    public class VariableScalarResolver<T> : IScalarResolver<T>
    {
        private string Name { get; }
        private ContextVariables Variables { get; }

        public VariableScalarResolver(string name, ContextVariables variables)
            => (Name, Variables) = (name, variables);

        public T? Execute() => (T?)Variables[Name];
        object? IScalarResolver.Execute() => Execute();
    }
}
