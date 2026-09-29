using Expressif.Bindings;
using Expressif.Semantics;
using Expressif.Values;
using Expressif.Discovery;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ValueRecord = Expressif.Values.RecordValue;

namespace Expressif.Functions;

public abstract class BaseExpressionFactory
{
    protected IImplementationRegistry Registry { get; }
    private IValueConverter Converter { get; }

    protected BaseExpressionFactory(IImplementationRegistry registry, ITypesProbe probe)
        : this(registry, ProbeService.Create<IValueConverter>(probe)) { }

    protected BaseExpressionFactory(IImplementationRegistry registry, IValueConverter converter)
        => (Registry, Converter) = (registry, converter);

    protected internal T Instantiate<T>(string functionName, IParameter[] parameters, IContext context)
        => Instantiate<T>(Registry.Resolve(functionName), parameters, context);

    protected T Instantiate<T>(Type type, IParameter[] parameters, IContext context)
    {
        var ctor = GetMatchingConstructor(type, parameters.Length);

        var zip = ctor.GetParameters().Zip(parameters, (x, y) => new { x.ParameterType, Value = y });
        var typedFunctionParameters = new List<Delegate>();

        foreach (var param in zip)
        {
            //If the parameter of the contextReference is a Func<>
            if (param.ParameterType.IsGenericType && param.ParameterType.GetGenericTypeDefinition() == typeof(Func<>))
            {
                var scalarType = param.ParameterType.GenericTypeArguments[0];
                var @delegate = CreateParameter(param.Value, scalarType, context);
                typedFunctionParameters.Add(@delegate);
            }
            else
            {
                typedFunctionParameters.Add(() => param.Value);
            }
        }

        return (T)ctor.Invoke(typedFunctionParameters.ToArray());
    }

    protected T Instantiate<T>(Type type, FunctionArgument[] arguments, IContext context)
    {
        var binding = ParameterArgumentBinder.Bind(type, arguments);
        return Instantiate<T>(binding.Constructor, binding.Parameters, context);
    }

    private T Instantiate<T>(ConstructorInfo ctor, IParameter[] parameters, IContext context)
    {
        var zip = ctor.GetParameters().Zip(parameters, (x, y) => new { x.ParameterType, Value = y });
        var typedFunctionParameters = new List<Delegate>();
        foreach (var param in zip)
        {
            if (param.ParameterType.IsGenericType && param.ParameterType.GetGenericTypeDefinition() == typeof(Func<>))
            {
                var scalarType = param.ParameterType.GenericTypeArguments[0];
                typedFunctionParameters.Add(CreateParameter(param.Value, scalarType, context));
            }
            else
            {
                typedFunctionParameters.Add(() => param.Value);
            }
        }
        return (T)ctor.Invoke(typedFunctionParameters.ToArray());
    }

    protected internal virtual ConstructorInfo GetMatchingConstructor(Type type, int paramCount)
        => type.GetConstructors().SingleOrDefault(x => x.GetParameters().Length == paramCount)
            ?? throw new MissingOrUnexpectedParametersFunctionException(type.Name, paramCount);

    protected virtual Delegate CreateParameter(IParameter parameter, Type scalarType, IContext context)
    {
        return parameter switch
        {
            ArrayParameter array => CreateFunctionCast(() => BuildArray(array, context), scalarType),
            TupleParameter tuple => CreateFunctionCast(() => BuildTuple(tuple, context), scalarType),
            VectorParameter vector => CreateFunctionCast(() => BuildVector(vector, context), scalarType),
            RecordLiteralParameter record => CreateFunctionCast(() => BuildRecord(record, context), scalarType),
            InputExpressionParameter input => CreateDelegateCast(CreateInputExpression(input, scalarType, context), scalarType),
            IntervalParameter interval => CreateCast(BuildInterval(interval.Value), scalarType),
            QuotedLiteralParameter quoted => CreateCast(quoted.Value, scalarType),
            LiteralParameter { Value: null } => CreateFunctionCast(() => null, scalarType),
            LiteralParameter literal => CreateCast(literal.Value, scalarType),
            ObjectIndexParameter index => CreateFunctionCast(() => GetAmbientValue(context, index.Index), scalarType),
            ScopedTupleProjectionParameter projection => CreateFunctionCast(() => ResolveScopedTupleProjection(projection), scalarType),
            TupleProjectionParameter projection => CreateFunctionCast(() => ResolveTupleProjection(GetCurrent(context), projection), scalarType),
            ObjectPropertyParameter prop => CreateFunctionCast(() => GetAmbientValue(context, prop.Name), scalarType),
            EnclosingObjectPropertyParameter prop => CreateFunctionCast(
                () => NamedValueAccessor.Get(EvaluationRuntime.Frame?.Scope.Resolve(FieldReferenceKind.EnclosingExpressionRoot, null, null), prop.Name),
                scalarType),
            VariableParameter variable => CreateFunctionCast(() => GetVariable(context, variable.Name), scalarType),
            IncomingValueParameter => CreateFunctionCast(() => GetCurrent(context), scalarType),
            ContextParameter contextReference => CreateFunctionCast(() => contextReference.Function.Invoke(context), scalarType),
            _ => throw new BindingException($"Cannot handle the parameter type '{parameter.GetType().Name}'.")
        };
    }

