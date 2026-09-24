using Expressif.Bindings;
using Expressif.Semantics;
using Expressif.Functions.Accumulation;
using Expressif.Predicates;
using Expressif.Values;
using Expressif.Functions.Coercions;
using Expressif.Discovery;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using LinqExpression = System.Linq.Expressions.Expression;

namespace Expressif.Functions;

internal sealed partial class FunctionFactoryRuntime : BaseExpressionFactory, IFunctionConstructionContext
{
    private readonly IImplementationRegistry predicateRegistry;
    private readonly AccumulatorRegistry accumulatorRegistry;
    private readonly CoercionRegistry coercionRegistry;
    private readonly FunctionConstructorRegistry constructors;
    private readonly IPredicationFactory predicationFactory;
    private readonly ITupleFunctionInvoker tupleBinding;

    public FunctionFactoryRuntime(ITypeSource source)
        : this(
            new FunctionRegistry(source),
            new PredicateRegistry(source),
            new AccumulatorRegistry(source),
            new CoercionRegistry(source),
            new FunctionConstructorRegistry(source),
            TypeSourceService.Create<IPredicationFactory>(source),
            TypeSourceService.Create<ITupleFunctionInvoker>(source),
            source) { }

    public FunctionFactoryRuntime(IImplementationRegistry registry, ITypeSource source)
        : this(
            registry,
            new PredicateRegistry(source),
            new AccumulatorRegistry(source),
            new CoercionRegistry(source),
            new FunctionConstructorRegistry(source),
            TypeSourceService.Create<IPredicationFactory>(source),
            TypeSourceService.Create<ITupleFunctionInvoker>(source),
            source) { }

    public FunctionFactoryRuntime(
        IImplementationRegistry registry,
        IImplementationRegistry predicateRegistry,
        AccumulatorRegistry accumulatorRegistry,
        CoercionRegistry coercionRegistry,
        FunctionConstructorRegistry constructors,
        IPredicationFactory predicationFactory,
        ITupleFunctionInvoker tupleBinding,
        ITypeSource source)
        : base(registry, source)
        => (this.predicateRegistry, this.accumulatorRegistry, this.coercionRegistry, this.constructors,
                this.predicationFactory, this.tupleBinding)
            = (predicateRegistry, accumulatorRegistry, coercionRegistry, constructors, predicationFactory, tupleBinding);

    Delegate IFunctionConstructionContext.CreateParameter(
        IParameter parameter,
        Type targetType,
        IContext context)
        => CreateParameter(parameter, targetType, context);

    IFunction IFunctionConstructionContext.CreateOpenExpression(
        OpenExpression expression,
        IContext context)
        => BuildOpenExpression(expression, context);

    Func<object?, object?> IFunctionConstructionContext.CreateOpenExpressionValueEvaluator(
        OpenExpressionParameter expression,
        IContext context)
        => BuildOpenExpressionRecordEvaluator(expression, context);

    Func<object?, object?> IFunctionConstructionContext.CreateValueEvaluator(
        IParameter parameter,
        IContext context,
        bool establishScope)
        => BuildValueEvaluator(parameter, context, establishScope);

    IFunction IFunctionConstructionContext.CreateFunction(
        Bindings.Function function,
        IContext context)
        => InstantiateOrWrapAggregation(function, context);

    Func<IPredicate> IFunctionConstructionContext.CreatePredicateProvider(
        IParameter parameter,
        IContext context,
        string functionName)
        => BuildPredicateProvider(parameter, context, functionName);

    Func<IAccumulator> IFunctionConstructionContext.CreateAccumulatorProvider(
        IParameter parameter,
        IContext context)
        => BuildAccumulatorProvider(parameter, context);

    Func<IFunction> IFunctionConstructionContext.CreateTransformationProvider(
        OpenExpressionParameter parameter,
        IContext context)
        => BuildTransformationProvider(parameter, context);

    bool IFunctionConstructionContext.TryResolveImplementation(
        string name,
        out Type implementationType)
        => Registry.TryResolve(name, out implementationType!)
            || predicateRegistry.TryResolve(name, out implementationType!);

