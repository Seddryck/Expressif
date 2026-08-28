using Expressif.Bindings;
using Expressif.Functions.Array;
using Expressif.Accumulators;
using Expressif.Accumulators.Introspection;
using Expressif.Predicates;
using Expressif.Values;
using Expressif.Functions.Coercions;
using RecordEntryEvaluator = Expressif.Functions.Record.RecordEntryEvaluator;
using RecordFunction = Expressif.Functions.Record.Record;
using ArrayFunction = Expressif.Functions.Array.Array;
using TextArgumentEvaluator = Expressif.Functions.Text.TextArgumentEvaluator;
using TextFunction = Expressif.Functions.Text.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using LinqExpression = System.Linq.Expressions.Expression;

namespace Expressif.Functions;

public class FunctionFactory : BaseExpressionFactory
{
    private static readonly PredicateTypeMapper PredicateTypeMapper = new();
    private static readonly HashSet<string> ImplicitFoldAccumulators = new(
        new AccumulatorIntrospector().Locate().Select(x => x.Name),
        StringComparer.OrdinalIgnoreCase
    );
    private static readonly CoercionRegistry CoercionRegistry = new();

    public FunctionFactory()
        : base(new FunctionTypeMapper()) { }

    protected override Delegate CreateParameter(IParameter parameter, Type scalarType, IContext context)
    {
        if (parameter is OpenExpressionParameter open)
        {
            var evaluator = BuildOpenExpressionRecordEvaluator(open, context);
            return CreateFunctionCast(
                () => evaluator.Invoke(context.CurrentObject.Value ?? EvaluationRuntime.Frame?.Current),
                scalarType);
        }

        return base.CreateParameter(parameter, scalarType, context);
    }

    public IFunction Instantiate(IRootExpression rootExpression, IContext context)
    {
        return rootExpression switch
        {
            OpenRootExpression open => BuildOpenExpression(open.Expression, context),
            ClosedRootExpression closed => BuildClosedExpression(closed.Expression, context),
            _ => throw new BindingException($"Unsupported expression root '{rootExpression.GetType().Name}'.")
        };
    }

    public IFunction Instantiate(string name, IParameter[] parameters, IContext context)
        => Instantiate<IFunction>(name, parameters, context);

    public IFunction Instantiate(Type type, IParameter[] parameters, IContext context)
        => Instantiate<IFunction>(type, parameters, context);

    public IFunction InstantiateClosed(IRootExpression rootExpression, IContext context)
    {
        return rootExpression switch
        {
            ClosedRootExpression closed => BuildClosedExpression(closed.Expression, context),
            OpenRootExpression open => throw new ExpressionRequiresInputException(open.Expression.Members.FirstOrDefault()?.Name),
            _ => throw new BindingException($"Unsupported expression root '{rootExpression.GetType().Name}'.")
        };
    }

    private IFunction BuildOpenExpression(OpenExpression expression, IContext context)
        => BuildPipeline(expression, context);

    internal IFunction Instantiate(OpenExpression expression, IContext context)
        => BuildOpenExpression(expression, context);

    private IFunction BuildPipeline(OpenExpression expression, IContext context)
    {
        var members = expression.Members.ToArray();
        var functions = members
            .Select(member => InstantiateOrWrapAggregation(member, context))
            .ToList();

        if (functions is [IPredicate predicate])
            return predicate;

        return TryBuildTypedChain(members, functions, out var chain)
            ? chain
            : new ChainFunction(functions);
    }

