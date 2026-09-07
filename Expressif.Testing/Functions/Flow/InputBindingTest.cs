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
    public void Anonymous_SerializationPreservesReferenceSemantics(string source)
    {
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
