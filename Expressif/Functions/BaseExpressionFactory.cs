using Expressif.Parsers;
using Expressif.Values;
using Expressif.Values.Casters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Expressif.Functions;

public abstract class BaseExpressionFactory
{
    protected BaseTypeMapper TypeMapper { get; }

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
                typedFunctionParameters.Add(() => param.Value);
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
            InputExpressionParameter input => CreateDelegateCast(CreateInputExpression(input, scalarType, context), scalarType),
            IntervalParameter interval => CreateCast(buildInterval(interval.Value), scalarType),
            LiteralParameter literal => CreateCast(literal.Value, scalarType),
            ObjectIndexParameter index => CreateFunctionCast(() => context.CurrentObject[index.Index], scalarType),
            ObjectPropertyParameter prop => CreateFunctionCast(() => context.CurrentObject[prop.Name], scalarType),
            VariableParameter variable => CreateFunctionCast(() => context.Variables[variable.Name], scalarType),
            ContextParameter contextReference => CreateFunctionCast(() => contextReference.Function.Invoke(context), scalarType),
            _ => throw new NotImplementedException($"Cannot handle the parameter type '{parameter.GetType().Name}'")
        };

        static IInterval buildInterval(Interval value)
            => new IntervalBuilder().Create(value.LowerBoundType, value.LowerBound, value.UpperBound, value.UpperBoundType);
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
        => new Caster().Cast<T>(value);

    private MethodInfo? cacheFunctionCastInfo;
    protected Delegate CreateFunctionCast(Func<object?> function, Type type)
    {
        var method = cacheFunctionCastInfo ??= typeof(BaseExpressionFactory).GetMethod(nameof(FunctionCast), BindingFlags.Static | BindingFlags.NonPublic)
                        ?? throw new MissingMethodException();
        var genericMethod = method.MakeGenericMethod(type);
        return Delegate.CreateDelegate(typeof(Func<>).MakeGenericType(type), function, genericMethod);
    }

    protected static T? FunctionCast<T>(Func<object?> function)
        => new Caster().Cast<T>(function.Invoke());
        

    private MethodInfo? cacheDelegateCastInfo;
    protected Delegate CreateDelegateCast(Delegate function, Type type)
    {
        var method = cacheDelegateCastInfo ??= typeof(BaseExpressionFactory).GetMethod(nameof(DelegateCast), BindingFlags.Static | BindingFlags.NonPublic)
                        ?? throw new MissingMethodException();
        var genericMethod = method.MakeGenericMethod(type);
        return Delegate.CreateDelegate(typeof(Func<>).MakeGenericType(type), function, genericMethod);
    }

    protected static T? DelegateCast<T>(Delegate @delegate)
        => new Caster().Cast<T>(@delegate.DynamicInvoke());

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