    internal static bool TryBuildTypedChain(
        IReadOnlyList<Bindings.Function> members,
        List<IFunction> functions,
        [NotNullWhen(true)] out IFunction? chain)
    {
        chain = null;
        if (functions.Count == 0 || !TrySelectInitialContract(functions[0], out var initial))
            return false;

        var contracts = new List<(Type Input, Type Output, Type Contract)> { initial };
        var outputType = initial.Output;
        for (var index = 1; index < functions.Count; index++)
        {
            var descriptor = CoercionRegistry.Descriptors.SingleOrDefault(
                candidate => candidate.Name.Equals(members[index].Name, StringComparison.OrdinalIgnoreCase));
            if (descriptor is not null
                && CoercionRegistry.TryCreate(outputType, descriptor.TargetType, out var coercion))
            {
                functions[index] = coercion;
            }

            if (!TrySelectFollowingContract(functions[index], outputType, out var contract))
                return false;

            contracts.Add(contract);
            outputType = contract.Output;
        }

        var inputType = initial.Input;
        var parameter = LinqExpression.Parameter(inputType, "value");
        LinqExpression body = parameter;
        for (var index = 0; index < functions.Count; index++)
        {
            var contract = contracts[index];
            var argument = body.Type == contract.Input
                ? body
                : LinqExpression.Convert(body, contract.Input);
            body = LinqExpression.Call(
                LinqExpression.Convert(LinqExpression.Constant(functions[index]), contract.Contract),
                contract.Contract.GetMethod(nameof(IFunction.Evaluate))!,
                argument);
        }

        var delegateType = typeof(Func<,>).MakeGenericType(inputType, outputType);
        var pipeline = LinqExpression.Lambda(delegateType, body, parameter).Compile();
        var chainType = typeof(ChainFunction<,>).MakeGenericType(inputType, outputType);
        chain = (IFunction)Activator.CreateInstance(chainType, functions, pipeline)!;
        return true;
    }

    private static bool TrySelectInitialContract(
        IFunction function,
        out (Type Input, Type Output, Type Contract) contract)
    {
        var candidates = GetContracts(function)
            .Where(candidate => candidate.Input != typeof(object))
            .ToArray();
        if (candidates.Length != 1)
        {
            contract = default;
            return false;
        }

        contract = candidates[0];
        return true;
    }

    private static bool TrySelectFollowingContract(
        IFunction function,
        Type outputType,
        out (Type Input, Type Output, Type Contract) contract)
    {
        var candidates = GetContracts(function);
        var exact = candidates.Where(candidate => candidate.Input == outputType).ToArray();
        var compatible = exact.Length > 0
            ? exact
            : candidates.Where(candidate => candidate.Input.IsAssignableFrom(outputType)).ToArray();
        if (compatible.Length != 1)
        {
            contract = default;
            return false;
        }

        contract = compatible[0];
        return true;
    }

    private static (Type Input, Type Output, Type Contract)[] GetContracts(IFunction function)
        => function.GetType().GetInterfaces()
            .Where(candidate => candidate.IsGenericType
                && candidate.GetGenericTypeDefinition() == typeof(IFunction<,>))
            .Select(candidate => (
                candidate.GetGenericArguments()[0],
                candidate.GetGenericArguments()[1],
                candidate))
            .Distinct()
            .ToArray();

    private IFunction BuildClosedExpression(Bindings.ClosedExpression expression, IContext context)
    {
        var sourceEvaluator = BuildSourceEvaluator(expression.Parameter, context);
        var functions = new List<IFunction>();
        foreach (var member in expression.Members)
            functions.Add(InstantiateOrWrapAggregation(member, context));

        return new DelegatedFunction(input =>
        {
            var source = sourceEvaluator.Invoke(input);
            return functions.Aggregate(source, (current, function) => function.Evaluate(current));
        });
    }

    private Func<object?, object?> BuildSourceEvaluator(IParameter parameter, IContext context)
    {
        if (parameter is IncomingValueParameter
            or ArrayParameter
            or TupleParameter
            or RecordLiteralParameter
            or InputExpressionParameter)
            return BuildValueEvaluator(parameter, context);

        var provider = CreateParameter(parameter, typeof(object), context);
        return _ => provider.DynamicInvoke();
    }

