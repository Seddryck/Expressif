using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Predicates.Operators;

namespace Expressif.Predicates;
public class PredicateCombiner
{
    protected UnaryOperatorFactory UnaryFactory { get; }
    protected BinaryOperatorFactory BinaryFactory { get; }

    public PredicateCombiner()
        : this(new(), new()) { }

    public PredicateCombiner(UnaryOperatorFactory unaryFactory , BinaryOperatorFactory binaryFactory)
        => (UnaryFactory, BinaryFactory) = (unaryFactory, binaryFactory);

    public PredicateRightCombiner With(IPredicate left)
        => new(UnaryFactory, BinaryFactory, left);

    public PredicateRightCombiner WithNot(IPredicate left)
        => new(UnaryFactory, BinaryFactory, UnaryFactory.Instantiate("!", left));

    public class PredicateRightCombiner
    {
        protected UnaryOperatorFactory UnaryFactory { get; }
        protected BinaryOperatorFactory BinaryFactory { get; }
        private IPredicate State { get; }

        internal PredicateRightCombiner(UnaryOperatorFactory unaryFactory, BinaryOperatorFactory binaryFactory, IPredicate state)
            => (UnaryFactory, BinaryFactory, State) = (unaryFactory, binaryFactory, state);

        public PredicateRightCombiner Or(IPredicate right)
            => new(UnaryFactory, BinaryFactory, BinaryFactory.Instantiate("OR", State, right));

        public PredicateRightCombiner OrNot(IPredicate right)
            => Or(UnaryFactory.Instantiate("!", right));

        public PredicateRightCombiner And(IPredicate right)
            => new(UnaryFactory, BinaryFactory, BinaryFactory.Instantiate("AND", State, right));

        public PredicateRightCombiner AndNot(IPredicate right)
            => And(UnaryFactory.Instantiate("!", right));

        public PredicateRightCombiner Xor(IPredicate right)
            => new(UnaryFactory, BinaryFactory, BinaryFactory.Instantiate("XOR", State, right));

        public PredicateRightCombiner XorNot(IPredicate right)
            => Xor(UnaryFactory.Instantiate("!", right));

        public IPredicate Build()
                => State!;
    }
}
