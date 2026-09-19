using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions;
using Expressif.Predicates;

namespace Expressif;

public class Predication : IPredicate
{
    private readonly IPredicate predicate;
    private readonly EvaluationContext context;

    public static Predication Create(string text)
        => new Predication(new PredicationFactory().Instantiate(text, new Context()));

    public static Predication Create(string text, IContext context)
        => new Predication(new PredicationFactory().Instantiate(text, context));

    public Predication(string code)
        : this(code, new Context()) { }
    public Predication(string code, IContext context)
        : this(code, context, new PredicationFactory()) { }
    public Predication(string code, IContext context, PredicationFactory factory)
        : this(factory.Instantiate(code, context)) { }

    internal Predication(IPredicate predicate)
        : this(predicate, EvaluationContext.Empty) { }

    private Predication(IPredicate predicate, EvaluationContext context)
        => (this.predicate, this.context) = (predicate, context);

    public virtual bool Evaluate(object? value)
    {
        using var scope = EvaluationRuntime.Enter(new EvaluationFrame(value, value), context);
        return predicate.Evaluate(value);
    }

    public Predication WithContext(EvaluationContext context)
        => new Predication(predicate, context ?? throw new ArgumentNullException(nameof(context)));

    object? IFunction.Evaluate(object? value) => Evaluate(value);
}
