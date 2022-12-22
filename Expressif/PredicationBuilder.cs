using Expressif.Predicates;
using Expressif.Predicates.Serializer;
using Expressif.Predicates.Combination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions;

namespace Expressif
{
    public class PredicationBuilder
    {
        
        private Context Context { get; }
        private PredicationFactory Factory { get; }
        private PredicationSerializer Serializer { get; }

        public PredicationBuilder()
            : this(new Context()) { }
        public PredicationBuilder(Context? context = null, PredicationFactory? factory = null, PredicationSerializer? serializer = null)
            => (Context, Factory, Serializer) = (context ?? new Context(), factory ?? new(), serializer ?? new());

        public Queue<object> Pile { get; } = new();

        public PredicationBuilder Chain<P>(params object[] parameters)
            where P : IPredicate
            => ChainWork(null, typeof(P), parameters);

        public PredicationBuilder And<P>(params object[] parameters)
            where P : IPredicate
            => ChainWork(typeof(AndOperator), typeof(P), parameters);

        public PredicationBuilder Or<P>(params object[] parameters)
            where P : IPredicate
            => ChainWork(typeof(OrOperator), typeof(P), parameters);

        public PredicationBuilder Xor<P>(params object[] parameters)
            where P : IPredicate
            => ChainWork(typeof(XorOperator), typeof(P), parameters);

        public PredicationBuilder Chain<O, P>(params object[] parameters) 
            where O: ICombinationOperator
            where P : IPredicate
            => ChainWork(typeof(O), typeof(P), parameters);

        public PredicationBuilder Chain(Type predicate, params object[] parameters)
            => ChainWork(null, predicate, parameters);

        public PredicationBuilder And(Type predicate, params object[] parameters)
            => ChainWork(typeof(AndOperator), predicate, parameters);

        public PredicationBuilder Or(Type predicate, params object[] parameters)
            => ChainWork(typeof(OrOperator), predicate, parameters);

        public PredicationBuilder Xor(Type predicate, params object[] parameters)
            => ChainWork(typeof(XorOperator), predicate, parameters);

        public PredicationBuilder Chain(Type @operator, Type predicate, params object[] parameters)
            => ChainWork(@operator, predicate, parameters);

        protected PredicationBuilder ChainWork(Type? @operator, Type predicate, params object[] parameters)
        {
            if (@operator != null && !@operator.GetInterfaces().Contains(typeof(ICombinationOperator)))
                throw new ArgumentException($"The type '{@operator.FullName}' doesn't implement the interface '{nameof(ICombinationOperator)}'. Only types implementing this interface can be chained to create a predication.", nameof(@operator));

            if (!predicate.GetInterfaces().Contains(typeof(IPredicate)))
                throw new ArgumentException($"The type '{predicate.FullName}' doesn't implement the interface '{nameof(IPredicate)}'. Only types implementing this interface can be chained to create a predication.", nameof(predicate));

            Pile.Enqueue(new PredicationMember(@operator, predicate, parameters));
            return this;
        }

        public PredicationBuilder Chain<O>(PredicationBuilder builder) where O : ICombinationOperator
            => Chain(typeof(O), builder);

        public PredicationBuilder Chain(Type @operator, PredicationBuilder builder)
        {
            Pile.Enqueue(new SubPredicationMember (@operator, builder));
            return this;
        }

        public PredicationBuilder Chain<O>(IPredicate predicate) where O : ICombinationOperator
        {
            Pile.Enqueue(predicate);
            return this;
        }

        public IPredicate Build()
        {
            IPredicate? predicate = null;
            if (!Pile.Any())
                throw new InvalidOperationException();

            while (Pile.Any())
            {
                ICombinationOperator? @operator = null;
                IPredicate? memberPredicate = null;
                var member = Pile.Dequeue();

                if (member is PredicationMember m)
                    (@operator, memberPredicate) = m.Build(Context, Factory);
                else if (member is SubPredicationMember sub)
                   (@operator, memberPredicate) = sub.Build(Context, Factory);
                else
                    throw new NotSupportedException();
                predicate = predicate is null ? memberPredicate : new CombinedPredicate(predicate, @operator!, memberPredicate!);
            }
            return predicate!;
        }

        public string Serialize()
        {
            if (!Pile.Any())
                throw new InvalidOperationException();

            return Serializer.Serialize(this);
        }
    }
}
