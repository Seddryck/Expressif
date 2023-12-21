using Expressif.Functions;
using Expressif.Values;
using Expressif.Values.Casters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Predicates;

[Predicate]
public abstract class BasePredicate : IPredicate
{
    public BasePredicate() { }
    public abstract bool Evaluate(object? value);
    protected virtual bool EvaluateNull() => false;
    object? IFunction.Evaluate(object? value) => Evaluate(value);
}
