using Expressif.Bindings;
using Expressif.Functions;
using Expressif.Syntax;
using NUnit.Framework;

namespace Expressif.Testing.Bindings;

public sealed class FunctionBinderRegistryTest
{
    [Test]
    public void RegistryAssociatesBinderWithEachDeclaredFunctionType()
    {
        var binder = new SampleBinder();
        var registry = new FunctionBinderRegistry([binder]);

        Assert.Multiple(() =>
        {
            Assert.That(registry.TryGet(typeof(FirstFunction), out var first), Is.True);
            Assert.That(first, Is.SameAs(binder));
            Assert.That(registry.TryGet(typeof(SecondFunction), out var second), Is.True);
            Assert.That(second, Is.SameAs(binder));
        });
    }

    [Test]
    public void RegistryDiscoversBindersFromAssembly()
    {
        var registry = new FunctionBinderRegistry(typeof(FunctionBinderRegistryTest).Assembly);

        Assert.That(registry.TryGet(typeof(FirstFunction), out var binder), Is.True);
        Assert.That(binder, Is.TypeOf<SampleBinder>());
    }

    [Test]
    public void RegistryRejectsDuplicateFunctionAssociations()
        => Assert.That(
            () => new FunctionBinderRegistry([new SampleBinder(), new SampleBinder()]),
            Throws.InvalidOperationException.With.Message.EqualTo(
                $"A function binder is already registered for '{typeof(FirstFunction).FullName}'."));

    private sealed class SampleBinder :
        IFunctionBinder<FirstFunction>,
        IFunctionBinder<SecondFunction>
    {
        public Function Bind(FunctionCallSyntax syntax, IFunctionBindingContext context)
            => new(syntax.Name, []);
    }

    private sealed class FirstFunction : IFunction
    {
        public object? Evaluate(object? value) => value;
    }

    private sealed class SecondFunction : IFunction
    {
        public object? Evaluate(object? value) => value;
    }
}
