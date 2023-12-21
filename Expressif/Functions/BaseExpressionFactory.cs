using Expressif.Parsers;
using Expressif.Values;
using Expressif.Values.Resolvers;
using Sprache;
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

    protected internal T Instantiate<T>(string functionName, IParameter[] parameters, Context context)
        => Instantiate<T>(TypeMapper.Execute(functionName), parameters, context);

    protected T Instantiate<T>(Type type, IParameter[] parameters, Context context)
    {
        var ctor = GetMatchingConstructor(type, parameters.Length);

        var zip = ctor.GetParameters().Zip(parameters, (x, y) => new { x.ParameterType, Value = y });
        var typedFunctionParameters = new List<Delegate>();

        foreach (var param in zip)
        {
            //If the parameter of the function is a Func<>
            if (param.ParameterType.IsGenericType && param.ParameterType.GetGenericTypeDefinition()==typeof(Func<>))
            {
                var scalarType = param.ParameterType.GenericTypeArguments[0];
                var @delegate = param.Value switch
                {
                    InputExpressionParameter exp => InstantiateInputExpressionDelegate(exp, scalarType, context),
                    IntervalParameter interval => InstantiateIntervalDelegate(param.Value, scalarType, context),
                    _ => InstantiateScalarDelegate(param.Value, scalarType, context),
                };
                typedFunctionParameters.Add(@delegate);
            }
            else
                typedFunctionParameters.Add(() => param.Value);
        }

        return (T)ctor.Invoke(typedFunctionParameters.ToArray());
    }

    protected internal ConstructorInfo GetMatchingConstructor(Type type, int paramCount)
        => type.GetConstructors().SingleOrDefault(x => x.GetParameters().Length == paramCount)
            ?? throw new MissingOrUnexpectedParametersFunctionException(type.Name, paramCount);

    protected Delegate InstantiateScalarDelegate(IParameter parameter, Type scalarType, Context context)
    {
        var instantiate = typeof(BaseExpressionFactory).GetMethod(nameof(InstantiateScalarResolver), BindingFlags.Static | BindingFlags.NonPublic, new[] { typeof(IParameter), typeof(Context) })
            ?? throw new InvalidProgramException(nameof(InstantiateScalarResolver));
        var instantiateGeneric = instantiate.MakeGenericMethod(scalarType);
        var resolver = instantiateGeneric.Invoke(null, new object[] { parameter, context })!;
        var execute = resolver.GetType().GetMethod("Execute", BindingFlags.Instance | BindingFlags.Public)!;

        var funcType = typeof(Func<>).MakeGenericType(scalarType);

        return Delegate.CreateDelegate(funcType, resolver, execute);
    }

    private static IScalarResolver<T> InstantiateScalarResolver<T>(IParameter parameter, Context context)
        => parameter switch
        {
            LiteralParameter l => InstantiateScalarResolver<T>(typeof(LiteralScalarResolver<T>), new object[] { l.Value }),
            VariableParameter v => InstantiateScalarResolver<T>(typeof(VariableScalarResolver<T>), new object[] { v.Name, context.Variables }),
            ObjectPropertyParameter item => InstantiateScalarResolver<T>(typeof(ObjectPropertyResolver<T>), new object[] { item.Name, context.CurrentObject }),
            ObjectIndexParameter index => InstantiateScalarResolver<T>(typeof(ObjectIndexResolver<T>), new object[] { index.Index, context.CurrentObject }),
            _ => throw new ArgumentOutOfRangeException(nameof(parameter))
        };

    private static IScalarResolver<T> InstantiateScalarResolver<T>(Type generic, object[] parameters)
        => (Activator.CreateInstance(generic, parameters) as IScalarResolver<T>)!;

    protected Delegate InstantiateIntervalDelegate(IParameter parameter, Type type, Context context)
    {
        if (parameter is not IntervalParameter i)
            throw new ArgumentOutOfRangeException(nameof(parameter));

        var interval = new IntervalBuilder().Create(i.Value.LowerBoundType, i.Value.LowerBound, i.Value.UpperBound, i.Value.UpperBoundType);

        var instantiate = typeof(BaseExpressionFactory).GetMethod(nameof(InstantiateScalarResolver), BindingFlags.Static | BindingFlags.NonPublic, new[] { typeof(Type), typeof(object[]) })
            ?? throw new InvalidProgramException(nameof(InstantiateScalarResolver));
        var instantiateGeneric = instantiate.MakeGenericMethod(type);

        var typeGeneric = typeof(IntervalResolver<>).MakeGenericType(type);

        var resolver = instantiateGeneric.Invoke(null, new object[] { typeGeneric, new object[] { interval } })!;
        var execute = resolver.GetType().GetMethod("Execute", BindingFlags.Instance | BindingFlags.Public)!;

        var funcType = typeof(Func<>).MakeGenericType(type);
        return Delegate.CreateDelegate(funcType, resolver, execute);
    }

    protected Delegate InstantiateInputExpressionDelegate(InputExpressionParameter exp, Type type, Context context)
    {
        var functions = new List<IFunction>();
        foreach (var member in exp.Expression.Members)
            functions.Add(Instantiate<IFunction>(member.Name, member.Parameters, context));
        var expression = new ChainFunction(functions);

        var arg = InstantiateScalarDelegate(exp.Expression.Parameter, typeof(object), context);

        var instantiate = typeof(BaseExpressionFactory).GetMethod(nameof(InstantiateScalarResolver), BindingFlags.Static | BindingFlags.NonPublic, new[] { typeof(Type), typeof(object[]) })
            ?? throw new InvalidProgramException(nameof(InstantiateScalarResolver));
        var instantiateGeneric = instantiate.MakeGenericMethod(type);

        var typeGeneric = typeof(InputExpressionResolver<>).MakeGenericType(type);

        var resolver = instantiateGeneric.Invoke(null, new object[] {typeGeneric, new object[] { arg, expression } })!;
        var execute = resolver.GetType().GetMethod("Execute", BindingFlags.Instance | BindingFlags.Public)!;

        var funcType = typeof(Func<>).MakeGenericType(type);
        return Delegate.CreateDelegate(funcType, resolver, execute);
    }
}
