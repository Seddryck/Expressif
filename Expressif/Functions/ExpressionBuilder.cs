using Expressif.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Expressif.Functions
{
    public class ExpressionBuilder
    {
        private record class ExpressionMember(Type Type, object[] Parameters)
        {
            public IFunction Build(Context context, ExpressionFactory factory)
            {
                var typedParameters = new List<IParameter>();
                foreach (var parameter in Parameters)
                {
                    if (parameter is not IParameter)
                        typedParameters.Add(new LiteralParameter(parameter.ToString()!));
                }
                return factory.Instantiate(Type, typedParameters.ToArray(), context);
            }
        };

        private Context Context { get; }
        private ExpressionFactory Factory { get; }
        public ExpressionBuilder()
            : this(new Context()) { }
        public ExpressionBuilder(Context context)
            : this(context, new ExpressionFactory()) { }
        public ExpressionBuilder(Context context, ExpressionFactory factory)
            => (Context, Factory) = (context, factory);

        public Queue<object> Pile { get; } = new();

        public ExpressionBuilder Chain<T>(params object[] parameters) where T : IFunction
            => Chain(typeof(T), parameters);

        public ExpressionBuilder Chain(Type type, params object[] parameters)
        {
            if (!type.GetInterfaces().Contains(typeof(IFunction)))
                throw new ArgumentException($"The type '{type.FullName}' doesn't implement the interface '{nameof(IFunction)}'. Only types implementing this interface can be chained to create an expression.", nameof(type));

            Pile.Enqueue(new ExpressionMember(type, parameters));
            return this;
        }

        public ExpressionBuilder Chain(ExpressionBuilder builder)
        {
            Pile.Enqueue(builder);
            return this;
        }

        public ExpressionBuilder Chain(IFunction function)
        {
            Pile.Enqueue(function);
            return this;
        }

        public IFunction Build()
        {
            IFunction? function = null;
            if (!Pile.Any())
                throw new InvalidOperationException();
            
            while (Pile.Any())
            {
                var member = Pile.Dequeue() switch
                {
                    ExpressionMember m => m.Build(Context, Factory),
                    ExpressionBuilder b => b.Build(),
                    IFunction f => f,
                    _ => throw new NotSupportedException()
                };
                function = function is null ? member : new ChainFunction(new[] { function, member });
            }
            return function!;
        }
    }
}
