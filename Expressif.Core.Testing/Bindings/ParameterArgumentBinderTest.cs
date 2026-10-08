using Expressif.Bindings;

namespace Expressif.Testing.Bindings;

public class ParameterArgumentBinderTest
{
    [Test]
    public void Bind_AmbiguousConstructors_ThrowsSpecificException()
    {
        var arguments = new[] { new FunctionArgument("value", new LiteralParameter(1)) };

        Assert.That(
            () => ParameterArgumentBinder.Bind(typeof(AmbiguousConstructors), arguments),
            Throws.TypeOf<AmbiguousParameterBindingException>());
    }

    [Test]
    public void Bind_IncompatibleNamedArguments_ThrowsSpecificException()
    {
        FunctionArgument[] arguments =
        [
            new(null, new LiteralParameter(1)),
            new("second", new LiteralParameter(2)),
            new("third", new LiteralParameter(3)),
        ];

        Assert.That(
            () => ParameterArgumentBinder.Bind(typeof(IncompatibleConstructors), arguments),
            Throws.TypeOf<AmbiguousParameterBindingException>());
    }

    [Test]
    public void Bind_DuplicateNamedArguments_ThrowsSpecificException()
    {
        FunctionArgument[] arguments =
        [
            new("value", new LiteralParameter(1)),
            new("value", new LiteralParameter(2)),
        ];

        Assert.That(
            () => ParameterArgumentBinder.Bind(typeof(SingleConstructor), arguments),
            Throws.TypeOf<DuplicateNamedArgumentException>());
    }

    [Test]
    public void Bind_OmittedOptionalParameter_UsesDefaultValue()
    {
        var binding = ParameterArgumentBinder.Bind(typeof(OptionalParameter), []);

        Assert.Multiple(() =>
        {
            Assert.That(binding.Parameters, Is.EqualTo(new[] { new LiteralParameter(null) }));
            Assert.That(binding.Supplied, Is.EqualTo(new[] { false }));
        });
    }

    [Test]
    public void Bind_ExactArityWinsOverOverloadWithOptionalCallback()
    {
        var arguments = new[] { new FunctionArgument(null, new LiteralParameter(1)) };

        var binding = ParameterArgumentBinder.Bind(typeof(ExactAndOptional), arguments);

        Assert.That(binding.Constructor.GetParameters(), Has.Length.EqualTo(1));
    }

    private sealed class AmbiguousConstructors
    {
        public AmbiguousConstructors(Func<int> value) { }
        public AmbiguousConstructors(Func<string> value) { }
    }

    private sealed class IncompatibleConstructors
    {
        public IncompatibleConstructors(Func<int> first, Func<int> second, Func<int>? optional = null) { }
        public IncompatibleConstructors(Func<string> first, Func<int> third, Func<int>? optional = null) { }
    }

    private sealed class SingleConstructor
    {
        public SingleConstructor(Func<int> value) { }
    }

    private sealed class OptionalParameter
    {
        public OptionalParameter(Func<int>? value = null) { }
    }

    private sealed class ExactAndOptional
    {
        public ExactAndOptional(Func<int> value) { }
        public ExactAndOptional(Func<int> value, Func<int>? other = null) { }
    }
}
