using Expressif.Bindings;
using Expressif.Discovery;
using Expressif.Functions;

namespace Expressif.Testing.Functions;

public class AcceptedExpressionShapeTest
{
    [Test]
    public void Discovery_AcceptsDirectFieldAndOpenExpressionShapes()
    {
        var registry = new FunctionConstructorRegistry(new FixedTypeSource(
            typeof(DirectField), typeof(OpenExpression)));

        Assert.That(registry.TryGetShapeAnnotated(typeof(DirectField), out _), Is.True);
        Assert.That(registry.TryGetShapeAnnotated(typeof(OpenExpression), out _), Is.True);
    }

    [TestCase(typeof(WrongShape), "DirectFieldSelector requires")]
    [TestCase(typeof(WrongCallableShape), "CallableReference requires")]
    [TestCase(typeof(InconsistentOverloads), "same expression shape")]
    public void Discovery_RejectsInvalidShapeMetadata(Type type, string reason)
        => Assert.That(() => new FunctionConstructorRegistry(new FixedTypeSource(type)),
            Throws.TypeOf<InvalidOperationException>()
                .With.Message.Contains(type.FullName!).And.Message.Contains(reason));

    private sealed class FixedTypeSource(params Type[] types) : ITypeSource
    {
        public IEnumerable<Type> GetTypes() => types;
    }

    public sealed class DirectField(
        [AcceptedExpressionShape(AcceptedExpressionShape.DirectFieldSelector)] NamedFieldSelector selector) : IFunction
    {
        public object? Evaluate(object? value) => selector.Evaluate(value);
    }

    public sealed class OpenExpression(
        [AcceptedExpressionShape(AcceptedExpressionShape.OpenExpression)] Func<IFunction> expression) : IFunction
    {
        public object? Evaluate(object? value) => expression().Evaluate(value);
    }

    public sealed class WrongShape(
        [AcceptedExpressionShape(AcceptedExpressionShape.DirectFieldSelector)] Func<IFunction> expression) : IFunction
    {
        public object? Evaluate(object? value) => expression().Evaluate(value);
    }

    public sealed class WrongCallableShape(
        [AcceptedExpressionShape(AcceptedExpressionShape.CallableReference)] string comparer) : IFunction
    {
        public object? Evaluate(object? value) => comparer;
    }

    public sealed class InconsistentOverloads : IFunction
    {
        public InconsistentOverloads(
            [AcceptedExpressionShape(AcceptedExpressionShape.DirectFieldSelector)] NamedFieldSelector selector) { }
        public InconsistentOverloads(
            [AcceptedExpressionShape(AcceptedExpressionShape.OpenExpression)] Func<IFunction> selector) { }
        public object? Evaluate(object? value) => value;
    }
}
