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
    public class ExpressionFactory
    {
        private Parser<Parsers.Expression> Parser { get; } = Parsers.Expression.Parser;

        public IFunction Instantiate(string code, Context context)
        {
            var expression = Parser.Parse(code);

            var functions = new List<IFunction>();
            foreach (var member in expression.Members)
                functions.Add(Instantiate(member.Name, member.Parameters, context));
            return new ExpressionFunction(functions);
        }

        public IFunction Instantiate(string functionName, IParameter[] parameters, Context context)
            => Instantiate(GetFunctionType(functionName), parameters, context);

        public IFunction Instantiate(Type type, IParameter[] parameters, Context context)
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
                        ParameterizedExpressionParameter exp => GetParametrizedExpressionScalarResolver(exp, scalarType, context),
                        _ => InstantiateScalarResolver(param.Value, scalarType, context),
                    };
                    typedFunctionParameters.Add(resolver);
                }
                else
                    typedFunctionParameters.Add(param.Value);
            }

            return (IFunction)ctor.Invoke(typedFunctionParameters.ToArray());
        }

        protected internal virtual Type GetFunctionType(string functionName)
        {
            var textInfo = CultureInfo.InvariantCulture.TextInfo;
            var className = textInfo.ToTitleCase(functionName.Trim().Replace("-", " "))
                .Replace(" ", "")
                .Replace("Datetime", "DateTime")
                .Replace("Timespan", "TimeSpan");

            return typeof(IFunction).Assembly.GetTypes()
                       .Where(
                                t => t.IsClass
                                && t.IsAbstract == false
                                && t.Name == className
                                && t.GetInterface(typeof(IFunction).Name) != null)
                       .SingleOrDefault()
                       ?? throw new NotImplementedFunctionException(className);
        }

        protected internal virtual ConstructorInfo GetMatchingConstructor(Type type, int paramCount)
            => type.GetConstructors().SingleOrDefault(x => x.GetParameters().Length == paramCount)
                ?? throw new MissingOrUnexpectedParametersFunctionException(type.Name, paramCount);

        protected internal IScalarResolver InstantiateScalarResolver(IParameter parameter, Type type, Context context)
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

        protected internal IScalarResolver GetParametrizedExpressionScalarResolver(ParameterizedExpressionParameter exp, Type type, Context context)
        {
            var functions = new List<IFunction>();
            foreach (var member in exp.Expression.Members)
                functions.Add(Instantiate(member.Name, member.Parameters, context));
            var expression = new ExpressionFunction(functions);

            var arg = InstantiateScalarResolver(exp.Expression.Parameter, typeof(object), context);

            return InstantiateScalarResolver(typeof(ParametrizedExpressionResolver<>), type, new object[] { arg, expression });
        }
    }
}
