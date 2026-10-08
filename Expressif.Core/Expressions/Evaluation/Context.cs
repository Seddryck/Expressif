using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Values;

namespace Expressif;

internal sealed class Context : IContext
{
    public Context()
        : this([]) { }

    public Context(Dictionary<string, object?> variables)
        : this(new ContextVariables(variables), new ContextObject()) { }

    private Context(ContextVariables variables, ContextObject currentObject)
        => (Variables, CurrentObject) = (variables, currentObject);

    public ContextVariables Variables { get; } = new();
    public ContextObject CurrentObject { get; } = new();
}

internal interface IContext
{
    ContextVariables Variables { get; }
    ContextObject CurrentObject { get; }
}
