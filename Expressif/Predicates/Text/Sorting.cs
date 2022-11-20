using Expressif.Values;
using Expressif.Values.Casters;
using Expressif.Values.Special;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Text
{
    internal class EquivalentTo : BaseTextPredicateReference
    {
        protected StringComparer Comparer { get; }

        public EquivalentTo(IScalarResolver<string> reference)
            : this(reference, StringComparer.InvariantCultureIgnoreCase) { }

        public EquivalentTo(IScalarResolver<string> reference, StringComparer comparer)
            : base(reference) { Comparer = comparer; }

        protected override bool EvaluateText(string value, string reference)
            => Comparer.Compare(value, reference) == 0;
    }

    internal class SortedAfter : EquivalentTo
    {
        public SortedAfter(IScalarResolver<string> reference)
            : this(reference, StringComparer.InvariantCultureIgnoreCase) { }

        public SortedAfter(IScalarResolver<string> reference, StringComparer comparer)
            : base(reference, comparer) { }

        protected override bool EvaluateText(string value, string reference)
            => Comparer.Compare(value, reference) >0;
    }

    internal class SortedAfterOrEquivalentTo : EquivalentTo
    {
        public SortedAfterOrEquivalentTo(IScalarResolver<string> reference)
            : this(reference, StringComparer.InvariantCultureIgnoreCase) { }

        public SortedAfterOrEquivalentTo(IScalarResolver<string> reference, StringComparer comparer)
            : base(reference, comparer) { }

        protected override bool EvaluateText(string value, string reference)
            => Comparer.Compare(value, reference) >= 0;
    }

    internal class SortedBefore : EquivalentTo
    {
        public SortedBefore(IScalarResolver<string> reference)
            : this(reference, StringComparer.InvariantCultureIgnoreCase) { }

        public SortedBefore(IScalarResolver<string> reference, StringComparer comparer)
            : base(reference, comparer) { }

        protected override bool EvaluateText(string value, string reference)
            => Comparer.Compare(value, reference) < 0;
    }

    internal class SortedBeforeOrEquivalentTo : EquivalentTo
    {
        public SortedBeforeOrEquivalentTo(IScalarResolver<string> reference)
            : this(reference, StringComparer.InvariantCultureIgnoreCase) { }

        public SortedBeforeOrEquivalentTo(IScalarResolver<string> reference, StringComparer comparer)
            : base(reference, comparer) { }

        protected override bool EvaluateText(string value, string reference)
            => Comparer.Compare(value, reference) <= 0;
    }
}
