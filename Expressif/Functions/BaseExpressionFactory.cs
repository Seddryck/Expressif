using Expressif;
using Expressif.Functions;
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

namespace Expressif.Functions
{
    public abstract class BaseExpressionFactory
    {

        protected T Instantiate<T>(string functionName, IParameter[] parameters, Context context)
            => Instantiate<T>(GetFunctionType<T>(functionName), parameters, context);

        protected T Instantiate<T>(Type type, IParameter[] parameters, Context context)
        {
            var ctor = GetMatchingConstructor(type, parameters.Length);

            var zip = ctor.GetParameters().Zip(parameters, (x, y) => new { x.ParameterType, Value = y });
            var typedFunctionParameters = new List<object>();

            foreach (var param in zip)
            {
                //If the parameter of the function is a IScalarResolver
                if (typeof(IScalarResolver).IsAssignableFrom(param.ParameterType))
                {
                    var scalarType = param.ParameterType.GenericTypeArguments[0];
                    var resolver = param.Value switch
                    {
                        InputExpressionParameter exp => GetParametrizedExpressionScalarResolver(exp, scalarType, context),
                        _ => InstantiateScalarResolver(param.Value, scalarType, context),
                    };
                    typedFunctionParameters.Add(resolver);
                }
                else if (typeof(IInterval).IsAssignableFrom(param.ParameterType))
                {
                    var intervalType = param.ParameterType.GenericTypeArguments[0];
                    var resolver = InstantiateInterval(param.Value, intervalType, context);
                    typedFunctionParameters.Add(resolver);
                }
                else
                    typedFunctionParameters.Add(param.Value);
            }

            return (T)ctor.Invoke(typedFunctionParameters.ToArray());
        }

        protected virtual Type GetFunctionType<T>(string functionName)
            =>typeof(T).Assembly.GetTypes()
                .Where(
                        t => t.IsClass
                        && t.IsAbstract == false
                        && t.Name == functionName.ToPascalCase()
                        && t.GetInterface(typeof(T).Name) != null)
                .SingleOrDefault()
                ?? throw new NotImplementedFunctionException(functionName.ToPascalCase());

        protected internal ConstructorInfo GetMatchingConstructor(Type type, int paramCount)
            => type.GetConstructors().SingleOrDefault(x => x.GetParameters().Length == paramCount)
                ?? throw new MissingOrUnexpectedParametersFunctionException(type.Name, paramCount);

        protected IScalarResolver InstantiateScalarResolver(IParameter parameter, Type type, Context context)
            => parameter switch
            {
                LiteralParameter l => InstantiateScalarResolver(typeof(LiteralScalarResolver<>), type, new object[] { l.Value }),
                VariableParameter v => InstantiateScalarResolver(typeof(VariableScalarResolver<>), type, new object[] { v.Name, context.Variables }),
                ObjectPropertyParameter item => InstantiateScalarResolver(typeof(ObjectPropertyResolver<>), type, new object[] { item.Name, context.CurrentObject }),
                ObjectIndexParameter index => InstantiateScalarResolver(typeof(ObjectIndexResolver<>), type, new object[] { index.Index, context.CurrentObject }),
                _ => throw new ArgumentOutOfRangeException(nameof(parameter))
            };

        private IScalarResolver InstantiateScalarResolver(Type generic, Type type, object[] parameters)
                    => (Activator.CreateInstance(generic.MakeGenericType(type), parameters) as IScalarResolver)!;

        protected IInterval InstantiateInterval(IParameter parameter, Type type, Context context)
            => parameter switch
            {
                IntervalParameter i => new IntervalBuilder().Create(i.Value.LowerBoundType, i.Value.LowerBound, i.Value.UpperBound, i.Value.UpperBoundType),
                _ => throw new ArgumentOutOfRangeException(nameof(parameter))
            };

        protected internal IScalarResolver GetParametrizedExpressionScalarResolver(InputExpressionParameter exp, Type type, Context context)
        {
            var functions = new List<IFunction>();
            foreach (var member in exp.Expression.Members)
                functions.Add(Instantiate<IFunction>(member.Name, member.Parameters, context));
            var expression = new ChainFunction(functions);

            var arg = InstantiateScalarResolver(exp.Expression.Parameter, typeof(object), context);

            return InstantiateScalarResolver(typeof(InputExpressionResolver<>), type, new object[] { arg, expression });
        }
    }
}
