using Expressif.Functions;
using Expressif.Predicates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif;

public class Predication : IPredicate
{
    private readonly IPredicate predicate;

    public Predication(string code, IContext? context = null, PredicationFactory? factory = null)
        => predicate = (factory ?? new()).Instantiate(code, context ?? new Context());

    public virtual bool Evaluate(object? value) => predicate.Evaluate(value)!;

    object? IFunction.Evaluate(object? value) => predicate.Evaluate(value);
}
