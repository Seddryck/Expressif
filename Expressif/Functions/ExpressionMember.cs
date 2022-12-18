﻿using Expressif.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Functions
{
    public record class ExpressionMember(Type Type, object[] Parameters)
    {
        public IFunction Build(Context context, ExpressionFactory factory)
        {
            var typedParameters = new List<IParameter>();
            foreach (var parameter in Parameters)
            {
                typedParameters.Add(parameter switch
                {
                    IParameter p => p,
                    _ => new LiteralParameter(parameter.ToString()!)
                });
            }
            return factory.Instantiate(Type, typedParameters.ToArray(), context);
        }
    };
}