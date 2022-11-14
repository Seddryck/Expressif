﻿using Expressif.Parsers;
using Expressif.Values;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Expressif.Functions
{
    public class FunctionFactory
    {
        private ScalarResolverFactory ScalarResolverFactory { get; } = new ScalarResolverFactory();

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
                    var resolver = ScalarResolverFactory.Instantiate(param.Value, scalarType, context);
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
    }
}