    private object?[] BuildArray(ArrayParameter array, IContext currentContext)
    {
        var values = new List<object?>();
        foreach (var element in array.Elements)
        {
            var elementFactory = CreateParameter(element.Value, typeof(object), currentContext);
            var evaluated = elementFactory.DynamicInvoke();
            if (element.IsSpread)
                Functions.Array.SpreadValues.Append(evaluated, values);
            else
                values.Add(evaluated);
        }

        return values.ToArray();
    }

    private Expressif.Values.Tuple BuildTuple(TupleParameter tuple, IContext currentContext)
    {
        var values = new List<object?>();
        foreach (var element in tuple.Elements)
        {
            var elementFactory = (Func<object?>)CreateParameter(element.Value, typeof(object), currentContext);
            var evaluated = elementFactory.Invoke();
            if (element.IsSpread)
            {
                if (evaluated is null)
                    throw new SpreadArgumentException("Spread argument cannot be null.");
                if (evaluated is not IPositionalValue spread)
                    throw new SpreadArgumentException("Spread argument must evaluate to a tuple.");
                values.AddRange(Enumerable.Range(0, spread.Arity).Select(spread.GetPosition));
            }
            else
            {
                values.Add(evaluated);
            }
        }

        return new Expressif.Values.Tuple(values.ToArray());
    }

    private Expressif.Values.Vector BuildVector(VectorParameter vector, IContext currentContext)
    {
        var values = new List<object?>();
        foreach (var element in vector.Elements)
        {
            var elementFactory = (Func<object?>)CreateParameter(element.Value, typeof(object), currentContext);
            var evaluated = elementFactory.Invoke();
            if (element.IsSpread)
            {
                if (evaluated is null)
                    throw new SpreadArgumentException("Spread argument cannot be null.");
                if (evaluated is not VectorValue spread)
                    throw new SpreadArgumentException("Vector spread argument must evaluate to a vector.");
                values.AddRange(spread);
            }
            else
            {
                values.Add(evaluated);
            }
        }

        return new Expressif.Values.Vector(values.ToArray());
    }

    private ValueRecord BuildRecord(RecordLiteralParameter record, IContext currentContext)
    {
        var value = new ValueRecord();
        foreach (var field in record.Fields)
        {
            if (value.ContainsKey(field.Name))
                throw new ArgumentException($"Duplicate field '{field.Name}' in record literal.");

            if (field.Value is QuotedLiteralParameter quoted)
            {
                value.Set(field.Name, quoted.Value);
                continue;
            }

            if (field.Value is LiteralParameter literal)
            {
                value.Set(field.Name, literal.Value);
                continue;
            }

            var elementFactory = CreateParameter(field.Value, typeof(object), currentContext);
            value.Set(field.Name, elementFactory.DynamicInvoke());
        }

        return value;
    }

    private static IInterval BuildInterval(IntervalBinding value)
    {
        var lowerBoundType = value.IsLowerInclusive ? IntervalType.Closed : IntervalType.Open;
        var upperBoundType = value.IsUpperInclusive ? IntervalType.Closed : IntervalType.Open;
        var finiteValue = value.LowerBound.Value ?? value.UpperBound.Value;
        return finiteValue switch
        {
            DateOnly or DateTime => new Interval<DateTime>(
                ResolveDateTime(value.LowerBound), ResolveDateTime(value.UpperBound), lowerBoundType, upperBoundType),
            TimeOnly => new Interval<TimeOnly>(
                ResolveTime(value.LowerBound), ResolveTime(value.UpperBound), lowerBoundType, upperBoundType),
            decimal or null => new Interval<decimal>(
                ResolveNumeric(value.LowerBound), ResolveNumeric(value.UpperBound), lowerBoundType, upperBoundType),
            _ => throw new InvalidOperationException($"Unsupported interval bound type '{finiteValue.GetType().Name}'."),
        };
    }