    Type IFunctionConstructionContext.ResolveTupleTarget(string name, Syntax.SourceSpan? sourceSpan)
        => ResolveTupleTarget(name, sourceSpan);

    object? IFunctionConstructionContext.InvokeTuple(
        string name,
        IPositionalValue tuple,
        Syntax.SourceSpan? sourceSpan)
        => InvokeTuple(name, tuple, sourceSpan);

    internal IPredicate InstantiatePredication(IPredication predication, IContext context)
        => predicationFactory.Instantiate(predication, context);

    protected override Delegate CreateParameter(IParameter parameter, Type scalarType, IContext context)
    {
        if (parameter is PairParameter or GroupingParameter or DictionaryParameter)
        {
            var evaluator = BuildStructuredValueEvaluator(parameter, context)!;
            return CreateFunctionCast(
                () => evaluator.Invoke(EvaluationRuntime.Frame is { IsInputBound: true } frame
                    ? frame.Current : ArgumentScope.Root(context.CurrentObject.Value, EvaluationRuntime.Frame?.Current)),
                scalarType);
        }

        if (parameter is OpenExpressionParameter open)
        {
            var evaluator = IsExplicitlyRooted(open)
                ? BuildPipeline(open.Expression, context).Evaluate
                : BuildOpenExpressionRecordEvaluator(open, context);
            return CreateFunctionCast(
                () => evaluator.Invoke(EvaluationRuntime.Frame is { IsInputBound: true } frame
                    ? frame.Current : ArgumentScope.Root(context.CurrentObject.Value, EvaluationRuntime.Frame?.Current)),
                scalarType);
        }

        return base.CreateParameter(parameter, scalarType, context);
    }

    internal static bool IsExplicitlyRooted(OpenExpressionParameter expression)
        => expression.Expression.Members.FirstOrDefault()?.Syntax
            is FunctionSyntax.RootFieldShorthand or FunctionSyntax.EnclosingRootFieldShorthand;

    internal static bool IsExplicitlyRooted(IParameter parameter)
        => parameter is ObjectPropertyParameter or EnclosingObjectPropertyParameter
            || (parameter is InputExpressionParameter expression
                && IsExplicitlyRooted(expression.Expression.Parameter));

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
        if (expression.InputBinding is { } binding)
            return BuildInputBoundFunction(binding, context);

        var members = expression.Members.ToArray();
        var functions = members
            .Select(member => InstantiateOrWrapAggregation(member, context))
            .ToList();

        if (functions.Count == 1 && functions[0] is IPredicate)
            return functions[0];

