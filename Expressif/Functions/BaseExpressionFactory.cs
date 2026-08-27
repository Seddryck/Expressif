using Expressif.Bindings;
using Expressif.Values;
using Expressif.Values.Casters;
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
    protected BaseTypeMapper TypeMapper { get; }

    private static Caster? caster;
    private static Caster Caster => caster ??= new();

    protected BaseExpressionFactory(BaseTypeMapper typeSetter)
        => TypeMapper = typeSetter;

    protected internal T Instantiate<T>(string functionName, IParameter[] parameters, IContext context)
        => Instantiate<T>(TypeMapper.Execute(functionName), parameters, context);

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
            RecordLiteralParameter record => CreateFunctionCast(() => BuildRecord(record, context), scalarType),
            InputExpressionParameter input => CreateDelegateCast(CreateInputExpression(input, scalarType, context), scalarType),
            IntervalParameter interval => CreateCast(buildInterval(interval.Value), scalarType),
            QuotedLiteralParameter quoted => CreateCast(quoted.Value, scalarType),
            LiteralParameter { Value: null } => CreateFunctionCast(() => null, scalarType),
            LiteralParameter literal => CreateCast(literal.Value, scalarType),
            ObjectIndexParameter index => CreateFunctionCast(() => GetAmbientValue(context, index.Index), scalarType),
            TupleProjectionParameter projection => CreateFunctionCast(() => ResolveTupleProjection(GetCurrent(context), projection), scalarType),
            ObjectPropertyParameter prop => CreateFunctionCast(() => GetAmbientValue(context, prop.Name), scalarType),
            VariableParameter variable => CreateFunctionCast(() => GetVariable(context, variable.Name), scalarType),
            ContextParameter contextReference => CreateFunctionCast(() => contextReference.Function.Invoke(context), scalarType),
            _ => throw new BindingException($"Cannot handle the parameter type '{parameter.GetType().Name}'.")
        };

        object?[] BuildArray(ArrayParameter array, IContext currentContext)
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

        Expressif.Values.Tuple BuildTuple(TupleParameter tuple, IContext currentContext)
        {
            var values = new object?[tuple.Values.Length];
            for (var i = 0; i < tuple.Values.Length; i++)
            {
                var elementFactory = CreateParameter(tuple.Values[i], typeof(object), currentContext);
                values[i] = elementFactory.DynamicInvoke();
            }

            return new Expressif.Values.Tuple(values);
        }

        ValueRecord BuildRecord(RecordLiteralParameter record, IContext currentContext)
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

        static IInterval buildInterval(IntervalBinding value)
            => new IntervalBuilder().Create(value);
    }

    private static object? ResolveTupleProjection(object? value, TupleProjectionParameter projection)
    {
        if (value is not TupleValue tuple)
            return null;

        var index = projection.FromEnd ? tuple.Count - projection.Index : projection.Index;
        return index >= 0 && index < tuple.Count ? tuple[index] : null;
    }

    private static object? GetAmbient(IContext context)
        => context.CurrentObject.Value ?? EvaluationRuntime.Frame?.Ambient;

    private static object? GetCurrent(IContext context)
        => EvaluationRuntime.Frame?.Current ?? context.CurrentObject.Value;

    private static object? GetVariable(IContext context, string name)
        => EvaluationRuntime.Context is { } evaluationContext
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

    private MethodInfo? cacheCastInfo;
    protected Delegate CreateCast(object value, Type type)
    {
        var method = cacheCastInfo ??= typeof(BaseExpressionFactory).GetMethod(nameof(Cast), BindingFlags.Static | BindingFlags.NonPublic)
                        ?? throw new MissingMethodException();
        var genericMethod = method.MakeGenericMethod(type);
        return Delegate.CreateDelegate(typeof(Func<>).MakeGenericType(type), value, genericMethod);
    }

    protected static T? Cast<T>(object value)
        => Caster.Cast<T>(value);

    private MethodInfo? cacheFunctionCastInfo;
    protected Delegate CreateFunctionCast(Func<object?> function, Type type)
    {
        var method = cacheFunctionCastInfo ??= typeof(BaseExpressionFactory).GetMethod(nameof(FunctionCast), BindingFlags.Static | BindingFlags.NonPublic)
                        ?? throw new MissingMethodException();
        var genericMethod = method.MakeGenericMethod(type);
        return Delegate.CreateDelegate(typeof(Func<>).MakeGenericType(type), function, genericMethod);
    }

    protected static T? FunctionCast<T>(Func<object?> function)
        => Caster.Cast<T>(function.Invoke());

    private MethodInfo? cacheDelegateCastInfo;
    protected Delegate CreateDelegateCast(Delegate function, Type type)
    {
        var method = cacheDelegateCastInfo ??= typeof(BaseExpressionFactory).GetMethod(nameof(DelegateCast), BindingFlags.Static | BindingFlags.NonPublic)
                        ?? throw new MissingMethodException();
        var genericMethod = method.MakeGenericMethod(type);
        return Delegate.CreateDelegate(typeof(Func<>).MakeGenericType(type), function, genericMethod);
    }

    protected static T? DelegateCast<T>(Delegate @delegate)
        => Caster.Cast<T>(@delegate.DynamicInvoke());

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
