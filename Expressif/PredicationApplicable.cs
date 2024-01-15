using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions;
using Expressif.Predicates;

namespace Expressif;
public class PredicationApplicable : IPredicationApplicable
{
    private IParameter Argument { get; }
    private IPredicate Predication { get; }
    private IContext Context { get; }

    public PredicationApplicable(string code, IContext? context = null, PredicationApplicableFactory? factory = null)
    {
        context ??= new Context();
        var applicable = (factory ?? new PredicationApplicableFactory()).Instantiate(code, context);
        (Argument, Predication, Context) = (applicable.Argument, applicable.Predication, context);
    }

    internal PredicationApplicable(IParameter argument, IPredicate predication, IContext context)
        => (Argument, Predication, Context) = (argument, predication, context);

    public bool Evaluate()
        => Predication.Evaluate(GetArgument(Argument, Context));

    object? IApplicable.Evaluate() => Evaluate();

    protected virtual object? GetArgument(IParameter parameter, IContext context)
        => parameter switch
        {
            LiteralParameter literal => literal.Value,
            ObjectIndexParameter index => context.CurrentObject[index.Index],
            ObjectPropertyParameter prop => context.CurrentObject[prop.Name],
            VariableParameter variable => context.Variables[variable.Name],
            ContextParameter contextReference => contextReference.Function.Invoke(context),
            _ => throw new NotImplementedException($"Cannot handle the parameter type '{parameter.GetType().Name}'")
        };
}