    private IFunction InstantiateOrWrapAggregation(Bindings.Function function, IContext context)
    {
        var name = function.Name.ToKebabCase();

        if (function.Arguments.Any(x => x.Name is not null)
            && name is "record" or "coalesce")
            throw new UnknownParameterNameException(name, function.Arguments.First(x => x.Name is not null).Name!);

        if (function.Arguments.Any(x => x.Name is not null) && name == "adjacent")
            function = new Bindings.Function(name, ParameterArgumentBinder.Bind(TypeMapper.Execute(name), function.Arguments).Parameters);

        if (name.Equals("record", StringComparison.OrdinalIgnoreCase))
            return BuildRecordFunction(function, context);
        if (name.Equals("with", StringComparison.OrdinalIgnoreCase))
            return BuildWithFunction(function, context);
        if (name.Equals("array", StringComparison.OrdinalIgnoreCase))
            return BuildArrayFunction(function, context);
        if (name.Equals("text", StringComparison.OrdinalIgnoreCase))
            return BuildTextFunction(function, context);

        if (name.Equals("coalesce", StringComparison.OrdinalIgnoreCase))
            return BuildCoalesceFunction(function, context);
        if (name.Equals("adjacent", StringComparison.OrdinalIgnoreCase))
            return BuildAdjacentFunction(function, context);

        if (name.Equals("extend", StringComparison.OrdinalIgnoreCase))
        {
            if (function.Arguments is not [var extension]
                || (extension.Name is not null && !extension.Name.Equals("value", StringComparison.OrdinalIgnoreCase)))
                throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);
            var evaluator = BuildValueEvaluator(extension.Value, context);
            return new Tuple.Extend(value => evaluator.Invoke(value));
        }
        if (name.Equals("pick", StringComparison.OrdinalIgnoreCase))
        {
            var positions = function.Arguments
                .Select(argument => (Func<int>)CreateParameter(argument.Value, typeof(int), context))
                .ToArray();
            return new Tuple.Pick(() => positions.Select(position => position.Invoke()).ToArray());
        }

