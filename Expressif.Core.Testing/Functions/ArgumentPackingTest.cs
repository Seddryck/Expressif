using Expressif.Discovery;
using Expressif.Functions;

namespace Expressif.Testing.Functions;

public class ArgumentPackingTest
{
    [Test]
    public void Discovery_UsesParameterRatherThanFunctionMetadata()
    {
        var registry = Discover(typeof(ValidSpread));

        Assert.That(registry.TryGetVariadicPacked(typeof(ValidSpread), out var constructor), Is.True);
        Assert.That(constructor.GetParameters()[0].Name, Is.EqualTo("items"));
        Assert.That(Discover(typeof(TypedIntegers)).TryGetVariadicPacked(typeof(TypedIntegers), out _), Is.True);
        Assert.That(Discover(typeof(TypedStrings)).TryGetVariadicPacked(typeof(TypedStrings), out _), Is.True);
    }

    [TestCase(typeof(SpreadOnSingle), "spread requires variadic")]
    [TestCase(typeof(WrongDelegate), "Func<object?, object?[]>")]
    [TestCase(typeof(MultipleVariadic), "only one positional variadic")]
    [TestCase(typeof(AdditionalParameter), "single-parameter constructor")]
    [TestCase(typeof(TypedSpread), "Func<object?, object?[]>")]
    [TestCase(typeof(InvalidTypedProvider), "Func<T[]>")]
    [TestCase(typeof(AmbiguousTypedProviders), "ambiguous variadic constructors")]
    public void Discovery_RejectsInvalidPacking(Type type, string reason)
        => Assert.That(() => Discover(type),
            Throws.TypeOf<InvalidOperationException>()
                .With.Message.Contains(type.FullName!).And.Message.Contains(reason));

    private static FunctionConstructorRegistry Discover(Type type) => new(new FixedTypeSource(type));

    private sealed class FixedTypeSource(params Type[] types) : ITypeSource
    {
        public IEnumerable<Type> GetTypes() => types;
    }

    public sealed class ValidSpread(
        [ArgumentPacking(ArgumentPackingMode.Variadic, AllowSpread = true)] Func<object?, object?[]> items) : IFunction
    {
        public object? Evaluate(object? input) => items(input);
    }

    public sealed class TypedIntegers(
        [ArgumentPacking(ArgumentPackingMode.Variadic)] Func<int[]> items) : IFunction
    {
        public object? Evaluate(object? input) => items();
    }

    public sealed class TypedStrings(
        [ArgumentPacking(ArgumentPackingMode.Variadic)] Func<string[]> items) : IFunction
    {
        public object? Evaluate(object? input) => items();
    }

    public sealed class TypedSpread(
        [ArgumentPacking(ArgumentPackingMode.Variadic, AllowSpread = true)] Func<int[]> items) : IFunction
    {
        public object? Evaluate(object? input) => items();
    }

    public sealed class InvalidTypedProvider(
        [ArgumentPacking(ArgumentPackingMode.Variadic)] Func<int> item) : IFunction
    {
        public object? Evaluate(object? input) => item();
    }

    public sealed class AmbiguousTypedProviders : IFunction
    {
        public AmbiguousTypedProviders(
            [ArgumentPacking(ArgumentPackingMode.Variadic)] Func<int[]> items) { }

        public AmbiguousTypedProviders(
            [ArgumentPacking(ArgumentPackingMode.Variadic)] Func<string[]> items) { }

        public object? Evaluate(object? input) => input;
    }

    public sealed class SpreadOnSingle(
        [ArgumentPacking(ArgumentPackingMode.Single, AllowSpread = true)] Func<object?, object?[]> items) : IFunction
    {
        public object? Evaluate(object? input) => items(input);
    }

    public sealed class WrongDelegate(
        [ArgumentPacking(ArgumentPackingMode.Variadic, AllowSpread = true)] Func<object?, object?> item) : IFunction
    {
        public object? Evaluate(object? input) => item(input);
    }

    public sealed class MultipleVariadic : IFunction
    {
        public MultipleVariadic(
            [ArgumentPacking(ArgumentPackingMode.Variadic)] Func<object?, object?[]> first,
            [ArgumentPacking(ArgumentPackingMode.Variadic)] Func<object?, object?[]> second) { }
        public object? Evaluate(object? input) => input;
    }

    public sealed class AdditionalParameter : IFunction
    {
        public AdditionalParameter(
            [ArgumentPacking(ArgumentPackingMode.Variadic, AllowSpread = true)] Func<object?, object?[]> items,
            int count) { }
        public object? Evaluate(object? input) => input;
    }
}
