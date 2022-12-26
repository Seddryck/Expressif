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

    /// <summary>
    /// Compare the text value passed as argument and the text value passed as parameter and returns `true` if they are equal. By default the comparison is agnostic of the culture and case-insensitive.
    /// </summary>
    public class EquivalentTo : BaseTextPredicateReference
    {
        protected StringComparer Comparer { get; }

        /// <param name="reference">A string to be compared to the argument value</param>
        public EquivalentTo(Func<string> reference)
            : this(reference, StringComparer.InvariantCultureIgnoreCase) { }

        /// <param name="reference">A string to be compared to the argument value</param>
        /// <param name="comparer">A definition of the parameters of the comparison (case-sensitivity, culture-sensitivity)</param>
        public EquivalentTo(Func<string> reference, StringComparer comparer)
            : base(reference) { Comparer = comparer; }

        protected override bool EvaluateText(string value, string reference)
            => Comparer.Compare(value, reference) == 0;
    }

    /// <summary>
    /// Compare the text value passed as argument and the text value passed as parameter and returns `true` if argument value is alphabetically sorted after the parameter value. By default the comparison is agnostic of the culture and case-insensitive.
    /// </summary>
    public class SortedAfter : EquivalentTo
    {
        /// <param name="reference">A string to be compared to the argument value</param>
        public SortedAfter(Func<string> reference)
            : this(reference, StringComparer.InvariantCultureIgnoreCase) { }

        /// <param name="reference">A string to be compared to the argument value</param>
        /// <param name="comparer">A definition of the parameters of the comparison (case-sensitivity, culture-sensitivity)</param>
        public SortedAfter(Func<string> reference, StringComparer comparer)
            : base(reference, comparer) { }

        protected override bool EvaluateText(string value, string reference)
            => Comparer.Compare(value, reference) >0;
    }

    /// <summary>
    /// Compare the text value passed as argument and the text value passed as parameter and returns `true` if argument value is alphabetically sorted after the parameter value or if the two values are equal. By default the comparison is agnostic of the culture and case-insensitive./// </summary>
    public class SortedAfterOrEquivalentTo : EquivalentTo
    {
        /// <param name="reference">A string to be compared to the argument value</param>
        public SortedAfterOrEquivalentTo(Func<string> reference)
            : this(reference, StringComparer.InvariantCultureIgnoreCase) { }

        /// <param name="reference">A string to be compared to the argument value</param>
        /// <param name="comparer">A definition of the parameters of the comparison (case-sensitivity, culture-sensitivity)</param>
        public SortedAfterOrEquivalentTo(Func<string> reference, StringComparer comparer)
            : base(reference, comparer) { }

        protected override bool EvaluateText(string value, string reference)
            => Comparer.Compare(value, reference) >= 0;
    }

    /// <summary>
    /// Compare the text value passed as argument and the text value passed as parameter and returns `true` if argument value is alphabetically sorted before the parameter value. By default the comparison is agnostic of the culture and case-insensitive.
    /// </summary>
    public class SortedBefore : EquivalentTo
    {
        /// <param name="reference">A string to be compared to the argument value</param>
        public SortedBefore(Func<string> reference)
            : this(reference, StringComparer.InvariantCultureIgnoreCase) { }

        /// <param name="reference">A string to be compared to the argument value</param>
        /// <param name="comparer">A definition of the parameters of the comparison (case-sensitivity, culture-sensitivity)</param>
        public SortedBefore(Func<string> reference, StringComparer comparer)
            : base(reference, comparer) { }

        protected override bool EvaluateText(string value, string reference)
            => Comparer.Compare(value, reference) < 0;
    }

    /// <summary>
    /// Compare the text value passed as argument and the text value passed as parameter and returns `true` if argument value is alphabetically sorted before the parameter value or if the two values are equal. By default the comparison is agnostic of the culture and case-insensitive.
    /// </summary>
    public class SortedBeforeOrEquivalentTo : EquivalentTo
    {
        /// <param name="reference">A string to be compared to the argument value</param>
        public SortedBeforeOrEquivalentTo(Func<string> reference)
            : this(reference, StringComparer.InvariantCultureIgnoreCase) { }

        /// <param name="reference">A string to be compared to the argument value</param>
        /// <param name="comparer">A definition of the parameters of the comparison (case-sensitivity, culture-sensitivity)</param>
        public SortedBeforeOrEquivalentTo(Func<string> reference, StringComparer comparer)
            : base(reference, comparer) { }

        protected override bool EvaluateText(string value, string reference)
            => Comparer.Compare(value, reference) <= 0;
    }
}
