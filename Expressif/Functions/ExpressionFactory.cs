using Expressif.Parsers;
using Expressif.Functions.Array;
using Expressif.Accumulators;
using Expressif.Accumulators.Introspection;
using Expressif.Predicates;
using Expressif.Values;
using RecordEntryEvaluator = Expressif.Functions.Record.RecordEntryEvaluator;
using RecordFunction = Expressif.Functions.Record.Record;
using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Expressif.Functions;

public class ExpressionFactory : BaseExpressionFactory
{
    private Parser<IRootExpression> Parser { get; } = RootExpression.Parser;
    private static readonly PredicateTypeMapper PredicateTypeMapper = new();
    private static readonly HashSet<string> ImplicitFoldAccumulators = new(
        new AccumulatorIntrospector().Locate().Select(x => x.Name),
        StringComparer.OrdinalIgnoreCase
    );

    public ExpressionFactory()
        : base(new FunctionTypeMapper()) { }

    public IFunction Instantiate(string code, IContext context)
    {
        var rootExpression = ParseRootExpression(code);
        return rootExpression switch
        {
            OpenRootExpression open => BuildOpenExpression(open.Expression, context),
            ClosedRootExpression closed => BuildClosedExpression(closed.Expression, context),
            _ => throw new ParseException($"Unsupported expression root '{rootExpression.GetType().Name}'.")
        };
    }

    public IFunction InstantiateClosed(string code, IContext context)
    {
        var rootExpression = ParseRootExpression(code);
        return rootExpression switch
        {
            ClosedRootExpression closed => BuildClosedExpression(closed.Expression, context),
            OpenRootExpression open => throw new ExpressionRequiresInputException(open.Expression.Members.FirstOrDefault()?.Name),
            _ => throw new ParseException($"Unsupported expression root '{rootExpression.GetType().Name}'.")
        };
    }

    private IRootExpression ParseRootExpression(string code)
        => Parser.Parse(code);

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

        if (name.Equals("record", StringComparison.OrdinalIgnoreCase))
            return BuildRecordFunction(function, context);

        if (name.Equals("coalesce", StringComparison.OrdinalIgnoreCase))
            return BuildCoalesceFunction(function, context);
        if (name.Equals("adjacent", StringComparison.OrdinalIgnoreCase))
            return BuildAdjacentFunction(function, context);

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

    private IFunction BuildCoalesceFunction(Parsers.Function function, IContext context)
    {
        if (function.Parameters.Length < 2)
            throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);

