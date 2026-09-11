using Expressif.Functions;
using Expressif.Functions.Flow;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Functions.Flow;

public class CatchTest
{
    [Conformance]
    public void Catch_Recovery(object? value, string expression, string? expected)
        => Assert.That(Expression.Create($"catch({expression}) | upper").Evaluate(value), Is.EqualTo(expected));

    [TestCase("{nickname := #null, name := \"Alice\"} | .nickname | catch(.name) | upper", "Alice")]
    [TestCase("{nickname := #null, name := \"Alice\"} | .nickname | catch(expression:= .name) | upper", "Alice")]
    [TestCase("{nickname := #null, name := \"Alice\"} | .nickname | catch(.name | upper) | lower", "ALICE")]
    [TestCase("#null | catch(#null) | catch(42)", null)]
    [TestCase("#null | catch(42) | upper", 42)]
    [TestCase("\"Alice\" | catch(throw(#true)) | upper", "ALICE")]
    [TestCase("#null | catch(\"Alice\") | throw(#true)", "Alice")]
    [TestCase("#null | catch(#null | catch(\"Alice\") | upper) | lower", "Alice")]
    [TestCase("#null | apply(catch(\"Alice\") | lower) | upper", "ALICE")]
    public void Evaluate_PipelineBoundaryAndContext(string source, object? expected)
        => Assert.That(Expression.Create(source).Evaluate(null), Is.EqualTo(expected));

    [Test]
    public void Evaluate_MapTerminatesOnlyElementPipeline()
    {
        var source = "{{nickname := #null, name := \"Alice\"}, {nickname := \"Bob\", name := \"Bobby\"}} | map(.nickname | catch(.name) | upper) | reverse";
        Assert.That(Expression.Create(source).Evaluate(null), Is.EqualTo(new[] { "BOB", "Alice" }));
    }

    [Test]
    public void Evaluate_RecoveryRunsOnceAndPropagatesErrors()
    {
        var count = 0;
        var function = new Catch(() => { count++; return new Throw(); });
        Assert.That(function.Evaluate(42), Is.EqualTo(42));
        Assert.That(count, Is.Zero);
        Assert.Throws<EvaluationException>(() => function.Evaluate(null));
        Assert.That(count, Is.EqualTo(1));
    }

    [Test]
    public void Evaluate_NullSemantics()
    {
        var count = 0;
        var function = new Catch(() => { count++; return new Expressif.Functions.Special.Neutral(); });
        function.Evaluate(DBNull.Value);
        Assert.That(count, Is.EqualTo(1));
    }

    [TestCase("")]
    [TestCase(" ")]
    public void Evaluate_EmptyAndBlankAreNotNull(string input)
        => Assert.That(new Catch(() => throw new InvalidOperationException()).Evaluate(input), Is.SameAs(input));

    [Test]
    public void Evaluate_PreservesObjectIdentity()
    {
        var value = new object();
        Assert.That(Expression.Create("catch(42) | throw()").Evaluate(value), Is.SameAs(value));
    }

    [TestCase("catch()")]
    [TestCase("catch(1, 2)")]
    [TestCase("catch(other:= 1)")]
    [TestCase("catch(1, expression:= 2)")]
    public void Bind_InvalidArguments(string source)
        => Assert.Catch<ExpressifException>(() => Expression.Create(source));

    [Test]
    public void Evaluate_ConcurrentReuse()
    {
        var expression = Expression.Create("catch(\"fallback\") | upper");
        Parallel.For(0, 100, index => Assert.That(expression.Evaluate(index % 2 == 0 ? null : "value"),
            Is.EqualTo(index % 2 == 0 ? "fallback" : "VALUE")));
    }
}
