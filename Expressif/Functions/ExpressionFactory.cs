using Expressif.Parsers;
using Expressif.Functions.Array;
using Expressif.Accumulators;
using Expressif.Accumulators.Introspection;
using Expressif.Predicates;
using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Expressif.Functions;

public class ExpressionFactory : BaseExpressionFactory
{
    private Parser<IRootExpression> Parser { get; } = RootExpression.Parser;
    private static readonly HashSet<string> ImplicitFoldAccumulators = new(
        new AccumulatorIntrospector().Locate().Select(x => x.Name),
        StringComparer.OrdinalIgnoreCase
    );

    public ExpressionFactory()
        : base(new FunctionTypeMapper()) { }

    public IFunction Instantiate(string code, IContext context)
    {
        var rootExpression = Parser.Parse(code);
        return rootExpression switch
        {
            OpenRootExpression open => BuildOpenExpression(open.Expression, context),
            ClosedRootExpression closed => BuildClosedExpression(closed.Expression, context),
            _ => throw new ParseException($"Unsupported expression root '{rootExpression.GetType().Name}'.")
        };
    }

    public IFunction Instantiate(string name, IParameter[] parameters, IContext context)
        => Instantiate<IFunction>(name, parameters, context);

    public IFunction Instantiate(Type type, IParameter[] parameters, IContext context)
        => Instantiate<IFunction>(type, parameters, context);

    private IFunction BuildOpenExpression(OpenExpression expression, IContext context)
    {
        var functions = new List<IFunction>();
        foreach (var member in expression.Members)
            functions.Add(InstantiateOrWrapAggregation(member, context));

        return new ChainFunction(functions);
    }

    private IFunction BuildClosedExpression(Parsers.ClosedExpression expression, IContext context)
    {
        var sourceParameter = CreateParameter(expression.Parameter, typeof(object), context);
        var functions = new List<IFunction>();
        foreach (var member in expression.Members)
            functions.Add(InstantiateOrWrapAggregation(member, context));

        return new DelegatedFunction(_ =>
        {
            var source = sourceParameter.DynamicInvoke();
            return functions.Aggregate(source, (current, function) => function.Evaluate(current));
        });
    }

    private IFunction InstantiateOrWrapAggregation(Parsers.Function function, IContext context)
    {
        var name = function.Name.ToKebabCase();

        if (ImplicitFoldAccumulators.Contains(name) && function.Parameters.Length == 0)
            return new Fold(() => name);

        var type = TypeMapper.Execute(function.Name);

        if (TryInstantiateWithAccumulatorProvider(type, function, context, out var aggregation))
            return aggregation;

        if (TryInstantiateWithTransformationProvider(type, function, context, out var transformation))
            return transformation;

        if (TryInstantiateWithPredicateProvider(type, function, context, out var filtering))
            return filtering;

        return Instantiate<IFunction>(type, function.Parameters, context);
    }

    private bool TryInstantiateWithAccumulatorProvider(Type type, Parsers.Function function, IContext context, out IFunction aggregation)
    {
        aggregation = null!;

        var ctor = type.GetConstructors()
                       .FirstOrDefault(x => x.GetParameters().Length == 1
                                         && x.GetParameters()[0].ParameterType == typeof(Func<IAccumulator>));
        if (ctor is null)
            return false;

        if (function.Parameters.Length != 1)
            throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);

        aggregation = (IFunction)ctor.Invoke([BuildAccumulatorProvider(function.Parameters[0], context)]);
        return true;
    }

    private Func<IAccumulator> BuildAccumulatorProvider(IParameter parameter, IContext context)
    {
        var nameProvider = BuildAccumulatorNameProvider(parameter, context);
        return () => AccumulatorFactory.Instantiate(nameProvider.Invoke());
    }

    private bool TryInstantiateWithTransformationProvider(Type type, Parsers.Function function, IContext context, out IFunction transformation)
    {
        transformation = null!;

        var ctor = type.GetConstructors()
                       .FirstOrDefault(x => x.GetParameters().Length == 1
                                         && x.GetParameters()[0].ParameterType == typeof(Func<IFunction>));
        if (ctor is null)
            return false;

        if (function.Parameters.Length != 1)
            throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);

        if (function.Parameters[0] is not OpenExpressionParameter openExpression)
            throw new ArgumentException(
                $"The function named '{function.Name}' expects a parameter of type '{nameof(OpenExpressionParameter)}' but received '{function.Parameters[0].GetType().Name}'.",
                nameof(function));

        transformation = (IFunction)ctor.Invoke([BuildTransformationProvider(openExpression, context)]);
        return true;
    }

    private Func<IFunction> BuildTransformationProvider(OpenExpressionParameter parameter, IContext context)
        => () => new ChainFunction(parameter.Expression.Members.Select(member => InstantiateOrWrapAggregation(member, context)).ToArray());

    private bool TryInstantiateWithPredicateProvider(Type type, Parsers.Function function, IContext context, out IFunction filtering)
    {
        filtering = null!;

        var ctor = type.GetConstructors()
                       .FirstOrDefault(x => x.GetParameters().Length == 1
                                         && x.GetParameters()[0].ParameterType == typeof(Func<IPredicate>));
        if (ctor is null)
            return false;

        if (function.Parameters.Length != 1)
            throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);

        if (function.Parameters[0] is not PredicationParameter and not OpenExpressionParameter)
            throw new ArgumentException(
                $"The function named '{function.Name}' expects a parameter of type '{nameof(PredicationParameter)}' or '{nameof(OpenExpressionParameter)}' but received '{function.Parameters[0].GetType().Name}'.",
                nameof(function));

        filtering = (IFunction)ctor.Invoke([BuildPredicateProvider(function.Parameters[0], context, function.Name)]);
        return true;
    }

    private Func<IPredicate> BuildPredicateProvider(IParameter parameter, IContext context, string functionName)
    {
        var factory = new PredicationFactory();
        return parameter switch
        {
            PredicationParameter predication => () => factory.Instantiate(predication.Predication, context),
            OpenExpressionParameter openExpression => BuildSinglePredicateFromOpenExpression(openExpression, factory, context, functionName),
            _ => throw new ArgumentException(
                    $"The function named '{functionName}' expects a parameter of type '{nameof(PredicationParameter)}' or '{nameof(OpenExpressionParameter)}' but received '{parameter.GetType().Name}'.",
                    nameof(parameter))
        };

        static Func<IPredicate> BuildSinglePredicateFromOpenExpression(OpenExpressionParameter openExpression, PredicationFactory factory, IContext context, string functionName)
        {
            var members = openExpression.Expression.Members.ToArray();
            if (members.Length != 1)
                throw new MissingOrUnexpectedParametersFunctionException(functionName, members.Length);

            var predication = new SinglePredication(members[0]);
            return () => factory.Instantiate(predication, context);
        }
    }

    private Func<string> BuildAccumulatorNameProvider(IParameter parameter, IContext context)
    {
        var provider = CreateParameter(parameter, typeof(string), context);
        return () => provider.DynamicInvoke()?.ToString() ?? string.Empty;
    }

    private sealed class DelegatedFunction : IFunction
    {
        private Func<object?, object?> Function { get; }

        public DelegatedFunction(Func<object?, object?> function)
            => Function = function;

        public object? Evaluate(object? value)
            => Function.Invoke(value);
    }
}
