
/* Unmerged change from project 'Expressif (net6.0)'
Before:
using Expressif.Parsers;
After:
using Expressif;
using Expressif;
using Expressif.Parsers;
*/
using Expressif.Functions;
using Expressif.Functions.Serializer;
using Expressif.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Expressif
{
    public class ExpressionBuilder
    {
        
        private Context Context { get; }
        private ExpressionFactory Factory { get; }
        private ExpressionSerializer Serializer { get; }

        public ExpressionBuilder()
            : this(new Context()) { }
        public ExpressionBuilder(Context? context = null, ExpressionFactory? factory = null, ExpressionSerializer? serializer = null)
            => (Context, Factory, Serializer) = (context ?? new Context(), factory ?? new ExpressionFactory(), serializer ?? new ExpressionSerializer());

        public Queue<object> Pile { get; } = new();

        public ExpressionBuilder Chain<T>(params object?[] parameters) where T : IFunction
            => Chain(typeof(T), parameters);

        public ExpressionBuilder Chain(Type type, params object?[] parameters)
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

        public string Serialize()
        {
            if (!Pile.Any())
                throw new InvalidOperationException();

            return Serializer.Serialize(this);
        }
    }
}