    private static decimal ResolveNumeric(IntervalBoundBinding bound) => bound.Kind switch
    {
        IntervalBoundBindingKind.NegativeInfinity => decimal.MinValue,
        IntervalBoundBindingKind.PositiveInfinity => decimal.MaxValue,
        IntervalBoundBindingKind.Finite when bound.Value is decimal value => value,
        _ => throw new InvalidOperationException("Interval bounds must have compatible numeric types."),
    };

    private static DateTime ResolveDateTime(IntervalBoundBinding bound) => bound.Kind switch
    {
        IntervalBoundBindingKind.NegativeInfinity => DateTime.MinValue,
        IntervalBoundBindingKind.PositiveInfinity => DateTime.MaxValue,
        IntervalBoundBindingKind.Finite when bound.Value is DateTime value => value,
        IntervalBoundBindingKind.Finite when bound.Value is DateOnly value => value.ToDateTime(TimeOnly.MinValue),
        _ => throw new InvalidOperationException("Interval bounds must have compatible temporal types."),
    };

    private static TimeOnly ResolveTime(IntervalBoundBinding bound) => bound.Kind switch
    {
        IntervalBoundBindingKind.NegativeInfinity => TimeOnly.MinValue,
        IntervalBoundBindingKind.PositiveInfinity => TimeOnly.MaxValue,
        IntervalBoundBindingKind.Finite when bound.Value is TimeOnly value => value,
        _ => throw new InvalidOperationException("Interval bounds must have compatible temporal types."),
    };

    protected static object? ResolveTupleProjection(object? value, TupleProjectionParameter projection)
    {
        if (value is not IPositionalValue tuple)
            return null;

        var index = projection.FromEnd ? tuple.Arity - projection.Index : projection.Index;
        return index >= 0 && index < tuple.Arity ? tuple.GetPosition(index) : null;
    }

    protected static object? ResolveScopedTupleProjection(ScopedTupleProjectionParameter projection)
    {
        var frame = EvaluationRuntime.Frame;
        for (var depth = 1; depth < projection.ScopeDepth && frame is not null; depth++)
            frame = frame.Parent;
        return ResolveTupleProjection(frame?.Ambient, new TupleProjectionParameter(projection.Index));
    }

    private static object? GetAmbient(IContext context)
        => ArgumentScope.Root(context.CurrentObject.Value, EvaluationRuntime.Frame?.Scope.Resolve(FieldReferenceKind.ExpressionRoot, null, null));

    private static object? GetCurrent(IContext context)
        => EvaluationRuntime.Frame is { } frame ? frame.Current : context.CurrentObject.Value;

    private static object? GetVariable(IContext context, string name)
        => EvaluationRuntime.TryGetBinding(name, out var bound) ? bound
            : EvaluationRuntime.Context is { } evaluationContext
            && evaluationContext.TryGetVariable(name, out var value)
                ? value
                : context.Variables[name];

    private static object? GetAmbientValue(IContext context, string name)
        => NamedValueAccessor.Get(GetAmbient(context), name);

    private static object? GetAmbientValue(IContext context, int index)
    {
        var ambient = new ContextObject();
        ambient.Set(GetAmbient(context));
        return ambient[index];
    }

    protected Delegate CreateCast(object value, Type type)
        => CreateFunctionCast(() => value, type);

    private MethodInfo? cacheFunctionCastInfo;
    protected Delegate CreateFunctionCast(Func<object?> function, Type type)
    {
        var method = cacheFunctionCastInfo ??= typeof(BaseExpressionFactory).GetMethod(nameof(CreateFunctionCastCore), BindingFlags.Instance | BindingFlags.NonPublic)
                        ?? throw new MissingMethodException();
        return (Delegate)method.MakeGenericMethod(type).Invoke(this, [function])!;
    }

    private Func<T?> CreateFunctionCastCore<T>(Func<object?> function)
        => () => (T?)Converter.Convert(function.Invoke(), typeof(T));

    protected Delegate CreateDelegateCast(Delegate function, Type type)
        => CreateFunctionCast(() => function.DynamicInvoke(), type);

    protected virtual Delegate CreateInputExpression(InputExpressionParameter input, Type type, IContext context)
    {
        var functions = new List<IFunction>();
        foreach (var member in input.Expression.Members)
            functions.Add(Instantiate<IFunction>(member.Name, member.Parameters, context));
        var expression = new ChainFunction(functions);

        var arg = CreateParameter(input.Expression.Parameter, typeof(object), context);

        return CreateFunctionCast(() => expression.Evaluate(arg.DynamicInvoke()), type);
    }
}
