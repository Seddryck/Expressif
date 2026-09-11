using Expressif.Bindings;
using Expressif.Semantics;
using Expressif.Functions.Array;
using Expressif.Accumulators;
using Expressif.Accumulators.Introspection;
using Expressif.Predicates;
using Expressif.Values;
using Expressif.Functions.Coercions;
using RecordEntryEvaluator = Expressif.Functions.Record.RecordEntryEvaluator;
using RecordFunction = Expressif.Functions.Record.Record;
using ArrayFunction = Expressif.Functions.Array.Array;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using LinqExpression = System.Linq.Expressions.Expression;

namespace Expressif.Functions;

public partial class FunctionFactory : BaseExpressionFactory
{
    private static readonly PredicateTypeMapper PredicateTypeMapper = new();
    private static readonly HashSet<string> ImplicitFoldAccumulators = new(
        new AccumulatorIntrospector().Locate().Select(x => x.Name),
        StringComparer.OrdinalIgnoreCase
    );
    private static readonly CoercionRegistry CoercionRegistry = new();

    public FunctionFactory()
        : base(new FunctionTypeMapper()) { }

    public FunctionFactory(BaseTypeMapper typeMapper)
        : base(typeMapper) { }

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
            var evaluator = BuildOpenExpressionRecordEvaluator(open, context);
            return CreateFunctionCast(
                () => evaluator.Invoke(EvaluationRuntime.Frame is { IsInputBound: true } frame
                    ? frame.Current : ArgumentScope.Root(context.CurrentObject.Value, EvaluationRuntime.Frame?.Current)),
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
        if (expression is InputBoundExpression binding)
            return BuildInputBoundFunction(binding, context);

        var members = expression.Members.ToArray();
        var functions = members
            .Select(member => InstantiateOrWrapAggregation(member, context))
            .ToList();

        if (FunctionConstruction.IsPredicatePipeline(functions, function => function is IPredicate))
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
            using var scope = EvaluationRuntime.Derive(source);
            return functions.Aggregate(source, (current, function) => function.Evaluate(current));
        });
    }

    private Func<object?, object?> BuildSourceEvaluator(IParameter parameter, IContext context)
    {
        if (FunctionConstruction.UsesInputValueEvaluator(parameter))
            return BuildValueEvaluator(parameter, context);

        var provider = CreateParameter(parameter, typeof(object), context);
        return _ => provider.DynamicInvoke();
    }

    private IFunction InstantiateOrWrapAggregation(Bindings.Function function, IContext context)
    {
        if (function.Syntax == FunctionSyntax.InputBindingStage
            && function.Parameters is [OpenExpressionParameter { Expression: InputBoundExpression binding }])
            return BuildInputBoundFunction(binding, context);
        var name = function.Name.ToKebabCase();
        var construction = FunctionConstruction.Classify(name);

        if (BuildReferenceFunction(function) is { } reference)
            return reference;

        if (function.Arguments.Any(x => x.Name is not null)
            && name is "record" or "coalesce")
            throw new UnknownParameterNameException(name, function.Arguments.First(x => x.Name is not null).Name!);

        if (function.Arguments.Any(x => x.Name is not null) && name == "adjacent")
            function = new Bindings.Function(name, ParameterArgumentBinder.Bind(TypeMapper.Execute(name), function.Arguments).Parameters);

        if (function.Arguments.Any(x => x.Name is not null) && name == "generate")
            function = new Bindings.Function(name, ParameterArgumentBinder.Bind(TypeMapper.Execute(name), function.Arguments).Parameters);

        var specialFunction = TryBuildSpecialFunction(construction, function, context);
        if (specialFunction is not null)
            return specialFunction;

        if (construction == FunctionConstructionKind.Extend)
        {
            if (function.Arguments is not [var extension]
                || (extension.Name is not null && !extension.Name.Equals("value", StringComparison.OrdinalIgnoreCase)))
                throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);
            var evaluator = BuildValueEvaluator(extension.Value, context);
            return new Tuple.Extend(value => evaluator.Invoke(value));
        }
        if (construction == FunctionConstructionKind.Pair)
        {
            var bound = ParameterArgumentBinder.Bind(TypeMapper.Execute(name), function.Arguments).Parameters;
            if (bound is not [var key, var value])
                throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);
            return new Pair.Pair(BuildValueEvaluator(key, context), BuildValueEvaluator(value, context));
        }
        if (construction == FunctionConstructionKind.Put)
            return BuildPutFunction(name, function, context);

        if (construction == FunctionConstructionKind.PutPath)
            return BuildPutPathFunction(name, function, context);

        if (construction == FunctionConstructionKind.RenameFields)
        {
            var bound = ParameterArgumentBinder.Bind(typeof(Record.RenameFields), function.Arguments).Parameters;
            var transform = bound[0] is OpenExpressionParameter open
                ? BuildOpenExpression(open.Expression, context)
                : new DelegatedFunction(BuildValueEvaluator(bound[0], context));
            var filter = bound.Length == 2 ? BuildPredicateProvider(bound[1], context, name).Invoke() : null;
            return new Record.RenameFields(() => transform, filter is null ? null : () => filter);
        }

        if (construction == FunctionConstructionKind.Key)
            return new Array.Key(BuildGroupingExpressionEvaluators(function, context));

        if (construction == FunctionConstructionKind.DrillDown)
        {
            var expressions = BuildGroupingExpressionEvaluators(function, context)
                .Select(evaluator => new DelegatedFunction(evaluator))
                .Select(expression => (Func<object?, object?>)(value => EvaluateNested(expression, value)));
            return new Grouping.DrillDown(expressions);
        }

        if (construction == FunctionConstructionKind.DrillUp)
        {
            var bound = ParameterArgumentBinder.Bind(TypeMapper.Execute(name), function.Arguments).Parameters;
            if (bound is not [var expression])
                throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);
            var evaluator = new DelegatedFunction(BuildValueEvaluator(expression, context));
            return new Grouping.DrillUp(key => EvaluateNested(evaluator, key));
        }

        if (construction == FunctionConstructionKind.GroupBy)
            return new Array.GroupBy(BuildGroupingExpressionEvaluators(function, context));

        if (construction == FunctionConstructionKind.Pick)
        {
            var positions = function.Arguments
                .Select(argument => (Func<int>)CreateParameter(argument.Value, typeof(int), context))
                .ToArray();
            return new Tuple.Pick(() => positions.Select(position => position.Invoke()).ToArray());
        }

        if (construction == FunctionConstructionKind.Apply)
        {
            if (function.Parameters.Length != 1)
                throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);

            var operation = TryGetOpenExpression(function.Parameters[0], out var open)
                ? BuildOpenExpression(open.Expression, context)
                : new DelegatedFunction(BuildValueEvaluator(function.Parameters[0], context, establishScope: true));
            IFunction expression = function.Parameters[0] is InputExpressionParameter
                ? operation
                : new LexicallyBoundContextFunction(operation);
            return new Flow.Apply(() => expression);
        }

        if (construction == FunctionConstructionKind.TransformWith)
            return BuildTransformWithFunction(function, context);

        if (construction == FunctionConstructionKind.TransformAs)
            return BuildTransformAsFunction(function, context);

        if (IsImplicitFoldAccumulator(function))
            return new Fold(() => name);

        if (!TypeMapper.TryExecute(function.Name, out var type))
        {
            if (PredicateTypeMapper.TryExecute(function.Name, out _))
                return new PredicationFactory().Instantiate(new SinglePredication(function), context);

            throw new NotImplementedFunctionException(function.Name);
        }

        if (typeof(IValueSpreadAware).IsAssignableFrom(type))
            return InstantiateValueSpreadAware(type, function, context);

        if (TryInstantiateWithAccumulatorProvider(type, function, context, out var aggregation))
            return aggregation;

        if (TryInstantiateWithTransformationProvider(type, function, context, out var transformation))
            return transformation;

        if (TryInstantiateWithPredicateProvider(type, function, context, out var filtering))
            return filtering;

        return Instantiate<IFunction>(type, function.Arguments, context);
    }

    internal static bool IsImplicitFoldAccumulator(Bindings.Function function)
        => ImplicitFoldAccumulators.Contains(function.Name.ToKebabCase()) && function.Parameters.Length == 0;

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

    private IFunction? TryBuildSpecialFunction(FunctionConstructionKind construction, Bindings.Function function, IContext context)
        => construction switch
        {
            FunctionConstructionKind.TupleBind => BuildTupleBind(function, context),
            FunctionConstructionKind.Record => BuildRecordFunction(function, context),
            FunctionConstructionKind.With => BuildWithFunction(function, context),
            FunctionConstructionKind.Conditional => BuildConditionalFunction(function, context),
            FunctionConstructionKind.ControlFlow => BuildControlFlowFunction(function, context),
            FunctionConstructionKind.Coalesce => BuildCoalesceFunction(function, context),
            FunctionConstructionKind.Coerce => BuildCoerceFunction(function),
            FunctionConstructionKind.Adjacent => BuildAdjacentFunction(function, context),
            FunctionConstructionKind.ChunkWhile => BuildChunkWhileFunction(function, context),
            FunctionConstructionKind.Generate => BuildGenerateFunction(function, context),
            FunctionConstructionKind.Closest => new Fold(BuildClosestProvider(function, context)),
            FunctionConstructionKind.Implode => BuildImplodeFunction(function, context),
            FunctionConstructionKind.MapOver => BuildDirectionalMap(function, context, mapOver: true),
            FunctionConstructionKind.MapWith => BuildDirectionalMap(function, context, mapOver: false),
            FunctionConstructionKind.Reduce => BuildReduceFunction(function, context),
            _ => null,
        };

    private Func<IAccumulator> BuildClosestProvider(Bindings.Function function, IContext context)
    {
        var bound = ParameterArgumentBinder.Bind(typeof(ClosestAccumulator), function.Arguments).Parameters;
        var target = BuildValueEvaluator(bound[0], context);
        return () => new ClosestAccumulator(() => target.Invoke(EvaluationRuntime.Frame?.Current));
    }

    private IFunction BuildImplodeFunction(Bindings.Function function, IContext context)
    {
        var bound = ParameterArgumentBinder.Bind(typeof(ImplodeAccumulator), function.Arguments).Parameters;
        if (bound.Length == 0)
            return new Fold(() => new ImplodeAccumulator());

        var separator = (Func<string>)CreateParameter(bound[0], typeof(string), context);
        return new Fold(() => new ImplodeAccumulator(separator));
    }

    private IFunction BuildReduceFunction(Bindings.Function function, IContext context)
    {
        var bound = ParameterArgumentBinder.Bind(typeof(ReduceAccumulator), function.Arguments).Parameters;
        if (!TryGetOpenExpression(bound[0], out var operation))
        {
            throw new ArgumentException(
                $"The accumulator named '{function.Name}' expects parameter 'operation' to be an open expression.",
                nameof(function));
        }

        var operationProvider = BuildReduceOperationProvider(operation, context);
        if (bound.Length == 1)
            return new Fold(() => new ReduceAccumulator(operationProvider));

        var initial = BuildValueEvaluator(bound[1], context);
        return new Fold(() => new ReduceAccumulator(operationProvider, () => initial.Invoke(EvaluationRuntime.Frame?.Current)));
    }

    private Func<IFunction> BuildReduceOperationProvider(OpenExpressionParameter operation, IContext context)
    {
        if (operation.Expression is InputBoundExpression binding)
            return () => BuildInputBoundFunction(binding, context);

        var members = operation.Expression.Members.ToArray();
        if (members is [var first, ..]
            && first.Arguments is [
            { Name: null, Value: TupleProjectionParameter { Index: 0, FromEnd: false } },
                .. var remaining])
        {
            members[0] = Bindings.Function.FromArguments(first.Name, remaining);
            members = [
                new Bindings.Function("tuple-at", [new LiteralParameter("0")]),
                .. members,
            ];
        }

        var normalized = new OpenExpression(members);
        return () => BuildPipeline(normalized, context);
    }

    private IFunction BuildPutFunction(string name, Bindings.Function function, IContext context)
    {
        if (function.Arguments.Length == 0 || function.Arguments.Any(argument => argument.Name is null || argument.IsSpread))
            throw new BindingException($"Function '{name}' expects one or more named assignments.");

        var names = new HashSet<string>(StringComparer.Ordinal);
        var duplicate = function.Arguments.FirstOrDefault(argument => !names.Add(argument.Name!));
        if (duplicate is not null)
            throw new BindingException($"Duplicate assignment '{duplicate.Name}' in {name}(...).");

        var assignments = function.Arguments
            .Select(argument => new Record.RecordAssignmentEvaluator(
                argument.Name!,
                BuildValueEvaluator(argument.Value, context)))
            .ToArray();
        return name switch
        {
            "put" => new Record.Put(() => assignments),
            "put-present" => new Record.PutPresent(() => assignments),
            "put-absent" => new Record.PutAbsent(() => assignments),
            _ => throw new NotImplementedFunctionException(name),
        };
    }

    private IFunction BuildPutPathFunction(string name, Bindings.Function function, IContext context)
    {
        var bound = ParameterArgumentBinder.Bind(TypeMapper.Execute(name), function.Arguments).Parameters;
        if (bound is not [var path, var value])
            throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);

        var pathEvaluator = BuildValueEvaluator(path, context);
        var valueEvaluator = BuildValueEvaluator(value, context);
        return name switch
        {
            "put-path" => new Record.PutPath(pathEvaluator, valueEvaluator),
            "put-present-path" => new Record.PutPresentPath(pathEvaluator, valueEvaluator),
            "put-absent-path" => new Record.PutAbsentPath(pathEvaluator, valueEvaluator),
            _ => throw new NotImplementedFunctionException(name),
        };
    }

    private IEnumerable<Func<object?, object?>> BuildGroupingExpressionEvaluators(
        Bindings.Function function,
        IContext context)
    {
        if (function.Arguments.Length == 0 || function.Arguments.Any(argument => argument.Name is not null || argument.IsSpread))
            throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);

        return function.Arguments.Select(argument => BuildValueEvaluator(argument.Value, context)).ToArray();
    }

    private IFunction BuildTransformWithFunction(Bindings.Function function, IContext context)
    {
        if (function.Parameters.Length < 2 || !TryGetOpenExpression(function.Parameters[0], out var open))
            throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);

        var operation = BuildOpenExpression(open.Expression, context);
        var expressions = function.Parameters.Skip(1).Select(parameter => BuildValueEvaluator(parameter, context));
        return new Flow.TransformWith(
            () => new LexicallyBoundContextFunction(operation),
            expressions);
    }

    private IFunction BuildTransformAsFunction(Bindings.Function function, IContext context)
    {
        if (function.Arguments.Length < 2
            || function.Arguments[0].Name is not null
            || !TryGetOpenExpression(function.Arguments[0].Value, out var open)
            || function.Arguments.Skip(1).Any(argument => argument.Name is null || argument.IsSpread))
        {
            throw new BindingException(
                $"The function named '{function.Name}' expects one positional open expression followed by one or more named expressions.");
        }

        var names = new HashSet<string>(StringComparer.Ordinal);
        var duplicate = function.Arguments.Skip(1).FirstOrDefault(argument => !names.Add(argument.Name!));
        if (duplicate is not null)
            throw new BindingException($"Duplicate named expression '{duplicate.Name}' in {function.Name}(...).");

        var operation = BuildOpenExpression(open.Expression, context);
        var expressions = function.Arguments.Skip(1).Select(argument => new Flow.NamedExpressionEvaluator(
            argument.Name!,
            BuildValueEvaluator(argument.Value, context)));
        return new Flow.TransformAs(
            () => new LexicallyBoundContextFunction(operation),
            expressions);
    }

    private static IFunction BuildCoerceFunction(Bindings.Function function)
    {
        if (function.Parameters.All(parameter => parameter is PositionalCoercionParameter))
        {
            return new Special.Coerce(function.Parameters
                .Cast<PositionalCoercionParameter>()
                .Select(parameter => parameter.TargetType)
                .ToArray());
        }

        var mappings = function.Parameters.Select(parameter => parameter switch
        {
            FieldCoercionParameter field => new Special.CoercionMapping(
                new Special.FieldCoercionSelector(field.Field),
                field.TargetType),
            TupleCoercionParameter tuple => new Special.CoercionMapping(
                new Special.TupleCoercionSelector(tuple.Position),
                tuple.TargetType),
            _ => throw new InvalidOperationException(
                $"Unsupported bound coercion specification '{parameter.GetType().Name}'."),
        }).ToArray();
        return new Special.Coerce(mappings);
    }

    private IFunction InstantiateValueSpreadAware(Type type, Bindings.Function function, IContext context)
    {
        var values = function.Arguments
            .Select(argument => new ValueArgumentEvaluator(
                BuildValueEvaluator(argument.Value, context),
                argument.IsSpread))
            .ToArray();
        var arguments = (Func<ValueArgumentEvaluator[]>)(() => values);
        return Activator.CreateInstance(type, arguments) as IFunction
            ?? throw new InvalidOperationException(
                $"Value-spread-aware type '{type.FullName}' must expose a constructor accepting Func<ValueArgumentEvaluator[]>.");
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
            var function = new ArrayFunction(() => elements);
            return function.Evaluate;
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
                new PairValue(entry.Key.Invoke(input), entry.Value.Invoke(input))));
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
                new PairValue(entry.Key.Invoke(input), entry.Value.Invoke(input))));
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

    private IFunction BuildConditionalFunction(Bindings.Function function, IContext context)
    {
        var backward = function.Syntax == FunctionSyntax.ConditionalBackward;
        return new Flow.Conditional(
            BuildControlFlowEvaluator(function.Parameters[backward ? 0 : 1], context),
            BuildControlFlowEvaluator(function.Parameters[backward ? 1 : 0], context),
            backward);
    }

    private IFunction BuildControlFlowFunction(Bindings.Function function, IContext context)
    {
        var branches = function.Parameters.Cast<ControlFlowBranchParameter>()
            .Select(branch => new Flow.ControlFlowBranch(
                BuildControlFlowEvaluator(branch.Expression, context),
                branch.Predicate is null ? null : BuildControlFlowEvaluator(branch.Predicate, context)))
            .ToArray();
        return function.Name.Equals("try", StringComparison.OrdinalIgnoreCase)
            ? new Flow.Try(branches)
            : new Flow.Switch(branches);
    }

    private Func<object?, object?> BuildControlFlowEvaluator(IParameter parameter, IContext context)
    {
        if (parameter is OpenExpressionParameter open)
        {
            var expression = BuildOpenExpression(open.Expression, context);
            return expression.Evaluate;
        }
        if (parameter is InputExpressionParameter closed)
        {
            var source = BuildControlFlowEvaluator(closed.Expression.Parameter, context);
            var expression = BuildOpenExpression(new OpenExpression(closed.Expression.Members), context);
            return input => expression.Evaluate(source.Invoke(input));
        }
        return BuildValueEvaluator(parameter, context);
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
        if (LegacyTupleBindingRules.IsCandidate("adjacent", open.Expression) && TryBuildBinaryCallable(members[0].Name, context, out var callable))
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

        return new Adjacent(() => new LexicallyBoundContextFunction(operation));
    }

    private IFunction BuildChunkWhileFunction(Bindings.Function function, IContext context)
    {
        if (function.Parameters.Length != 1 || !TryGetOpenExpression(function.Parameters[0], out var open))
            throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);

        var members = open.Expression.Members.ToArray();
        IFunction operation;
        if (LegacyTupleBindingRules.IsCandidate("chunk-while", open.Expression)
            && TryBuildBinaryCallable(members[0].Name, context, out var callable))
        {
            var functions = new List<IFunction>
            {
                InstantiateOrWrapAggregation(new Bindings.Function("tuple-at", [new LiteralParameter("1")]), context),
                callable,
            };
            if (members.Length > 1)
                functions.Add(BuildPipeline(new OpenExpression(members.Skip(1)), context));
            operation = new ChainFunction(functions);
        }
        else
        {
            operation = BuildOpenExpression(open.Expression, context);
        }

        return new ChunkWhile(() => new LexicallyBoundContextFunction(operation));
    }

    private IFunction BuildGenerateFunction(Bindings.Function function, IContext context)
    {
        if (function.Parameters.Length is < 2 or > 3)
            throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);

        var condition = BuildPredicateProvider(function.Parameters[0], context, function.Name);
        if (!TryGetOpenExpression(function.Parameters[1], out var next))
        {
            throw new ArgumentException(
                $"The function named '{function.Name}' expects parameter 'next' to be an open expression.",
                nameof(function));
        }

        Func<IFunction>? result = null;
        if (function.Parameters.Length == 3)
        {
            if (!TryGetOpenExpression(function.Parameters[2], out var projection))
            {
                throw new ArgumentException(
                    $"The function named '{function.Name}' expects parameter 'result' to be an open expression.",
                    nameof(function));
            }
            result = BuildTransformationProvider(projection, context);
        }

        return new Generate(condition, BuildTransformationProvider(next, context), result);
    }

    private IFunction BuildDirectionalMap(Bindings.Function function, IContext context, bool mapOver)
    {
        var type = mapOver ? typeof(MapOver) : typeof(MapWith);
        var binding = ParameterArgumentBinder.Bind(type, function.Arguments);
        if (binding.Parameters is not [OpenExpressionParameter expression, var values])
            throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);

        var valuesEvaluator = BuildValueEvaluator(values, context);
        Func<System.Collections.IEnumerable?> valuesProvider = () =>
            valuesEvaluator.Invoke(EvaluationRuntime.Frame?.Current) is { } evaluated
                && AggregationEnumerable.TryGetEnumerable(evaluated, out var enumerable)
                    ? enumerable
                    : null;
        Func<IFunction> operationProvider = () => BuildDirectionalMapOperation(expression, context, mapOver);

        return mapOver
            ? new MapOver(operationProvider, valuesProvider)
            : new MapWith(operationProvider, valuesProvider);
    }

    private IFunction BuildDirectionalMapOperation(OpenExpressionParameter expression, IContext context, bool mapOver)
    {
        var members = expression.Expression.Members.ToArray();
        if (TupleBindingOperations.LeadingLength(expression.Expression) > 0)
            return BuildExplicitDirectionalMapOperation(expression.Expression, context, mapOver);
        var isBareCallable = LegacyTupleBindingRules.IsCandidate(mapOver ? "map-over" : "map-with", expression.Expression);
        if (isBareCallable)
        {
            var name = members[0].Name;
            return new DelegatedFunction(value =>
            {
                var invocation = GetDirectionalMapInput(value);
                var inputs = DirectionalScope<object?>.Create(mapOver, invocation.Outer, invocation.Item);
                var arguments = mapOver
                    ? GetMapOverArguments(invocation.Item)
                    : [new LiteralParameter(invocation.Outer)];
                var callable = InstantiateOrWrapAggregation(new Bindings.Function(name, arguments), context);
                using var scope = EvaluationRuntime.Derive(inputs.Arguments);
                return callable.Evaluate(inputs.Input);
            });
        }

        var normalized = mapOver && expression.Expression is not InputBoundExpression
            ? NormalizeMapOverProjections(expression.Expression) : expression.Expression;
        var operation = BuildOpenExpression(normalized, context);
        return new DelegatedFunction(value =>
        {
            var invocation = GetDirectionalMapInput(value);
            var inputs = DirectionalScope<object?>.Create(mapOver, invocation.Outer, invocation.Item);
            if (operation is InputBoundFunction)
                return operation.Evaluate(inputs.Input);
            using var scope = EvaluationRuntime.Derive(inputs.Arguments);
            return operation.Evaluate(inputs.Input);
        });
    }

    private IFunction BuildExplicitDirectionalMapOperation(OpenExpression expression, IContext context, bool mapOver)
    {
        var explicitOperation = BuildOpenExpression(mapOver
            ? NormalizeMapOverProjections(expression) : expression, context);
        return new DelegatedFunction(value =>
        {
            var invocation = GetDirectionalMapInput(value);
            var inputs = DirectionalScope<object?>.Create(mapOver, invocation.Outer, invocation.Item);
            var arguments = mapOver && invocation.Item is Values.Tuple tuple
                ? tuple.ToArray() : new[] { invocation.Item };
            var prepared = new Values.Tuple([invocation.Outer, .. arguments]);
            using var scope = EvaluationRuntime.Derive(inputs.Arguments);
            return explicitOperation.Evaluate(prepared);
        });
    }

    private static IParameter[] GetMapOverArguments(object? item)
        => item is Values.Tuple tuple
            ? tuple.Select(value => (IParameter)new LiteralParameter(value)).ToArray()
            : [new LiteralParameter(item)];

    private static DirectionalMapInput GetDirectionalMapInput(object? value)
        => value as DirectionalMapInput
            ?? throw new InvalidOperationException("Directional map operations require a directional map input.");

    private static OpenExpression NormalizeMapOverProjections(OpenExpression expression)
        => new(expression.Members.Select(member =>
        {
            var normalized = Bindings.Function.FromArguments(member.Name,
                member.Arguments.Select(argument => argument with { Value = NormalizeMapOverProjection(argument.Value) }).ToArray(), member.Syntax);
            normalized.SourceSpan = member.SourceSpan;
            return normalized;
        }));

    private static IParameter NormalizeMapOverProjection(IParameter parameter)
        => parameter switch
        {
            TupleProjectionParameter { Index: > 0, FromEnd: false } projection
                => projection with { Index = projection.Index - 1 },
            ArrayParameter array => new ArrayParameter(array.Elements.Select(element
                => element with { Value = NormalizeMapOverProjection(element.Value) }).ToArray()),
            TupleParameter tuple => new TupleParameter(tuple.Elements.Select(element
                => element with { Value = NormalizeMapOverProjection(element.Value) }).ToArray()),
            _ => parameter,
        };

    private bool TryBuildBinaryCallable(string name, IContext context, [NotNullWhen(true)] out IFunction? callable)
    {
        var parameterized = new Bindings.Function(name, [new TupleProjectionParameter(0)]);
        if (TypeMapper.TryExecute(name, out var functionType))
        {
            if (!LegacyTupleBindingRules.HasBinarySignature(functionType))
            {
                callable = null;
                return false;
            }
            callable = InstantiateOrWrapAggregation(parameterized, context);
            return true;
        }

        if (PredicateTypeMapper.TryExecute(name, out var predicateType))
        {
            if (!LegacyTupleBindingRules.HasBinarySignature(predicateType))
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

    private sealed class LexicallyBoundContextFunction(IFunction expression) : IFunction
    {
        public object? Evaluate(object? value)
        {
            return EvaluationRuntime.EvaluateNested(expression, value);
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
                case RecordSpreadEntry spread:
                    evaluators.Add(RecordEntryEvaluator.Spread(BuildRecordNamedValueEvaluator(spread.Value, context)));
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
        return new Expressif.Functions.Record.With(() => projections, body,
            definition.Body is OpenExpressionParameter { Expression: InputBoundExpression });
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
        if (open.Expression is InputBoundExpression binding)
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
        return new BooleanFunctionPredicate(function, preserveCurrentInput: function is IPredicate);
    }

    private static bool TryGetOpenExpression(
        IParameter parameter,
        [NotNullWhen(true)] out OpenExpressionParameter? expression)
    {
        expression = parameter switch
        {
            OpenExpressionParameter open => open,
            ScopedTupleProjectionParameter projection => new OpenExpressionParameter(new OpenExpression([
                new Bindings.Function(
                    "tuple-at",
                    [projection],
                    FunctionSyntax.ScopedTupleProjectionShorthand),
            ])),
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
