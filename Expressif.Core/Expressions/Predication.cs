using Expressif.Functions;
using Expressif.Predicates;

namespace Expressif;

public class Predication : IPredicate
{
    private readonly IPredicate predicate;
    private readonly EvaluationContext context;

    public Predication(IPredicate predicate)
        : this(predicate, EvaluationContext.Empty) { }

    private Predication(IPredicate predicate, EvaluationContext context)
        => (this.predicate, this.context) = (
            predicate ?? throw new ArgumentNullException(nameof(predicate)),
            context);

    public virtual bool Evaluate(object? value)
    {
        using var scope = EvaluationRuntime.Enter(new EvaluationFrame(value, value), context);
        return predicate.Evaluate(value);
    }

    public Predication WithContext(EvaluationContext context)
        => new(predicate, context ?? throw new ArgumentNullException(nameof(context)));

    object? IFunction.Evaluate(object? value) => Evaluate(value);
}
