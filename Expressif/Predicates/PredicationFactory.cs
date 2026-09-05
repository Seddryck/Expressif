using Expressif.Functions;
using Expressif.Bindings;
using Expressif.Predicates.Operators;
using Expressif.Syntax;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates;

public class PredicationFactory : BaseExpressionFactory
{
    private static readonly IReadOnlyDictionary<Type, Func<Func<int>, IEnumerable<Func<bool>>, IPredicate>> CardinalityFactories =
        new Dictionary<Type, Func<Func<int>, IEnumerable<Func<bool>>, IPredicate>>
        {
            [typeof(Boolean.SatisfiesExactly)] = (count, predicates) => new Boolean.SatisfiesExactly(count, predicates),
            [typeof(Boolean.SatisfiesAtLeast)] = (count, predicates) => new Boolean.SatisfiesAtLeast(count, predicates),
            [typeof(Boolean.SatisfiesAtMost)] = (count, predicates) => new Boolean.SatisfiesAtMost(count, predicates),
        };

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
        var predication = Binder.BindPredication(ExpressionParser.Parse(code));
        var predicate = Instantiate(predication, context);
        return predicate;
    }

    public IPredicate Instantiate(IPredication predication, IContext context)
    => predication switch
    {
        SinglePredication single => Instantiate(single, context),
        PipelinePredication pipeline => Instantiate(pipeline, context),
        UnaryPredication unary => Instantiate(unary, context),
        BinaryPredication binary => Instantiate(binary, context),
        _ => throw new BindingException($"Unsupported predication model '{predication.GetType().Name}'.")
    };

    internal IPredicate Instantiate(SinglePredication basic, IContext context)
    {
        var type = TypeMapper.Execute(basic.Member.Name);
        if (type == typeof(Boolean.Majority))
            return InstantiateMajority(basic.Member, context);
        if (CardinalityFactories.TryGetValue(type, out var factory))
            return InstantiateCardinality(basic.Member, context, factory);
        return Instantiate<IPredicate>(type, basic.Member.Arguments, context);
    }

    private IPredicate InstantiateMajority(Bindings.Function function, IContext context)
    {
        EnsureOnlyPositionalArguments(function);
        return new Boolean.Majority(CreateBooleanEvaluators(function.Parameters, context));
    }

    private IPredicate InstantiateCardinality(
        Bindings.Function function,
        IContext context,
        Func<Func<int>, IEnumerable<Func<bool>>, IPredicate> factory)
    {
        EnsureOnlyPositionalArguments(function);
        if (function.Parameters.Length == 0)
            throw new MissingOrUnexpectedParametersFunctionException(function.Name, 0);

        var count = (Func<int>)CreateParameter(function.Parameters[0], typeof(int), context);
        return factory(count, CreateBooleanEvaluators(function.Parameters.Skip(1), context));
    }

    private IEnumerable<Func<bool>> CreateBooleanEvaluators(IEnumerable<IParameter> parameters, IContext context)
        => parameters.Select(parameter => (Func<bool>)CreateParameter(parameter, typeof(bool), context));

    private static void EnsureOnlyPositionalArguments(Bindings.Function function)
    {
        var named = function.Arguments.FirstOrDefault(argument => argument.Name is not null);
        if (named is not null)
            throw new UnknownParameterNameException(function.Name, named.Name!);
    }

    internal IPredicate Instantiate(PipelinePredication pipeline, IContext context)
        => new BooleanFunctionPredicate(new FunctionFactory().Instantiate(pipeline.Expression, context));

    protected override Delegate CreateParameter(IParameter parameter, Type scalarType, IContext context)
        => parameter is OpenExpressionParameter open
            ? CreateFunctionCast(
                () => new BooleanFunctionPredicate(new FunctionFactory().Instantiate(open.Expression, context))
                    .Evaluate(EvaluationRuntime.Frame?.Current ?? context.CurrentObject.Value),
                scalarType)
            : base.CreateParameter(parameter, scalarType, context);

    protected override Delegate CreateInputExpression(InputExpressionParameter input, Type type, IContext context)
    {
        var expression = new FunctionFactory().Instantiate(new ClosedRootExpression(input.Expression), context);
        return CreateFunctionCast(() => expression.Evaluate(null), type);
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
