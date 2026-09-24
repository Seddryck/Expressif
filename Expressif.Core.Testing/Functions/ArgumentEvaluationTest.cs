using Expressif.Discovery;
using Expressif.Functions;

namespace Expressif.Testing.Functions;

public class ArgumentEvaluationTest
{
    [Test]
    public void Discovery_AcceptsIncomingValueCallbacks()
    {
        var registry = Discover(typeof(ValidIncoming));

        Assert.That(registry.TryGetAnnotated(typeof(ValidIncoming), out var constructors), Is.True);
        Assert.That(constructors, Has.Length.EqualTo(1));
        Assert.That(Discover(typeof(OptionalNullable)).TryGetAnnotated(typeof(OptionalNullable), out _), Is.True);
        Assert.That(Discover(typeof(ValidNested)).TryGetAnnotated(typeof(ValidNested), out _), Is.True);
        Assert.That(Discover(typeof(ValidAmbient)).TryGetAnnotated(typeof(ValidAmbient), out _), Is.True);
    }

    [TestCase(typeof(MissingMode), "every constructor parameter")]
    [TestCase(typeof(ZeroInput), "one-input")]
    [TestCase(typeof(NonDelegate), "one-input")]
    [TestCase(typeof(RequiredNullable), "nullable delegates must be optional")]
    [TestCase(typeof(InvalidNestedShape), "one-input")]
    [TestCase(typeof(InvalidAmbientShape), "zero-input")]
    [TestCase(typeof(InconsistentOverloads), "same evaluation mode")]
    public void Discovery_RejectsInvalidMetadata(Type type, string reason)
        => Assert.That(() => Discover(type),
            Throws.TypeOf<InvalidOperationException>()
                .With.Message.Contains(type.FullName!).And.Message.Contains(reason));

    private static FunctionConstructorRegistry Discover(Type type)
        => new(new FixedTypeSource(type));

    private sealed class FixedTypeSource(params Type[] types) : ITypeSource
    {
        public IEnumerable<Type> GetTypes() => types;
    }

    public sealed class ValidIncoming(
        [ArgumentEvaluation(ArgumentEvaluationMode.Incoming)] Func<object?, object?> callback) : IFunction
    {
        public object? Evaluate(object? value) => callback(value);
    }

    public sealed class MissingMode : IFunction
    {
        public MissingMode(
            [ArgumentEvaluation(ArgumentEvaluationMode.Incoming)] Func<object?, object?> first,
            Func<object?, object?> second) { }

        public object? Evaluate(object? value) => value;
    }

    public sealed class ZeroInput(
        [ArgumentEvaluation(ArgumentEvaluationMode.Incoming)] Func<object?> callback) : IFunction
    {
        public object? Evaluate(object? value) => callback();
    }

    public sealed class NonDelegate(
        [ArgumentEvaluation(ArgumentEvaluationMode.Incoming)] object callback) : IFunction
    {
        public object? Evaluate(object? value) => callback;
    }

    public sealed class RequiredNullable(
        [ArgumentEvaluation(ArgumentEvaluationMode.Incoming)] Func<object?, object?>? callback) : IFunction
    {
        public object? Evaluate(object? value) => callback?.Invoke(value);
    }

    public sealed class ValidNested(
        [ArgumentEvaluation(ArgumentEvaluationMode.Nested)] Func<object?, object?> callback) : IFunction
    {
        public object? Evaluate(object? value) => callback(value);
    }

    public sealed class ValidAmbient(
        [ArgumentEvaluation(ArgumentEvaluationMode.Ambient)] Func<object?> callback) : IFunction
    {
        public object? Evaluate(object? value) => callback();
    }

    public sealed class InvalidNestedShape(
        [ArgumentEvaluation(ArgumentEvaluationMode.Nested)] Func<object?> callback) : IFunction
    {
        public object? Evaluate(object? value) => callback();
    }

    public sealed class InvalidAmbientShape(
        [ArgumentEvaluation(ArgumentEvaluationMode.Ambient)] Func<object?, object?> callback) : IFunction
    {
        public object? Evaluate(object? value) => callback(value);
    }

    public sealed class OptionalNullable(
        [ArgumentEvaluation(ArgumentEvaluationMode.Incoming)] Func<object?, object?>? callback = null) : IFunction
    {
        public object? Evaluate(object? value) => callback?.Invoke(value);
    }

    public sealed class InconsistentOverloads : IFunction
    {
        private readonly Func<object?, object?> callback;

        public InconsistentOverloads(
            [ArgumentEvaluation(ArgumentEvaluationMode.Incoming)] Func<object?, object?> callback)
            => this.callback = callback;

        public InconsistentOverloads(
            [ArgumentEvaluation(ArgumentEvaluationMode.Nested)] Func<object?, object?> callback,
            [ArgumentEvaluation(ArgumentEvaluationMode.Incoming)] Func<object?, object?> other)
            => this.callback = callback;

        public object? Evaluate(object? value) => callback(value);
    }
}