        if (name.Equals("apply", StringComparison.OrdinalIgnoreCase))
        {
            if (function.Parameters.Length != 1 || !TryGetOpenExpression(function.Parameters[0], out var open))
                throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);
            var expression = new LexicallyBoundTupleFunction(BuildOpenExpression(open.Expression, context));
            return new Tuple.Apply(() => expression);
        }

        if (ImplicitFoldAccumulators.Contains(name) && function.Parameters.Length == 0)
            return new Fold(() => name);

        if (!TypeMapper.TryExecute(function.Name, out var type))
        {
            if (PredicateTypeMapper.TryExecute(function.Name, out _))
                return new PredicationFactory().Instantiate(new SinglePredication(function), context);

            throw new NotImplementedFunctionException(function.Name);
        }

        if (TryInstantiateWithAccumulatorProvider(type, function, context, out var aggregation))
            return aggregation;

        if (TryInstantiateWithTransformationProvider(type, function, context, out var transformation))
            return transformation;

        if (TryInstantiateWithPredicateProvider(type, function, context, out var filtering))
            return filtering;

        return Instantiate<IFunction>(type, function.Arguments, context);
    }

    private IFunction BuildArrayFunction(Bindings.Function function, IContext context)
    {
        var values = function.Arguments
            .Select(argument => new ArrayArgumentEvaluator(
                BuildValueEvaluator(argument.Value, context),
                argument.IsSpread))
            .ToArray();
        return new ArrayFunction(() => values);
    }

    private IFunction BuildTextFunction(Bindings.Function function, IContext context)
    {
        var values = function.Arguments
            .Select(argument => BuildTextArgumentEvaluator(argument, context))
            .ToArray();
        return new TextFunction(() => values);
    }

    private TextArgumentEvaluator BuildTextArgumentEvaluator(FunctionArgument argument, IContext context)
    {
        if (!argument.IsSpread || argument.Value is not InputExpressionParameter inputExpression)
            return new TextArgumentEvaluator(BuildValueEvaluator(argument.Value, context), argument.IsSpread);

        var source = BuildValueEvaluator(inputExpression.Expression.Parameter, context);
        var chain = new ChainFunction(inputExpression.Expression.Members
            .Select(member => InstantiateOrWrapAggregation(member, context))
            .ToArray());
        return new TextArgumentEvaluator(input =>
        {
            var items = new List<object?>();
            SpreadValues.Append(source.Invoke(input), items);
            return items
                .Select(item => WithCurrentObject(context, item, () => chain.Evaluate(item)))
                .ToArray();
        }, true);
    }

    private Func<object?, object?> BuildValueEvaluator(IParameter parameter, IContext context)
    {
        if (parameter is IncomingValueParameter)
            return input => input;

        if (parameter is OpenExpressionParameter open)
        {
            var evaluator = BuildOpenExpressionRecordEvaluator(open, context);
            return input => WithCurrentObject(context, input, () => evaluator.Invoke(input));
        }

        if (parameter is InputExpressionParameter inputExpression)
        {
            var source = BuildValueEvaluator(inputExpression.Expression.Parameter, context);
            var chain = new ChainFunction(inputExpression.Expression.Members
                .Select(member => InstantiateOrWrapAggregation(member, context))
                .ToArray());
            return input => WithCurrentObject(
                context,
                input,
                () => chain.Evaluate(source.Invoke(input)));
        }

        if (parameter is ArrayParameter array)
        {
            var elements = array.Elements
                .Select(element => new ArrayArgumentEvaluator(
                    BuildValueEvaluator(element.Value, context),
                    element.IsSpread))
                .ToArray();
            var function = new ArrayFunction(() => elements);
            return function.Evaluate;
        }

        if (parameter is RecordLiteralParameter record)
        {
            var fields = record.Fields
                .Select(field => new
                {
                    field.Name,
                    Evaluator = BuildValueEvaluator(field.Value, context),
                })
                .ToArray();
            return input =>
            {
                var value = new RecordValue();
                foreach (var field in fields)
                    value.Set(field.Name, field.Evaluator.Invoke(input));
                return value;
            };
        }

        var provider = (Func<object?>)CreateParameter(parameter, typeof(object), context);
        return input => WithCurrentObject(context, input, provider);
    }

    private static object? WithCurrentObject(IContext context, object? input, Func<object?> evaluator)
    {
        var previous = context.CurrentObject.Value;
        context.CurrentObject.Set(input);
        try
        {
            return evaluator.Invoke();
        }
        finally
        {
            context.CurrentObject.Set(previous);
        }
    }

    private IFunction BuildCoalesceFunction(Bindings.Function function, IContext context)
    {
        if (function.Parameters.Length < 2)
        {
            throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);
        }

        var expressions = function.Parameters.Select(parameter => BuildCoalesceExpressionEvaluator(parameter, context));
        return new Special.Coalesce(expressions);
    }

    private Func<object?, object?> BuildCoalesceExpressionEvaluator(IParameter parameter, IContext context)
    {
        if (parameter is IncomingValueParameter)
        {
            return input => input;
        }

        if (parameter is OpenExpressionParameter open)
        {
            return TryBuildCoalesceFieldEvaluator(open, context, out var evaluator)
                ? evaluator
                : BuildOpenExpressionRecordEvaluator(open, context);
        }

        var provider = CreateParameter(parameter, typeof(object), context);
        return _ => provider.DynamicInvoke();
    }

    private bool TryBuildCoalesceFieldEvaluator(
        OpenExpressionParameter open,
        IContext context,
        [NotNullWhen(true)] out Func<object?, object?>? evaluator)
    {
        evaluator = null;
        var members = open.Expression.Members.ToArray();
        if (members.Length == 0
            || !members[0].Name.Equals("field", StringComparison.OrdinalIgnoreCase)
            || !TryGetFieldName(members[0].Parameters, out var fieldName))
        {
            return false;
        }

        var remainder = new ChainFunction(
            members.Skip(1).Select(member => InstantiateOrWrapAggregation(member, context)).ToArray());
        evaluator = input => NamedValueAccessor.TryGetValue(input, fieldName, out var fieldValue)
            ? remainder.Evaluate(fieldValue)
            : null;
        return true;
    }

    private static bool TryGetFieldName(IParameter[] parameters, out string fieldName)
    {
        fieldName = parameters switch
        {
            [LiteralParameter { Value: string value }] => value,
            [QuotedLiteralParameter quoted] => quoted.Value,
            _ => string.Empty
        };
        return parameters is [LiteralParameter] or [QuotedLiteralParameter];
    }

    private IFunction BuildAdjacentFunction(Bindings.Function function, IContext context)
    {
        if (function.Parameters.Length != 1 || !TryGetOpenExpression(function.Parameters[0], out var open))
            throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);

        var members = open.Expression.Members.ToArray();
        IFunction operation;
        if (members.Length == 1 && members[0].Parameters.Length == 0 && TryBuildBinaryCallable(members[0].Name, context, out var callable))
        {
            operation = new ChainFunction([
                InstantiateOrWrapAggregation(new Bindings.Function("tuple-at", [new LiteralParameter("1")]), context),
                callable
            ]);
        }
        else
        {
            operation = BuildOpenExpression(open.Expression, context);
        }

        return new Adjacent(() => new LexicallyBoundTupleFunction(operation));
    }

    private bool TryBuildBinaryCallable(string name, IContext context, [NotNullWhen(true)] out IFunction? callable)
    {
        var parameterized = new Bindings.Function(name, [new TupleProjectionParameter(0)]);
        if (TypeMapper.TryExecute(name, out var functionType))
        {
            if (!functionType.GetConstructors().Any(x => x.GetParameters().Length == 1))
            {
                callable = null;
                return false;
            }
            callable = InstantiateOrWrapAggregation(parameterized, context);
            return true;
        }

        if (PredicateTypeMapper.TryExecute(name, out var predicateType))
        {
            if (!predicateType.GetConstructors().Any(x => x.GetParameters().Length == 1))
            {
                callable = null;
                return false;
            }
            callable = InstantiateOrWrapAggregation(parameterized, context);
            return true;
        }

        callable = null;
        return false;
    }

    private sealed class LexicallyBoundTupleFunction(IFunction expression) : IFunction
    {
        public object? Evaluate(object? value)
        {
            using var scope = EvaluationRuntime.Derive(value);
            return expression.Evaluate(value);
        }
    }

    private IFunction BuildRecordFunction(Bindings.Function function, IContext context)
    {
        if (function.Parameters.Length == 0)
            return new RecordFunction();

        if (function.Parameters.Length != 1 || function.Parameters[0] is not RecordDefinitionParameter definition)
            throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);

        var explicitNames = new HashSet<string>(StringComparer.Ordinal);
        var duplicateEntry = definition.Entries
            .OfType<RecordNamedEntry>()
            .FirstOrDefault(entry => !explicitNames.Add(entry.Name));
        if (duplicateEntry is not null)
            throw new BindingException($"Duplicate explicit field '{duplicateEntry.Name}' in record(...).");

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
                    throw new BindingException($"Unsupported entry type '{entry.GetType().Name}' in record(...).");
            }
        }

        return new RecordFunction(() => [.. evaluators]);
    }

    private IFunction BuildWithFunction(Bindings.Function function, IContext context)
    {
        if (function.Parameters is not [WithDefinitionParameter definition])
            throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);

        var projections = definition.Projections
            .Select(projection => RecordEntryEvaluator.Named(
                projection.Name,
                BuildValueEvaluator(projection.Value, context)))
            .ToArray();
        var body = BuildValueEvaluator(definition.Body, context);
        return new Expressif.Functions.Record.With(() => projections, body);
    }

    private Func<object?, object?> BuildRecordNamedValueEvaluator(IParameter parameter, IContext context)
    {
        if (parameter is IncomingValueParameter)
            return input => input;

        if (parameter is QuotedLiteralParameter quoted)
            return _ => quoted.Value;

        if (parameter is LiteralParameter literal)
            return _ => literal.Value;

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

    private static bool TryBuildSingleTokenEvaluator(
        OpenExpressionParameter open,
        [NotNullWhen(true)] out Func<object?, object?>? evaluator)
    {
        evaluator = null;
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

    private bool TryInstantiateWithAccumulatorProvider(
        Type type,
        Bindings.Function function,
        IContext context,
        [NotNullWhen(true)] out IFunction? aggregation)
    {
        aggregation = null;

        var ctor = type.GetConstructors()
                       .FirstOrDefault(x => x.GetParameters().Length == 1
                                         && x.GetParameters()[0].ParameterType == typeof(Func<IAccumulator>));
        if (ctor is null)
        {
            return false;
        }

        if (function.Parameters.Length != 1)
        {
            throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);
        }

        aggregation = (IFunction)ctor.Invoke([BuildAccumulatorProvider(function.Parameters[0], context)]);
        return true;
    }

    private Func<IAccumulator> BuildAccumulatorProvider(IParameter parameter, IContext context)
    {
        var nameProvider = BuildAccumulatorNameProvider(parameter, context);
        return () => AccumulatorFactory.Instantiate(nameProvider.Invoke());
    }

    private bool TryInstantiateWithTransformationProvider(
        Type type,
        Bindings.Function function,
        IContext context,
        [NotNullWhen(true)] out IFunction? transformation)
    {
        transformation = null;

        var ctor = type.GetConstructors()
                       .FirstOrDefault(x => x.GetParameters().Length == 1
                                         && x.GetParameters()[0].ParameterType == typeof(Func<IFunction>));
        if (ctor is null)
            return false;

        if (function.Parameters.Length != 1)
            throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);

        if (!TryGetOpenExpression(function.Parameters[0], out var openExpression))
        {
            throw new ArgumentException(
                $"The function named '{function.Name}' expects a parameter of type '{nameof(OpenExpressionParameter)}' but received '{function.Parameters[0].GetType().Name}'.",
                nameof(function));
        }

        transformation = (IFunction)ctor.Invoke([BuildTransformationProvider(openExpression, context)]);
        return true;
    }

    private Func<IFunction> BuildTransformationProvider(OpenExpressionParameter parameter, IContext context)
        => () => BuildPipeline(parameter.Expression, context);

    private bool TryInstantiateWithPredicateProvider(
        Type type,
        Bindings.Function function,
        IContext context,
        [NotNullWhen(true)] out IFunction? filtering)
    {
        filtering = null;

        var ctor = type.GetConstructors()
                       .FirstOrDefault(x => x.GetParameters().Length == 1
                                         && x.GetParameters()[0].ParameterType == typeof(Func<IPredicate>));
        if (ctor is null)
        {
            return false;
        }

        if (function.Parameters.Length != 1)
        {
            throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);
        }

        if (function.Parameters[0] is not PredicationParameter && !TryGetOpenExpression(function.Parameters[0], out _))
        {
            throw new ArgumentException(
                $"The function named '{function.Name}' expects a parameter of type '{nameof(PredicationParameter)}' or '{nameof(OpenExpressionParameter)}' but received '{function.Parameters[0].GetType().Name}'.",
                nameof(function));
        }

        filtering = (IFunction)ctor.Invoke([BuildPredicateProvider(function.Parameters[0], context, function.Name)]);
        return true;
    }

    private Func<IPredicate> BuildPredicateProvider(IParameter parameter, IContext context, string functionName)
    {
        var factory = new PredicationFactory();
        if (TryGetOpenExpression(parameter, out var openExpression))
            return () => BuildBooleanPredicate(openExpression.Expression, context);

        return parameter switch
        {
            PredicationParameter predication => () => factory.Instantiate(predication.Predication, context),
            _ => throw new ArgumentException(
                    $"The function named '{functionName}' expects a parameter of type '{nameof(PredicationParameter)}' or '{nameof(OpenExpressionParameter)}' but received '{parameter.GetType().Name}'.",
                    nameof(parameter))
        };
    }

    private IPredicate BuildBooleanPredicate(OpenExpression expression, IContext context)
    {
        var function = BuildOpenExpression(expression, context);
        return function as IPredicate ?? new BooleanFunctionPredicate(function);
    }

    private static bool TryGetOpenExpression(
        IParameter parameter,
        [NotNullWhen(true)] out OpenExpressionParameter? expression)
    {
        expression = parameter switch
        {
            OpenExpressionParameter open => open,
            LiteralParameter { Value: string value } => new OpenExpressionParameter(
                new OpenExpression([new Bindings.Function(value, [])])),
            _ => null
        };
        return expression is not null;
    }

    protected override Delegate CreateInputExpression(InputExpressionParameter input, Type type, IContext context)
    {
        var expression = Instantiate(new ClosedRootExpression(input.Expression), context);
        return CreateFunctionCast(() => expression.Evaluate(null), type);
    }

    private Func<string> BuildAccumulatorNameProvider(IParameter parameter, IContext context)
    {
        if (parameter is OpenExpressionParameter open && IsSingleTokenExpression(open))
        {
            var accumulator = open.Expression.Members.Single();
            return () => accumulator.Name;
        }

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
