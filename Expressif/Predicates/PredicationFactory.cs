using Expressif.Functions;
using Expressif.Bindings;
using Expressif.Predicates.Operators;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates;

public class PredicationFactory : BaseExpressionFactory
{
    private ExpressifBinder Binder { get; } = new();

    protected UnaryOperatorFactory UnaryOperatorFactory { get; }
    protected BinaryOperatorFactory BinaryOperatorFactory { get; }

    protected internal PredicationFactory(PredicateTypeMapper mapper, UnaryOperatorFactory unary, BinaryOperatorFactory binary)
        : base(mapper)
        => (UnaryOperatorFactory, BinaryOperatorFactory) = (unary, binary);

    public PredicationFactory()
        : this(new PredicateTypeMapper(), new UnaryOperatorFactory(), new BinaryOperatorFactory()) { }

    public virtual IPredicate Instantiate(string code, IContext context)
    {
        var predication = Binder.BindPredication(code);
        var predicate = Instantiate(predication, context);
        return predicate;
    }

    public IPredicate Instantiate(IPredication predication, IContext context)
    => predication switch
    {
        SinglePredication single => Instantiate(single, context),
        UnaryPredication unary => Instantiate(unary, context),
        BinaryPredication binary => Instantiate(binary, context),
        _ => throw new BindingException($"Unsupported predication model '{predication.GetType().Name}'.")
    };

    internal IPredicate Instantiate(SinglePredication basic, IContext context)
    {
        var predicates = new List<IPredicate>();
        foreach (var predicate in basic.Members)
            predicates.Add(Instantiate<IPredicate>(predicate.Name, predicate.Parameters, context));
        return predicates.Count == 1
            ? predicates[0]
            : new ContextualPredicate(new ChainFunction(predicates), context);
    }

    protected override Delegate CreateParameter(IParameter parameter, Type scalarType, IContext context)
        => parameter is OpenExpressionParameter open
            ? CreateFunctionCast(
                () => Instantiate(new SinglePredication(open.Expression.Members.ToArray()), context)
                    .Evaluate(context.CurrentObject.Value),
                scalarType)
            : base.CreateParameter(parameter, scalarType, context);

    private sealed class ContextualPredicate(IFunction expression, IContext context) : IPredicate
    {
        public bool Evaluate(object? value)
        {
            var previous = context.CurrentObject.Value;
            context.CurrentObject.Set(value);
            try
            {
                return Boolean.BooleanConversion.ToBoolean(expression.Evaluate(value));
            }
            finally
            {
                context.CurrentObject.Set(previous);
            }
        }

        object? IFunction.Evaluate(object? value) => Evaluate(value);
    }

    internal IPredicate Instantiate(UnaryPredication unary, IContext context)
    {
        var predicate = Instantiate(unary.Member, context);
        return UnaryOperatorFactory.Instantiate(unary.Operator.Name, predicate);
    }

    internal IPredicate Instantiate(BinaryPredication binary, IContext context)
    {
        var left = Instantiate(binary.LeftMember, context);
        var right = Instantiate(binary.RightMember, context);
        return BinaryOperatorFactory.Instantiate(binary.Operator.Name, left, right);
    }
}
