using Expressif.Functions;
using Expressif.Parsers;
using Expressif.Values.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Values
{
    public class ScalarResolverFactory
    {
        public IScalarResolver Instantiate(IParameter parameter, Type type, Context context)
            => parameter switch
            {
                LiteralParameter l => Instantiate(typeof(LiteralScalarResolver<>), type, new object[] { l.Value }),
                VariableParameter v => Instantiate(typeof(VariableScalarResolver<>), type, new object[] { v.Name, context.Variables }),
                ObjectPropertyParameter item => Instantiate(typeof(ObjectPropertyResolver<>), type, new object[] { item.Name, context.CurrentObject }),
                ObjectIndexParameter index => Instantiate(typeof(ObjectIndexResolver<>), type, new object[] { index.Index, context.CurrentObject }),
                _ => throw new ArgumentOutOfRangeException(nameof(parameter))
            };

        private IScalarResolver Instantiate(Type generic, Type type, object[] parameters)
            => (Activator.CreateInstance(generic.MakeGenericType(type), parameters) as IScalarResolver)!;
    }
}
