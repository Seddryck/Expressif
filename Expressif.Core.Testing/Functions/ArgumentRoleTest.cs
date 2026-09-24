using Expressif.Discovery;
using Expressif.Functions;
using Expressif.Functions.Accumulation;
using Expressif.Predicates;

namespace Expressif.Testing.Functions;

public class ArgumentRoleTest
{
    [Test]
    public void Discovery_AcceptsSemanticProviderRoles()
    {
        var registry = new FunctionConstructorRegistry(new FixedTypeSource(
            typeof(PredicateRole), typeof(AccumulatorRole), typeof(TransformationRole)));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(registry.TryGetRoleAnnotated(typeof(PredicateRole), out _), Is.True);
            Assert.That(registry.TryGetRoleAnnotated(typeof(AccumulatorRole), out _), Is.True);
            Assert.That(registry.TryGetRoleAnnotated(typeof(TransformationRole), out _), Is.True);
        }
    }

    [TestCase(typeof(WrongRoleShape), "Predicate requires")]
    [TestCase(typeof(MissingRole), "every parameter")]
    [TestCase(typeof(InconsistentOverloads), "same semantic role")]
    public void Discovery_RejectsInvalidRoleMetadata(Type type, string reason)
        => Assert.That(() => new FunctionConstructorRegistry(new FixedTypeSource(type)),
            Throws.TypeOf<InvalidOperationException>()
                .With.Message.Contains(type.FullName!).And.Message.Contains(reason));

    private sealed class FixedTypeSource(params Type[] types) : ITypeSource
    {
        public IEnumerable<Type> GetTypes() => types;
    }

    public sealed class PredicateRole([ArgumentRole(ArgumentRole.Predicate)] Func<IPredicate> provider) : IFunction
    {
        public object? Evaluate(object? value) => provider().Evaluate(value);
    }

    public sealed class AccumulatorRole([ArgumentRole(ArgumentRole.Accumulator)] Func<IAccumulator> provider) : IFunction
    {
        public object? Evaluate(object? value) => provider();
    }

    public sealed class TransformationRole([ArgumentRole(ArgumentRole.Transformation)] Func<IFunction> provider) : IFunction
    {
        public object? Evaluate(object? value) => provider().Evaluate(value);
    }

    public sealed class WrongRoleShape([ArgumentRole(ArgumentRole.Predicate)] Func<IFunction> provider) : IFunction
    {
        public object? Evaluate(object? value) => provider().Evaluate(value);
    }

    public sealed class MissingRole : IFunction
    {
        public MissingRole([ArgumentRole(ArgumentRole.Predicate)] Func<IPredicate> predicate, Func<IFunction> transform) { }
        public object? Evaluate(object? value) => value;
    }

    public sealed class InconsistentOverloads : IFunction
    {
        public InconsistentOverloads([ArgumentRole(ArgumentRole.Predicate)] Func<IPredicate> source) { }
        public InconsistentOverloads([ArgumentRole(ArgumentRole.Transformation)] Func<IFunction> source) { }
        public object? Evaluate(object? value) => value;
    }
}
