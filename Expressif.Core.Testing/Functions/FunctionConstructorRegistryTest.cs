using Expressif.Discovery;
using Expressif.Functions;

namespace Expressif.Testing.Functions;

public sealed class FunctionConstructorRegistryTest
{
    [Test]
    public void RegistryAssociatesConstructorWithEveryDeclaredFunctionType()
    {
        var constructor = new SharedConstructor();
        var registry = new FunctionConstructorRegistry([constructor]);

        Assert.Multiple(() =>
        {
            Assert.That(registry.TryGet(typeof(FirstFunction), out var first), Is.True);
            Assert.That(first, Is.SameAs(constructor));
            Assert.That(registry.TryGet(typeof(SecondFunction), out var second), Is.True);
            Assert.That(second, Is.SameAs(constructor));
            Assert.That(registry.TryGet(typeof(UnknownFunction), out _), Is.False);
        });
    }

    [Test]
    public void RegistryDiscoversConstructorsFromTypeSource()
    {
        var source = Mock.Of<ITypeSource>(candidate => candidate.GetTypes() == new[] { typeof(FirstConstructor) });

        var registry = new FunctionConstructorRegistry(source);

        Assert.That(registry.TryGet(typeof(FirstFunction), out var constructor), Is.True);
        Assert.That(constructor, Is.TypeOf<FirstConstructor>());
    }

    [Test]
    public void RegistryRejectsConstructorWithoutGenericAssociation()
        => Assert.That(
            () => new FunctionConstructorRegistry([new UnassociatedConstructor()]),
            Throws.InvalidOperationException.With.Message.Contains("does not declare"));

    [Test]
    public void RegistryRejectsDuplicateFunctionAssociation()
        => Assert.That(
            () => new FunctionConstructorRegistry([new SharedConstructor(), new FirstConstructor()]),
            Throws.InvalidOperationException.With.Message.Contains("already registered"));

    private sealed class SharedConstructor : StubConstructor,
        IFunctionConstructor<FirstFunction>,
        IFunctionConstructor<SecondFunction>;

    private sealed class FirstConstructor : StubConstructor,
        IFunctionConstructor<FirstFunction>;

    private sealed class UnassociatedConstructor : StubConstructor;

    private abstract class StubConstructor : IFunctionConstructor
    {
        public IFunction Construct(
            Expressif.Bindings.Function function,
            IContext context,
            IFunctionConstructionContext constructionContext)
            => new FirstFunction();
    }

    private sealed class FirstFunction : IFunction
    {
        public object? Evaluate(object? value) => value;
    }

    private sealed class SecondFunction : IFunction
    {
        public object? Evaluate(object? value) => value;
    }

    private sealed class UnknownFunction : IFunction
    {
        public object? Evaluate(object? value) => value;
    }
}