        var expressions = function.Parameters.Select(parameter => BuildCoalesceExpressionEvaluator(parameter, context));
        return new Special.Coalesce(expressions);
    }

    private Func<object?, object?> BuildCoalesceExpressionEvaluator(IParameter parameter, IContext context)
    {
        if (parameter is IncomingValueParameter)
            return input => input;

        if (parameter is OpenExpressionParameter open)
            return BuildOpenExpressionRecordEvaluator(open, context);

        var provider = CreateParameter(parameter, typeof(object), context);
        return _ => provider.DynamicInvoke();
    }
    
    private IFunction BuildAdjacentFunction(Parsers.Function function, IContext context)
    {
        if (function.Parameters.Length != 1 || function.Parameters[0] is not OpenExpressionParameter open)
            throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);

        var members = open.Expression.Members.ToArray();
        IFunction operation;
        if (members.Length == 1 && members[0].Parameters.Length == 0 && TryBuildBinaryCallable(members[0].Name, context, out var callable))
        {
            operation = new ChainFunction([
                InstantiateOrWrapAggregation(new Parsers.Function("tuple-at", [new LiteralParameter("1")]), context),
                callable
            ]);
        }
        else
            operation = BuildOpenExpression(open.Expression, context);

        return new Adjacent(() => new LexicallyBoundTupleFunction(operation, context));
    }

    private bool TryBuildBinaryCallable(string name, IContext context, out IFunction callable)
    {
        var parameterized = new Parsers.Function(name, [new TupleProjectionParameter(0)]);
        try
        {
            var functionType = TypeMapper.Execute(name);
            if (!functionType.GetConstructors().Any(x => x.GetParameters().Length == 1))
            {
                callable = null!;
                return false;
            }
            callable = InstantiateOrWrapAggregation(parameterized, context);
            return true;
        }
        catch (NotImplementedFunctionException)
        {
            try
            {
                var predicateType = new PredicateTypeMapper().Execute(name);
                if (!predicateType.GetConstructors().Any(x => x.GetParameters().Length == 1))
                {
                    callable = null!;
                    return false;
                }
                callable = new PredicationFactory().Instantiate(new SinglePredication(parameterized), context);
                return true;
            }
            catch (NotImplementedFunctionException)
            {
                callable = null!;
                return false;
            }
        }
    }

    private sealed class LexicallyBoundTupleFunction(IFunction expression, IContext context) : IFunction
    {
        public object? Evaluate(object? value)
        {
            var previous = context.CurrentObject.Value;
            context.CurrentObject.Set(value);
            try
            {
                return expression.Evaluate(value);
            }
            finally
            {
                context.CurrentObject.Set(previous);
            }
        }
    }

    private IFunction BuildRecordFunction(Parsers.Function function, IContext context)
    {
        if (function.Parameters.Length == 0)
            return new RecordFunction();

        if (function.Parameters.Length != 1 || function.Parameters[0] is not RecordDefinitionParameter definition)
            throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);

        var explicitNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var entry in definition.Entries.OfType<RecordNamedEntry>())
        {
            if (!explicitNames.Add(entry.Name))
                throw new ParseException($"Duplicate explicit field '{entry.Name}' in record(...).");
        }

        var evaluators = new List<RecordEntryEvaluator>();
        foreach (var entry in definition.Entries)
        {
            switch (entry)
            {
                case RecordSpreadEntry:
                    evaluators.Add(RecordEntryEvaluator.Spread());
                    break;
                case RecordNamedEntry named:
                    evaluators.Add(RecordEntryEvaluator.Named(named.Name, BuildRecordNamedValueEvaluator(named.Value, context)));
                    break;
                default:
                    throw new ParseException($"Unsupported entry type '{entry.GetType().Name}' in record(...).");
            }
        }

        return new RecordFunction(() => [.. evaluators]);
    }

    private Func<object?, object?> BuildRecordNamedValueEvaluator(IParameter parameter, IContext context)
    {
        if (parameter is IncomingValueParameter)
            return input => input;

        if (parameter is QuotedLiteralParameter quoted)
            return _ => quoted.Value;

        if (parameter is LiteralParameter literal)
            return RecordSyntax.TryParseTypedToken(literal.Value, out var typed)
                ? _ => typed
                : _ => literal.Value;

        if (parameter is OpenExpressionParameter open)
            return BuildOpenExpressionRecordEvaluator(open, context);

        var provider = CreateParameter(parameter, typeof(object), context);
        return _ => provider.DynamicInvoke();
    }

    private Func<object?, object?> BuildOpenExpressionRecordEvaluator(OpenExpressionParameter open, IContext context)
    {
        if (TryBuildSingleTokenEvaluator(open, out var evaluator))
            return evaluator;

        try
        {
            var functions = open.Expression.Members.Select(member => InstantiateOrWrapAggregation(member, context)).ToArray();
            var chain = new ChainFunction(functions);
            return input => chain.Evaluate(input);
        }
        catch (NotImplementedFunctionException) when (IsSingleTokenExpression(open))
        {
            var literalToken = open.Expression.Members.First().Name;
            return RecordSyntax.TryParseTypedToken(literalToken, out var literalTyped)
                ? _ => literalTyped
                : _ => literalToken;
        }
    }

    private static bool TryBuildSingleTokenEvaluator(OpenExpressionParameter open, out Func<object?, object?> evaluator)
    {
        evaluator = null!;
        if (!IsSingleTokenExpression(open))
            return false;

        var literalToken = open.Expression.Members.First().Name;
        if (!RecordSyntax.TryParseTypedToken(literalToken, out var literalTyped))
            return false;

        evaluator = _ => literalTyped;
        return true;
    }

    private static bool IsSingleTokenExpression(OpenExpressionParameter open)
        => open.Expression.Members.Count() == 1 && open.Expression.Members.First().Parameters.Length == 0;

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
        => () => new ChainFunction(parameter.Expression.Members.Select(member => InstantiateTransformationMember(member, context)).ToArray());

    private IFunction InstantiateTransformationMember(Parsers.Function member, IContext context)
    {
        if (!TypeMapper.TryExecute(member.Name, out _) && PredicateTypeMapper.TryExecute(member.Name, out _))
            return new PredicationFactory().Instantiate(new SinglePredication(member), context);

        return InstantiateOrWrapAggregation(member, context);
    }

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
            if (members.Any(x => x.Syntax == FunctionSyntax.FieldShorthand))
            {
                var functions = members.Select(member => new ExpressionFactory().Instantiate(member.Name, member.Parameters, context)).ToArray();
                return () => new BooleanExpressionPredicate(new ChainFunction(functions));
            }

            if (members.Length != 1)
                throw new MissingOrUnexpectedParametersFunctionException(functionName, members.Length);

            var predication = new SinglePredication(members.Single());
            return () => factory.Instantiate(predication, context);
        }
    }

    private sealed class BooleanExpressionPredicate(IFunction expression) : IPredicate
    {
        public bool Evaluate(object? value)
            => expression.Evaluate(value) is true;

        object? IFunction.Evaluate(object? value)
            => Evaluate(value);
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
