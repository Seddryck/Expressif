using Expressif.Values;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Text;

/// <summary>
/// Returns `true` if the list of text values passed as parameter contains the text value passed as argument. Returns `false` otherwise.
/// </summary>
public class AnyOf : BaseTextPredicate
{
    public Func<IEnumerable<string>> References { get; }
    protected StringComparer Comparer { get; }

    /// <param name="references">An array of text values.</param>
    public AnyOf(Func<IEnumerable<string>> references)
        : this(references, StringComparer.InvariantCultureIgnoreCase) { }
    
    /// <param name="references"></param>
    /// <param name="comparer"></param>
    public AnyOf(Func<IEnumerable<string>> references, StringComparer comparer)
               => (References, Comparer) = (references, comparer);

    protected override bool EvaluateBaseText(string value)
    {
        foreach (var reference in References.Invoke())
        {
            var predicate = new EquivalentTo(() => reference, Comparer);
            if (predicate.Evaluate(value))
                return true;
        }
        return false;
    }
}
