using Expressif.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates.Boolean;

internal class Negate : IPredicate
{
    private IPredicate InnerPredicate { get; }

    public Negate(IPredicate predicate)
        => InnerPredicate = predicate;

    public bool Evaluate(object? value) => !InnerPredicate.Evaluate(value);
    object? IFunction.Evaluate(object? value) => Evaluate(value);
}