        return TryBuildTypedChain(members, functions, out var chain)
            ? chain
            : new ChainFunction(functions);
    }

    private IFunction BuildInputBoundFunction(InputBoundExpression binding, IContext context)
    {
        var members = binding.Body switch
        {
            OpenRootExpression open => open.Expression.Members,
            ClosedRootExpression closed => closed.Expression.Members,
            _ => throw new BindingException("Unsupported input binding body."),
        };
        var chain = BuildPipeline(new OpenExpression(members), context);
        Func<object?, object?> source = binding.Body is ClosedRootExpression closedBody
            ? BuildSourceEvaluator(closedBody.Expression.Parameter, context)
            : input => input;
        return new InputBoundFunction(binding, input => chain.Evaluate(source(input)));
    }

    internal bool TryBuildTypedChain(
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
            if (coercionRegistry.TryResolve(members[index].Name, out var targetType)
                && coercionRegistry.TryCreate(outputType, targetType, out var coercion))
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

        var pipeline = new ChainFunction(functions);
        return new DelegatedFunction(input =>
        {
            var source = sourceEvaluator.Invoke(input);
            using var scope = EvaluationRuntime.Derive(source);
            return pipeline.Evaluate(source);
        });
    }

    private Func<object?, object?> BuildSourceEvaluator(IParameter parameter, IContext context)
    {
        if (parameter is IncomingValueParameter or ArrayParameter or TupleParameter
            or PairParameter or GroupingParameter or DictionaryParameter
            or RecordLiteralParameter or InputExpressionParameter)
            return BuildValueEvaluator(parameter, context);

        var provider = CreateParameter(parameter, typeof(object), context);
        return _ => provider.DynamicInvoke();
    }

    private IFunction InstantiateOrWrapAggregation(Bindings.Function function, IContext context)
    {
        if (function.Syntax == FunctionSyntax.InputBindingStage
            && function.Parameters is [OpenExpressionParameter { Expression.InputBinding: { } binding }])
            return BuildInputBoundFunction(binding, context);
        var name = function.Name.ToKebabCase();

        if (BuildReferenceFunction(function) is { } reference)
            return reference;

        var hasRegularFunction = Registry.TryResolve(name, out _);
        if (accumulatorRegistry.TryResolve(name, out var accumulatorType)
            && (function.Syntax == FunctionSyntax.ImplicitFoldAccumulator || !hasRegularFunction))
        {
            return BuildAccumulatorFunction(function, accumulatorType, context);
        }

        if (Registry.TryResolve(name, out var registeredType)
            || predicateRegistry.TryResolve(name, out registeredType))
        {
            if (constructors.TryGet(registeredType, out var constructor))
                return constructor.Construct(function, context, this);
            if (constructors.TryGetAnnotated(registeredType, out var annotated))
                return InstantiateAnnotated(registeredType, annotated, function, context);
            if (constructors.TryGetRoleAnnotated(registeredType, out var roleAnnotated))
                return InstantiateRoleAnnotated(registeredType, roleAnnotated, function, context);
            if (constructors.TryGetShapeAnnotated(registeredType, out var shapeAnnotated))
                return InstantiateShapeAnnotated(registeredType, shapeAnnotated, function, context);
            if (constructors.TryGetSpreadPacked(registeredType, out var spreadPacked))
                return InstantiateValueSpread(registeredType, spreadPacked, function, context);
        }

        if (!Registry.TryResolve(function.Name, out var type))
        {
            if (predicateRegistry.TryResolve(function.Name, out _))
            {
                return predicationFactory.Instantiate(new SinglePredication(function), context);
            }

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

    private IFunction InstantiateAnnotated(
        Type type,
        ConstructorInfo[] targets,
        Bindings.Function function,
        IContext context)
    {
        var collection = targets.SingleOrDefault(target => target.GetParameters() is
            [var parameter] && (parameter.ParameterType == typeof(IEnumerable<Func<object?, object?>>)
                || FunctionConstructorRegistry.TryGetNamedEvaluatorConstructor(parameter.ParameterType, out _)));
        if (collection is not null)
        {
            var parameter = collection.GetParameters()[0];
            var mode = parameter.GetCustomAttribute<ArgumentEvaluationAttribute>()!.Mode;
            var layout = ParameterArgumentBinder.BindLayout(type, function.Arguments);
            object values;
            if (parameter.ParameterType == typeof(IEnumerable<Func<object?, object?>>))
            {
                values = layout.Positional.Select(argument => mode == ArgumentEvaluationMode.Nested
                    ? BuildNestedValueEvaluator(argument.Value, context)
                    : BuildValueEvaluator(argument.Value, context)).ToArray();
            }
            else
            {
                FunctionConstructorRegistry.TryGetNamedEvaluatorConstructor(parameter.ParameterType, out var entry);
                var entries = System.Array.CreateInstance(entry.DeclaringType!, layout.Named.Length);
                for (var index = 0; index < layout.Named.Length; index++)
                {
                    var argument = layout.Named[index];
                    var evaluator = mode == ArgumentEvaluationMode.Nested
                        ? BuildNestedValueEvaluator(argument.Value, context)
                        : BuildValueEvaluator(argument.Value, context);
                    entries.SetValue(entry.Invoke([argument.Name!, evaluator]), index);
                }
                values = LinqExpression.Lambda(parameter.ParameterType,
                    LinqExpression.Constant(entries, entries.GetType())).Compile();
            }
            return collection.Invoke([values]) as IFunction
                ?? throw new InvalidOperationException(
                    $"Annotated constructor for '{type.FullName}' did not create a function.");
        }

        var binding = ParameterArgumentBinder.Bind(type, function.Arguments, targets);
        var metadata = binding.Constructor.GetParameters();
        var callbacks = new object?[metadata.Length];
        for (var index = 0; index < metadata.Length; index++)
        {
            var parameter = metadata[index];
            var mode = parameter.GetCustomAttribute<ArgumentEvaluationAttribute>()!.Mode;
            callbacks[index] = metadata[index].IsOptional && !binding.Supplied[index]
                && binding.Parameters[index] is LiteralParameter { Value: null }
                    ? null
                    : mode switch
                    {
                        ArgumentEvaluationMode.Incoming => BuildValueEvaluator(binding.Parameters[index], context),
                        ArgumentEvaluationMode.Nested => BuildNestedValueEvaluator(binding.Parameters[index], context),
                        ArgumentEvaluationMode.Ambient => BuildAmbientValueProvider(binding.Parameters[index], context),
                        _ => throw new InvalidOperationException($"Unsupported argument evaluation mode '{mode}'."),
                    };
        }
        return binding.Constructor.Invoke(callbacks) as IFunction
            ?? throw new InvalidOperationException(
                $"Annotated constructor for '{type.FullName}' did not create a function.");
    }

    private IFunction InstantiateRoleAnnotated(
        Type type,
        ConstructorInfo[] targets,
        Bindings.Function function,
        IContext context)
    {
        var binding = ParameterArgumentBinder.Bind(type, function.Arguments, targets);
        var metadata = binding.Constructor.GetParameters();
        var providers = new object?[metadata.Length];
        for (var index = 0; index < metadata.Length; index++)
        {
            var parameter = binding.Parameters[index];
            providers[index] = metadata[index].GetCustomAttribute<ArgumentRoleAttribute>()!.Role switch
            {
                ArgumentRole.Predicate => ApplyProviderLifetime(
                    BuildPredicateProvider(parameter, context, function.Name), metadata[index]),
                ArgumentRole.Accumulator => ApplyProviderLifetime(
                    BuildAccumulatorProvider(parameter, context), metadata[index]),
                ArgumentRole.Transformation => TryGetOpenExpression(parameter, out var open)
                    ? ApplyProviderLifetime(BuildTransformationProvider(open, context), metadata[index])
                    : throw new BindingException(
                        $"Function '{function.Name}' parameter '{metadata[index].Name}' must be an open expression."),
                _ => throw new InvalidOperationException($"Unsupported argument role on '{type.FullName}.{metadata[index].Name}'."),
            };
        }
        return binding.Constructor.Invoke(providers) as IFunction
            ?? throw new InvalidOperationException($"Role-annotated constructor for '{type.FullName}' did not create a function.");
    }

    private static Func<T> ApplyProviderLifetime<T>(Func<T> provider, ParameterInfo parameter)
        where T : class
    {
        if (parameter.GetCustomAttribute<ProviderLifetimeAttribute>()?.Lifetime != ProviderLifetime.BoundExpression)
            return provider;
        var value = provider();
        return () => value;
    }

    private IFunction InstantiateShapeAnnotated(
        Type type,
        ConstructorInfo[] targets,
        Bindings.Function function,
        IContext context)
    {
        var binding = ParameterArgumentBinder.Bind(type, function.Arguments, targets);
        var metadata = binding.Constructor.GetParameters();
        var values = new object?[metadata.Length];
        for (var index = 0; index < metadata.Length; index++)
        {
            var parameter = metadata[index];
            var shape = parameter.GetCustomAttribute<AcceptedExpressionShapeAttribute>()?.Shape
                ?? throw new InvalidOperationException(
                    $"Constructor '{type.FullName}' parameter '{parameter.Name}' has no expression shape metadata.");
            values[index] = shape switch
            {
                AcceptedExpressionShape.DirectFieldSelector => CreateDirectFieldSelector(
                    binding.Parameters[index], function.Name, parameter.Name!, context),
                AcceptedExpressionShape.OpenExpression => ExpressionShapeNormalizer.TryGetOpenExpression(
                    binding.Parameters[index], out var open)
                    ? BuildTransformationProvider(open, context)
                    : throw new BindingException(
                        $"Function '{function.Name}' parameter '{parameter.Name}' must be an open expression."),
                _ => throw new InvalidOperationException($"Unsupported expression shape '{shape}'."),
            };
        }
        return binding.Constructor.Invoke(values) as IFunction
            ?? throw new InvalidOperationException($"Shape-annotated constructor for '{type.FullName}' did not create a function.");
    }

    private NamedFieldSelector CreateDirectFieldSelector(
        IParameter parameter, string function, string parameterName, IContext context)
    {
        var name = ExpressionShapeNormalizer.RequireDirectFieldName(parameter, function, parameterName);
        var evaluator = new DelegatedFunction(BuildValueEvaluator(parameter, context));
        return new NamedFieldSelector(name, value => EvaluationRuntime.EvaluateNested(evaluator, value, value));
    }

    private Func<object?, object?> BuildNestedValueEvaluator(IParameter parameter, IContext context)
    {
        var evaluator = new DelegatedFunction(BuildValueEvaluator(parameter, context));
        return value => EvaluationRuntime.EvaluateNested(evaluator, value);
    }

    private Func<object?> BuildAmbientValueProvider(IParameter parameter, IContext context)
    {
        var evaluator = BuildValueEvaluator(parameter, context);
        return () => evaluator.Invoke(EvaluationRuntime.Frame?.Current);
    }

    private IFunction BuildAccumulatorFunction(
        Bindings.Function function,
        Type accumulatorType,
        IContext context)
    {
        if (TryBuildAccumulatorConstructor(function, accumulatorType, context, out var create))
        {
            _ = create();
            return new AccumulatorFunction(create);
        }

        if (function.Parameters.Length != 0)
            throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);

        return new AccumulatorFunction(() => accumulatorRegistry.Create(function.Name));
    }

    private bool TryBuildAccumulatorConstructor(
        Bindings.Function function,
        Type accumulatorType,
        IContext context,
        [NotNullWhen(true)] out Func<IAccumulator>? create)
    {
        create = null;
        if (constructors.TryGet(accumulatorType, out var constructor))
        {
            create = () => constructor.Construct(function, context, this) as IAccumulator
                ?? throw new InvalidOperationException(
                    $"The constructor for accumulator '{function.Name}' did not create an accumulator.");
        }
        else if (constructors.TryGetAnnotated(accumulatorType, out var annotated))
        {
            create = () => InstantiateAnnotated(accumulatorType, annotated, function, context) as IAccumulator
                ?? throw new InvalidOperationException(
                    $"The annotated constructor for accumulator '{function.Name}' did not create an accumulator.");
        }
        return create is not null;
    }

    private static IFunction? BuildReferenceFunction(Bindings.Function function)
    {
        if (function.Syntax == FunctionSyntax.ScopedTupleProjectionShorthand
            && function.Parameters is [ScopedTupleProjectionParameter scoped])
            return new DelegatedFunction(_ => ResolveScopedTupleProjection(scoped));
        if (function.Syntax == FunctionSyntax.InputTupleProjectionShorthand)
        {
            var position = int.Parse((string)((LiteralParameter)function.Parameters[0]).Value!, System.Globalization.CultureInfo.InvariantCulture);
            var projection = new TupleProjectionParameter(position < 0 ? position == int.MinValue ? 0 : -position : position, position < 0);
            return new DelegatedFunction(input => ResolveTupleProjection(
                EvaluationRuntime.Frame is { IsInputBound: true } frame ? frame.Ambient : input, projection));
        }
        if (function.Syntax == FunctionSyntax.InputFieldShorthand
            && TryGetFieldName(function.Parameters, out var inputField))
        {
            return new DelegatedFunction(input => NamedValueAccessor.Get(
                EvaluationRuntime.Frame is { IsInputBound: true } frame ? frame.Ambient : input, inputField));
        }

        if (function.Syntax == FunctionSyntax.RootFieldShorthand)
            return BuildRootFieldFunction(function);

        if (function.Syntax == FunctionSyntax.EnclosingRootFieldShorthand)
            return BuildEnclosingRootFieldFunction(function);

        return null;
    }

    private static IFunction BuildRootFieldFunction(Bindings.Function function)
    {
        if (!TryGetFieldName(function.Parameters, out var fieldName))
            throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);

        return new DelegatedFunction(_ => NamedValueAccessor.Get(EvaluationRuntime.Frame?.Scope.Resolve(FieldReferenceKind.ExpressionRoot, null, null), fieldName));
    }

    private static IFunction BuildEnclosingRootFieldFunction(Bindings.Function function)
    {
        if (!TryGetFieldName(function.Parameters, out var fieldName))
            throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);

        return new DelegatedFunction(_ => NamedValueAccessor.Get(EvaluationRuntime.Frame?.Scope.Resolve(FieldReferenceKind.EnclosingExpressionRoot, null, null), fieldName));
    }

    private static object? EvaluateNested(IFunction expression, object? input)
        => EvaluateNested(expression, input, input);

    private static object? EvaluateNested(IFunction expression, object? input, object? currentInput)
    {
        return EvaluationRuntime.EvaluateNested(expression, input, currentInput);
    }

    private IFunction InstantiateValueSpread(Type type, ConstructorInfo constructor, Bindings.Function function, IContext context)
    {
        var values = function.Arguments
            .Select(argument => new ValueArgumentEvaluator(
                BuildValueEvaluator(argument.Value, context),
                argument.IsSpread))
            .ToArray();
        var arguments = (Func<object?, object?[]>)(input => ValueArguments.Evaluate(values, input).ToArray());
        return constructor.Invoke([arguments]) as IFunction
            ?? throw new InvalidOperationException(
                $"Spread-aware constructor for '{type.FullName}' did not create a function.");
    }

    private Func<object?, object?> BuildValueEvaluator(IParameter parameter, IContext context, bool establishScope = false)
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
                // Value construction retains its supplying scope; apply invokes a new one.
                () => establishScope
                    ? EvaluateNested(chain, source.Invoke(input), input)
                    : chain.Evaluate(source.Invoke(input)));
        }

        var structured = BuildStructuredValueEvaluator(parameter, context);
        if (structured is not null)
            return structured;

        var provider = (Func<object?>)CreateParameter(parameter, typeof(object), context);
        return input => WithCurrentObject(context, input, provider);
    }

    private Func<object?, object?>? BuildStructuredValueEvaluator(IParameter parameter, IContext context)
    {
        if (parameter is ArrayParameter array)
        {
            var elements = array.Elements
                .Select(element => new ValueArgumentEvaluator(
                    BuildValueEvaluator(element.Value, context),
                    element.IsSpread))
                .ToArray();
            return input => ValueArguments.Evaluate(elements, input).ToArray();
        }

        if (parameter is PairParameter pair)
        {
            var key = BuildValueEvaluator(pair.Key, context);
            var value = BuildValueEvaluator(pair.Value, context);
            return input => new Expressif.Values.Pair(key.Invoke(input), value.Invoke(input));
        }

        if (parameter is GroupingParameter grouping)
        {
            var entries = grouping.Entries
                .Select(entry => new
                {
                    Key = BuildValueEvaluator(entry.Key, context),
                    Value = BuildValueEvaluator(entry.Value, context),
                })
                .ToArray();
            return input => new Values.Grouping(entries.Select(entry =>
                new Pair(entry.Key.Invoke(input), entry.Value.Invoke(input))));
        }

        if (parameter is DictionaryParameter dictionary)
        {
            var entries = dictionary.Entries
                .Select(entry => new
                {
                    Key = BuildValueEvaluator(entry.Key, context),
                    Value = BuildValueEvaluator(entry.Value, context),
                })
                .ToArray();
            return input => new Values.Dictionary(entries.Select(entry =>
                new Pair(entry.Key.Invoke(input), entry.Value.Invoke(input))));
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

        return null;
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

    private Func<object?, object?> BuildOpenExpressionRecordEvaluator(OpenExpressionParameter open, IContext context)
    {
        if (open.Expression.InputBinding is { } binding)
            return BuildInputBoundFunction(binding, context).Evaluate;

        if (TryBuildSingleTokenEvaluator(open, out var evaluator))
            return evaluator;

        try
        {
            var functions = open.Expression.Members.Select(member => InstantiateOrWrapAggregation(member, context)).ToArray();
            var chain = new ChainFunction(functions);
            return input => EvaluateNested(chain, input);
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
        if (parameter is OpenExpressionParameter open && open.Expression.Members.Count() == 1)
        {
            var call = open.Expression.Members.Single();
            if (accumulatorRegistry.TryResolve(call.Name, out var accumulatorType)
                && TryBuildAccumulatorConstructor(call, accumulatorType, context, out var create))
                return create;
            if (call.Arguments.Length != 0)
                throw new MissingOrUnexpectedParametersFunctionException(call.Name, call.Parameters.Length);
        }
        var nameProvider = BuildAccumulatorNameProvider(parameter, context);
        return () => accumulatorRegistry.Create(nameProvider.Invoke());
    }

    private bool TryInstantiateWithTransformationProvider(
        Type type,
        Bindings.Function function,
        IContext context,
        [NotNullWhen(true)] out IFunction? transformation)
    {
        transformation = null;

        var ctor = type.GetConstructors()
                       .FirstOrDefault(x => x.GetParameters().Length >= 1
                                         && x.GetParameters()[0].ParameterType == typeof(Func<IFunction>)
                                         && x.GetParameters().Skip(1).All(parameter => parameter.ParameterType.IsGenericType
                                             && parameter.ParameterType.GetGenericTypeDefinition() == typeof(Func<>)));
        if (ctor is null)
            return false;

        if (function.Parameters.Length != ctor.GetParameters().Length)
            throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);

        var bound = ParameterArgumentBinder.Bind(type, function.Arguments).Parameters;

        if (!TryGetOpenExpression(bound[0], out var openExpression))
        {
            throw new ArgumentException(
                $"The function named '{function.Name}' expects a parameter of type '{nameof(OpenExpressionParameter)}' but received '{bound[0].GetType().Name}'.",
                nameof(function));
        }

        var arguments = new List<object> { BuildTransformationProvider(openExpression, context) };
        foreach (var parameter in ctor.GetParameters().Skip(1))
        {
            var scalarType = parameter.ParameterType.GetGenericArguments()[0];
            arguments.Add(CreateParameter(bound[parameter.Position], scalarType, context));
        }

        transformation = (IFunction)ctor.Invoke(arguments.ToArray());
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
        if (TryGetOpenExpression(parameter, out var openExpression))
            return () => BuildBooleanPredicate(openExpression.Expression, context);

        return parameter switch
        {
            PredicationParameter predication => () => predicationFactory.Instantiate(predication.Predication, context),
            _ => throw new ArgumentException(
                    $"The function named '{functionName}' expects a parameter of type '{nameof(PredicationParameter)}' or '{nameof(OpenExpressionParameter)}' but received '{parameter.GetType().Name}'.",
                    nameof(parameter))
        };
    }

    private IPredicate BuildBooleanPredicate(OpenExpression expression, IContext context)
    {
        var function = BuildOpenExpression(expression, context);
        return new BooleanFunctionPredicate(function, preserveCurrentInput: function is IPredicate);
    }

    private static bool TryGetOpenExpression(
        IParameter parameter,
        [NotNullWhen(true)] out OpenExpressionParameter? expression)
        => ExpressionShapeNormalizer.TryGetOpenExpression(parameter, out expression);

    protected override Delegate CreateInputExpression(InputExpressionParameter input, Type type, IContext context)
    {
        var source = BuildValueEvaluator(input.Expression.Parameter, context);
        var chain = new ChainFunction(input.Expression.Members
            .Select(member => InstantiateOrWrapAggregation(member, context))
            .ToArray());
        return CreateFunctionCast(() =>
        {
            var value = source.Invoke(EvaluationRuntime.Frame?.Current);
            if (IsExplicitlyRooted(input.Expression.Parameter))
                return chain.Evaluate(value);

            using var scope = EvaluationRuntime.Derive(value);
            return chain.Evaluate(value);
        }, type);
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

    private sealed class AccumulatorFunction(Func<IAccumulator> accumulatorProvider) : IFunction
    {
        public object? Evaluate(object? value)
        {
            if (value is not IEnumerable enumerable || value is string)
                return null;

            var accumulator = accumulatorProvider.Invoke();
            accumulator.Initialize();
            foreach (var item in enumerable)
                accumulator.Accumulate(item);
            return accumulator.GetValue();
        }
    }
}
