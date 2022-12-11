using Expressif.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Functions
{
    public class ExpressionBuilder
    {
        private Context Context { get; }
        private ExpressionFactory Factory { get; }
        public ExpressionBuilder()
            : this(new Context()) { }
        public ExpressionBuilder(Context context)
            : this(context, new ExpressionFactory()) { }
        public ExpressionBuilder(Context context, ExpressionFactory factory)
            => (Context, Factory) = (context, factory);

        public ExpressionMember? Root { get; private set; }

        public ExpressionMember As<T>(params object[] parameters) where T : IFunction
            => As(typeof(T), parameters);

        public ExpressionMember As(Type type, params object[] parameters)
            => Root = new ExpressionMember(type, parameters, Context, Factory);

        public IFunction Build()
        {
            if (Root is null)
                throw new InvalidOperationException();
            
            IFunction function = Root.Value;    
            var member = Root;
            while (member.Next is not null)
            {
                member = member.Next;
                function = new ChainFunction(new[] { function, member.Value });
            }
            return function;
        }

        public class ExpressionMember
        {
            private Context Context { get; }
            private ExpressionFactory Factory { get; }
            public IFunction Value { get; private set; }
            public ExpressionMember(Type type, object[] parameters, Context context, ExpressionFactory factory)
            {
                (Context, Factory) = (context, factory);
                var typedParameters = new List<IParameter>();
                foreach (var parameter in parameters)
                {
                    if (parameter is not IParameter)
                        typedParameters.Add(new LiteralParameter(parameter.ToString()!));
                }
                Value = factory.Instantiate(type, typedParameters.ToArray(), context);
            }

            public ExpressionMember? Next { get; private set; }

            public ExpressionMember Chain<T>(params object[] parameters) where T : IFunction
                => Chain(typeof(T), parameters);

            public ExpressionMember Chain(Type type, params object[] parameters)
                => Next = new ExpressionMember(type, parameters, Context, Factory);

        }
    }
}
