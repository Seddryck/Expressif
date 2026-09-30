using Expressif.Bindings;
using Expressif.Discovery;
using Expressif.Functions;
using Expressif.Functions.Accumulation;
using Expressif.Predicates;

namespace Expressif.Testing.Functions;

public class ArgumentContractTest
{
    [Test]
    public void Discovery_AcceptsEmptyVariadicAbsentAndFreshProvider()
    {
        var registry = new FunctionConstructorRegistry(new FixedTypeSource(
            typeof(EmptyValues), typeof(AbsentCallback), typeof(FreshAccumulator)));

        Assert.That(registry.TryGetVariadicPacked(typeof(EmptyValues), out _), Is.True);
        Assert.That(registry.TryGetAnnotated(typeof(AbsentCallback), out _), Is.True);
        Assert.That(registry.TryGetRoleAnnotated(typeof(FreshAccumulator), out _), Is.True);
    }

    [TestCase(typeof(EmptyWithoutPacking), "requires variadic packing")]
    [TestCase(typeof(AbsentRequired), "optional nullable")]
    [TestCase(typeof(StableAccumulator), "accumulator providers must be fresh")]
    [TestCase(typeof(LifetimeWithoutRole), "semantic-role provider delegate")]
    public void Discovery_RejectsIncompatibleContracts(Type type, string reason)
        => Assert.That(() => new FunctionConstructorRegistry(new FixedTypeSource(type)),
            Throws.TypeOf<InvalidOperationException>()
                .With.Message.Contains(type.FullName!).And.Message.Contains(reason));

    [Test]
    public void BoundExpressionLifetime_ReusesProviderResultWhileFreshDoesNot()
    {
        var stable = Instantiate("stable-predicate-probe", typeof(StablePredicateProbe), "even");
        var fresh = Instantiate("fresh-predicate-probe", typeof(FreshPredicateProbe), "even");

        Assert.That(stable.Evaluate(2), Is.True);
        Assert.That(fresh.Evaluate(2), Is.False);
    }

    [Test]
    public void AccumulatorProvider_CreatesIndependentInstances()
    {
        var runtime = Instantiate("fresh-accumulator-probe", typeof(FreshAccumulator), "sum");

        Assert.That(runtime.Evaluate(null), Is.False);
        Assert.That(runtime.Evaluate(null), Is.False);
    }

    private static IFunction Instantiate(string name, Type type, string argument)
    {
        var registry = new ImplementationRegistry([new(name, type)]);
        var function = new Function(name, [new LiteralParameter(argument)]);
        return new FunctionFactory(registry, TestExpression.LibraryTypeSource)
            .Instantiate(new OpenRootExpression(new OpenExpression([function])), new Context());
    }

    private sealed class FixedTypeSource(params Type[] types) : ITypeSource
    {
        public IEnumerable<Type> GetTypes() => types;
    }

    public sealed class EmptyValues(
        [ArgumentPacking(ArgumentPackingMode.Variadic, AllowSpread = true)]
        [ArgumentOmission(ArgumentOmissionMode.EmptyVariadic)] Func<object?, object?[]> values) : IFunction
    {
        public object? Evaluate(object? value) => values(value);
    }

    public sealed class EmptyWithoutPacking(
        [ArgumentOmission(ArgumentOmissionMode.EmptyVariadic)] Func<object?, object?[]> values) : IFunction
    {
        public object? Evaluate(object? value) => values(value);
    }

    public sealed class AbsentCallback(
        [ArgumentEvaluation(ArgumentEvaluationMode.Incoming)]
        [ArgumentOmission(ArgumentOmissionMode.Absent)] Func<object?, object?>? callback = null) : IFunction
    {
        public object? Evaluate(object? value) => callback?.Invoke(value);
    }

    public sealed class AbsentRequired(
        [ArgumentOmission(ArgumentOmissionMode.Absent)] Func<object?, object?>? callback) : IFunction
    {
        public object? Evaluate(object? value) => callback?.Invoke(value);
    }

    public sealed class FreshAccumulator(
        [ArgumentRole(ArgumentRole.Accumulator)]
        [ProviderLifetime(ProviderLifetime.FreshPerRequest)] Func<IAccumulator> provider) : IFunction
    {
        public object? Evaluate(object? value) => ReferenceEquals(provider(), provider());
    }

    public sealed class StableAccumulator(
        [ArgumentRole(ArgumentRole.Accumulator)]
        [ProviderLifetime(ProviderLifetime.BoundExpression)] Func<IAccumulator> provider) : IFunction
    {
        public object? Evaluate(object? value) => provider();
    }

    public sealed class LifetimeWithoutRole(
        [ProviderLifetime(ProviderLifetime.FreshPerRequest)] Func<IPredicate> provider) : IFunction
    {
        public object? Evaluate(object? value) => provider().Evaluate(value);
    }

    public sealed class StablePredicateProbe(
        [ArgumentRole(ArgumentRole.Predicate)]
        [ProviderLifetime(ProviderLifetime.BoundExpression)] Func<IPredicate> provider) : IFunction
    {
        public object? Evaluate(object? value) => ReferenceEquals(provider(), provider());
    }

    public sealed class FreshPredicateProbe(
        [ArgumentRole(ArgumentRole.Predicate)]
        [ProviderLifetime(ProviderLifetime.FreshPerRequest)] Func<IPredicate> provider) : IFunction
    {
        public object? Evaluate(object? value) => ReferenceEquals(provider(), provider());
    }
}
