using Expressif.Bindings;
using Expressif.Discovery;
using Expressif.Functions;

namespace Expressif.Testing.Bindings;

public class ArgumentLayoutTest
{
    [Test]
    public void NamedLayout_RequiresNamesAndPreservesCaseSensitiveUniqueness()
    {
        var bound = ParameterArgumentBinder.BindLayout(typeof(Named), [NamedArgument("A"), NamedArgument("a")]);

        Assert.That(bound.Named.Select(argument => argument.Name), Is.EqualTo(new[] { "A", "a" }));
        Assert.That(() => ParameterArgumentBinder.BindLayout(typeof(Named), [NamedArgument("A"), NamedArgument("A")]),
            Throws.TypeOf<BindingException>().With.Message.Contains("Duplicate named argument"));
        Assert.That(() => ParameterArgumentBinder.BindLayout(typeof(Named), [PositionalArgument()]),
            Throws.TypeOf<BindingException>().With.Message.Contains("named arguments"));
    }

    [Test]
    public void PositionalLayout_RejectsNamedSpreadAndMissingArguments()
    {
        Assert.That(() => ParameterArgumentBinder.BindLayout(typeof(Positional), []),
            Throws.TypeOf<MissingOrUnexpectedParametersFunctionException>());
        Assert.That(() => ParameterArgumentBinder.BindLayout(typeof(Positional), [NamedArgument("x")]),
            Throws.TypeOf<MissingOrUnexpectedParametersFunctionException>());
        Assert.That(() => ParameterArgumentBinder.BindLayout(typeof(Positional),
                [new FunctionArgument(null, new LiteralParameter(1), true)]),
            Throws.TypeOf<MissingOrUnexpectedParametersFunctionException>());
    }

    [Test]
    public void MixedLayout_RequiresPositionalPrefixThenNamedSuffix()
    {
        var bound = ParameterArgumentBinder.BindLayout(typeof(Mixed), [PositionalArgument(), NamedArgument("x")]);

        Assert.That(bound.Positional, Has.Length.EqualTo(1));
        Assert.That(bound.Named, Has.Length.EqualTo(1));
        Assert.That(() => ParameterArgumentBinder.BindLayout(typeof(Mixed), [NamedArgument("x"), NamedArgument("y")]),
            Throws.TypeOf<BindingException>().With.Message.Contains("positional"));
        Assert.That(() => ParameterArgumentBinder.BindLayout(typeof(Mixed), [PositionalArgument(), PositionalArgument()]),
            Throws.TypeOf<BindingException>().With.Message.Contains("named"));
    }

    [TestCase(typeof(InvalidBounds), "cardinality bounds")]
    [TestCase(typeof(InvalidPrefix), "positional prefix")]
    [TestCase(typeof(InvalidUniqueness), "name uniqueness")]
    public void Discovery_RejectsInvalidLayoutMetadata(Type type, string reason)
        => Assert.That(() => new FunctionConstructorRegistry(new FixedTypeSource(type)),
            Throws.TypeOf<InvalidOperationException>()
                .With.Message.Contains(type.FullName!).And.Message.Contains(reason));

    private static FunctionArgument PositionalArgument() => new(null, new LiteralParameter(1));
    private static FunctionArgument NamedArgument(string name) => new(name, new LiteralParameter(1));

    private sealed class FixedTypeSource(params Type[] types) : ITypeSource
    {
        public IEnumerable<Type> GetTypes() => types;
    }

    public sealed class Named : IFunction
    {
        [ArgumentLayout(ArgumentLayoutKind.Named, MinimumCardinality = 1, RequireUniqueNames = true)]
        public Named(Func<object?> values) { }
        public object? Evaluate(object? value) => value;
    }

    public sealed class Positional : IFunction
    {
        [ArgumentLayout(ArgumentLayoutKind.Positional, MinimumCardinality = 1)]
        public Positional(Func<object?> values) { }
        public object? Evaluate(object? value) => value;
    }

    public sealed class Mixed : IFunction
    {
        [ArgumentLayout(ArgumentLayoutKind.PositionalThenNamed, PositionalPrefix = 1,
            MinimumCardinality = 2, RequireUniqueNames = true)]
        public Mixed(Func<object?> first, Func<object?> rest) { }
        public object? Evaluate(object? value) => value;
    }

    public sealed class InvalidBounds : IFunction
    {
        [ArgumentLayout(ArgumentLayoutKind.Named, MinimumCardinality = 2, MaximumCardinality = 1)]
        public InvalidBounds(Func<object?> value) { }
        public object? Evaluate(object? value) => value;
    }

    public sealed class InvalidPrefix : IFunction
    {
        [ArgumentLayout(ArgumentLayoutKind.Named, PositionalPrefix = 1)]
        public InvalidPrefix(Func<object?> value) { }
        public object? Evaluate(object? value) => value;
    }

    public sealed class InvalidUniqueness : IFunction
    {
        [ArgumentLayout(ArgumentLayoutKind.Positional, RequireUniqueNames = true)]
        public InvalidUniqueness(Func<object?> value) { }
        public object? Evaluate(object? value) => value;
    }
}
