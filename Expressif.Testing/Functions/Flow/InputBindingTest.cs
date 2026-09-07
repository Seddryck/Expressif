using Expressif.Bindings;
using Expressif.Serializers;
using Expressif.Syntax;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Functions.Flow;

public class InputBindingTest
{
    [Conformance]
    public void InputBinding_Named(string source, object? expected)
        => Assert.That(Expression.CreateClosed(source).Evaluate(null), Is.EqualTo(NormalizeExpected(expected)));

    [Conformance]
    public void InputBinding_Anonymous(string source, object? expected)
        => Assert.That(Expression.CreateClosed(source).Evaluate(null), Is.EqualTo(NormalizeExpected(expected)));

    [TestCase("apply(:> $0 | add($1) | $1)")]
    [TestCase("apply(:> .first | upper | .last)")]
    [TestCase("apply(:> .address.city | upper)")]
    public void Anonymous_SerializationPreservesReferenceSemantics(string source)
    {
        var function = new ExpressifBinder().BindFunction(ExpressionParser.Parse(source));
        Assert.That(new FunctionSerializer().Serialize(function), Is.EqualTo(source));
    }

    [Conformance]
    public void InputBinding_Destructuring(string source, object? expected)
        => Assert.That(Expression.CreateClosed(source).Evaluate(null), Is.EqualTo(NormalizeExpected(expected)));

    [TestCase("T(10, 20) | apply(:> $0 | apply(:> add((^^$1))))", 30)]
    [TestCase("T(10, 20) | apply(:> with(v := 1, :> ^^$1))", 20)]
    [TestCase("T(10, 20) | apply(:> transform-as(:> ^^$1, x := $0) | field(x))", 20)]
    public void Binding_DoesNotDuplicateInvocationScopes(string source, int expected)
        => Assert.That(Expression.CreateClosed(source).Evaluate(null), Is.EqualTo(expected));

    [Test]
    public void Destructuring_GroupBindsKeyAndValueCollection()
    {
        var group = new Expressif.Values.Group("BE", new[] { 10, 20, 30 });
        var expression = Expression.Create("apply((key, values) :> @values | count)");
        Assert.That(expression.Evaluate(group), Is.EqualTo(3));
        Assert.That(Expression.Create("apply((key, values) :> @key)").Evaluate(group), Is.EqualTo("BE"));
    }

    [TestCase("apply((a, a) :> @a)")]
    [TestCase("apply((a, b, a) :> @b)")]
    public void Destructuring_RejectsDuplicateNames(string source)
        => Assert.That(() => Expression.Create(source), Throws.TypeOf<BindingException>()
            .With.Message.Contains("Duplicate input binding name 'a'").And.Message.Contains("offset"));

    [TestCase("apply(() :> identity)")]
    [TestCase("apply((a) :> @a)")]
    [TestCase("apply((a, b,) :> @a)")]
    [TestCase("apply((a, (b, c)) :> @a)")]
    public void Destructuring_RejectsMalformedLists(string source)
        => Assert.Throws<ExpressifSyntaxException>(() => Expression.Create(source));

    [Test]
    public void Destructuring_RequiresExactArityAndPositionalInput()
    {
        var expression = Expression.Create("apply((a, b) :> @a)");
        foreach (var input in new object?[] { null, 42, "ab", new[] { 1, 2 }, new Expressif.Values.RecordValue() })
            Assert.That(() => expression.Evaluate(input), Throws.ArgumentException.With.Message.Contains("requires a tuple"));
        foreach (var input in new[] { new Expressif.Values.Tuple(1), new Expressif.Values.Tuple(1, 2, 3) })
            Assert.That(() => expression.Evaluate(input), Throws.ArgumentException.With.Message.Contains("expects 2 components"));
    }

    [Test]
    public void Destructuring_SerializationPreservesNamesAndOrder()
    {
        const string source = "apply((a, b, c) :> @c | subtract(@a) | add(@b))";
        var function = new ExpressifBinder().BindFunction(ExpressionParser.Parse(source));
        Assert.That(new FunctionSerializer().Serialize(function), Is.EqualTo(source));
    }

    private static object? NormalizeExpected(object? value) => value switch
    {
        string text when Expressif.Values.RecordSyntax.TryParseTypedToken(text, out var typed) => typed,
        object?[] array => array.Select(NormalizeExpected).ToArray(),
        _ => value,
    };

    [TestCase("apply(input :> @input)")]
    [TestCase("apply(input:> @input)")]
    public void Named_PreservesWholeInput(string source)
    {
        var expression = Expression.Create(source);
        foreach (var input in new object?[] { null, 42, "hello", new[] { 1, 2 }, new Expressif.Values.Tuple(1, 2), new Expressif.Values.RecordValue() })
            Assert.That(expression.Evaluate(input), Is.SameAs(input));
    }

    [Test]
    public void Named_RestoresContextVariableAfterInvocation()
    {
        var context = new Context();
        context.Variables.Add<int>("input", 100);
        var expression = Expression.Create("apply(input :> @input | add(1)) | add(@input)", context);
        Assert.That(expression.Evaluate(10), Is.EqualTo(111));
        Assert.That(context.Variables["input"], Is.EqualTo(100));
    }

    [Test]
    public void Named_SerializationPreservesBinding()
    {
        const string source = "apply(input :> @input | add(@input))";
        var bound = new ExpressifBinder().BindFunction(ExpressionParser.Parse(source));
        Assert.That(new FunctionSerializer().Serialize(bound), Is.EqualTo(source));
    }

    [TestCase("T(10, 20) | apply(outer :> $0 | apply(inner :> ^^$1))", 20)]
    [TestCase("T(10, 20) | map(input :> ^^$1)", 20)]
    [TestCase("T(10, 20) | apply(input :> $0 | add(5) | $1)", 20)]
    public void Named_InputAndEnclosingScopes(string source, int expected)
    {
        var result = Expression.CreateClosed(source).Evaluate(null);
        Assert.That(result, result is System.Collections.IEnumerable ? Is.All.EqualTo(expected) : Is.EqualTo(expected));
    }

    [Test]
    public void Named_FailureDoesNotLeakBinding()
    {
        var context = new Context();
        context.Variables.Add<int>("input", 100);
        var failing = Expression.Create("apply(input :> @missing)", context);
        Assert.Catch(() => failing.Evaluate(10));
        Assert.That(Expression.CreateClosed("@input", context).Evaluate(null), Is.EqualTo(100));
    }

    [Test]
    public void Named_ParallelInvocationsAreIsolated()
    {
        var expression = Expression.Create("apply(input :> add(1) | multiply(@input))");
        Parallel.For(1, 30, input => Assert.That(expression.Evaluate(input), Is.EqualTo((input + 1) * input)));
    }

    [TestCase("apply(_input :> identity)")]
    [TestCase("apply(input : > identity)")]
    [TestCase("apply(input :>)")]
    public void Named_InvalidSyntaxIsRejected(string source)
        => Assert.Throws<ExpressifSyntaxException>(() => Expression.Create(source));
}
