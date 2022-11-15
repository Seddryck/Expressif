using Expressif.Values.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Values
{
    public class ContextVariables
    {
        private IDictionary<string, IScalarResolver> Variables { get; } = new Dictionary<string, IScalarResolver>();

        public void Add(string name, IScalarResolver value)
        {
            name = name.StartsWith("@") ? name[1..] : name;
            if (Variables.ContainsKey(name))
                throw new VariableAlreadyExistingException(name);
            Variables.Add(name, value);
        }

        public void Add<T>(string name, object value)
            => Add(name, new LiteralScalarResolver<T>(value));

        public void Set(string name, IScalarResolver value)
        {
            name = name.StartsWith("@") ? name[1..] : name;
            if (Variables.ContainsKey(name))
                Variables[name] = value;
            else
                Variables.Add(name, value);
        }

        public void Set<T>(string name, object value)
            => Set(name, new LiteralScalarResolver<T>(value));

        public void Remove(string name)
        {
            name = name.StartsWith("@") ? name[1..] : name;
            if (Variables.ContainsKey(name))
                Variables.Remove(name);
        }

        public int Count {get => Variables.Count;}

        public object? this[string name]
        {
            get
            {
                name = name.StartsWith("@") ? name[1..] : name;
                if (Variables.ContainsKey(name))
                    return Variables[name].Execute();
                throw new UnexpectedVariableException(name);
            }
        }
    }
}
