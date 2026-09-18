using Expressif.Bindings;
using Expressif.Functions.Flow;
using Expressif.Functions.Introspection;
using Expressif.Serializers;
using Expressif.Syntax;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Functions.Flow;

public class LetTest
{
    [Conformance]
    public void Let_Valid(string source, decimal? expected)
        => Assert.That(Expression.CreateClosed(source).Evaluate(null), Is.EqualTo(expected));

    [TestCase("let()")]
    [TestCase("let(10)")]
    [TestCase("let(a :=)")]
    [TestCase("let(a := 1, 2)")]
    [TestCase("let(...@_)")]
    [TestCase("let(a := 1, a := 2)")]
    [TestCase("let(\"a\" := 1)")]
    [TestCase("let(_a := 1)")]
    public void InvalidBindings_AreRejected(string source)
        => Assert.Catch(() => Expression.Create(source));

    [TestCase("10 | let(a := 1, b := @a) | @b")]
    [TestCase("10 | let(a := @missing) | @a")]
    [TestCase("{1, 2} | map(let(local := @_) | @local) | @local")]
    public void UnknownBindings_UseNormalDiagnostic(string source)
        => Assert.That(() => Expression.CreateClosed(source).Evaluate(null), Throws.InstanceOf<ExpressifException>());

    [TestCase("let(value := 10) | map(let(value := add(1)) | @value)", new[] { 2, 3 })]
    [TestCase("let(value := 10) | map(add(@value))", new[] { 11, 12 })]
    public void InlineInvocations_InheritAndShadow(string source, int[] expected)
        => Assert.That(Expression.Create(source).Evaluate(new[] { 1, 2 }), Is.EqualTo(expected));

    [TestCase("let(value := 10) | map(let(value := add(1)) | @value) | @value", 10)]
    [TestCase("let(value := 10) | apply(let(value := 20) | @value) | @value", 10)]
    [TestCase("(let(value := 10)) | add(@value)", 11)]
    [TestCase("let(value := 10) | apply(@_ | value :> let(value := 20) | @value) | @value", 10)]
    public void NestedInvocations_RestoreOuterValues(string source, int expected)
        => Assert.That(Expression.Create(source).Evaluate(1), Is.EqualTo(expected));

    [Test]
    public void NestedFailure_RestoresOuterEnvironmentAndPublishesNoPartialBindings()
    {
        using var invocation = EvaluationRuntime.Enter(new EvaluationFrame(1, 1), EvaluationContext.Empty);
        var outer = new Let(() => [new("value", _ => 10)]);
        outer.Evaluate(1);
        var failing = new Let(() => [new("value", _ => 20), new("missing", _ => throw new InvalidOperationException())]);
        Assert.That(() => EvaluationRuntime.EvaluateNested(failing, 1), Throws.InvalidOperationException);
        Assert.That(EvaluationRuntime.TryGetBinding("value", out var value), Is.True);
        Assert.That(value, Is.EqualTo(10));
        Assert.That(EvaluationRuntime.TryGetBinding("missing", out _), Is.False);
        Assert.That(() => failing.Evaluate(1), Throws.InvalidOperationException);
        Assert.That(EvaluationRuntime.TryGetBinding("value", out value), Is.True);
        Assert.That(value, Is.EqualTo(10));
    }

    [Test]
    public void ParallelBindings_ReadOuterEnvironment()
    {
        var expression = Expression.Create("let(a := 5) | let(a := 10, b := @a) | @b");
        Assert.That(expression.Evaluate(1), Is.EqualTo(5));
    }

    [TestCase("T(10, 20) | let(v := 1) | apply(^^$1)", 20)]
    [TestCase("T(10, 20) | apply($0 | let(v := ^^$1) | @v)", 20)]
    [TestCase("T(10, 20) | let(v := 1) | apply(let(w := 2) | ^^$1)", 20)]
    public void Let_DoesNotChangeCaretDepth(string source, int expected)
        => Assert.That(Expression.CreateClosed(source).Evaluate(null), Is.EqualTo(expected));

    [Test]
    public void BindingExpressions_UseTheOriginalRecordContext()
    {
        var input = new Expressif.Values.RecordValue();
        input.Set("quantity", 4);
        input.Set("unit-price", 5);
        var expression = Expression.Create("let(total := .quantity | multiply(.unit-price)) | @total");
        Assert.That(expression.Evaluate(input), Is.EqualTo(20));
    }

    [Test]
    public void NestedRecordBinding_UsesEnclosingValues()
    {
        const string source = "{country := \"BE\", employees := {{name := \"Alice\"}, {name := \"Bob\"}}} | let(country := .country) | .employees | map(let(name := .name) | record(country := @country, name := @name))";
        var values = (System.Collections.IEnumerable)Expression.CreateClosed(source).Evaluate(null)!;
        Assert.That(values.Cast<Expressif.Values.RecordValue>().Select(value => value["country"]), Is.All.EqualTo("BE"));
        Assert.That(values.Cast<Expressif.Values.RecordValue>().Select(value => value["name"]), Is.EqualTo(new[] { "Alice", "Bob" }));
    }

    [Test]
    public void ContextValues_AreShadowedWithoutMutation()
    {
        var context = new Context();
        context.Variables.Add<int>("value", 100);
        var expression = Expression.Create("let(value := @_) | @value", context);
        Assert.That(expression.Evaluate(10), Is.EqualTo(10));
        Assert.That(Expression.CreateClosed("@value", context).Evaluate(null), Is.EqualTo(100));
        Assert.That(context.Variables["value"], Is.EqualTo(100));
    }

    [Test]
    public void OriginalInput_IsPreservedIncludingNull()
    {
        var expression = Expression.Create("let(original := @_) | @original");
        var passThrough = Expression.Create("let(value := 1)");
        foreach (var value in new object?[] { null, 42, "text", new[] { 1, 2 }, new Expressif.Values.Tuple(1, 2) })
        {
            Assert.That(expression.Evaluate(value), Is.SameAs(value));
            Assert.That(passThrough.Evaluate(value), Is.SameAs(value));
        }
    }

    [Test]
    public void RepeatedAndConcurrentEvaluations_AreIsolated()
    {
        var expression = Expression.Create("let(original := @_, double := multiply(2)) | add(@double) | add(@original)");
        Parallel.For(1, 100, input => Assert.That(expression.Evaluate(input), Is.EqualTo(input * 4)));
        Assert.That(expression.Evaluate(5), Is.EqualTo(20));
    }

    [Test]
    public void Bindings_AreEvaluatedOnceInOrderAndStopOnFailure()
    {
        var calls = new List<string>();
        var function = new Let(() => [
            new("first", input => { calls.Add("first"); return input; }),
            new("fail", _ => { calls.Add("fail"); throw new InvalidOperationException(); }),
            new("last", input => { calls.Add("last"); return input; }),
        ]);
        Assert.That(() => function.Evaluate(10), Throws.InvalidOperationException);
        Assert.That(calls, Is.EqualTo(new[] { "first", "fail" }));
        using var invocation = EvaluationRuntime.Enter(new EvaluationFrame(10, 10), EvaluationContext.Empty);
        var count = 0;
        var successful = new Let(() => [new("once", input => { count++; return input; })]);
        successful.Evaluate(10);
        EvaluationRuntime.TryGetBinding("once", out _);
        EvaluationRuntime.TryGetBinding("once", out _);
        Assert.That(count, Is.EqualTo(1));
    }

    [Test]
    public void Failure_StopsRemainingBindingsAndPipelineStages()
    {
        var calls = 0;
        var context = new Context();
        context.Variables.Add<int>("next", (Func<int>)(() => ++calls));
        var failing = Expression.Create("let(a := @missing, b := @next) | add(@next)", context);
        Assert.That(() => failing.Evaluate(1), Throws.TypeOf<UnexpectedVariableException>());
        Assert.That(calls, Is.Zero);
        var successful = Expression.Create("let(a := @next) | add(@a) | multiply(@a)", context);
        Assert.That(successful.Evaluate(1), Is.EqualTo(2));
        Assert.That(calls, Is.EqualTo(1));
    }

    [Test]
    public void CurrentInputCapture_UsesThisCallsPipelineInput()
        => Assert.That(Expression.Create("add(1) | let(changed := @_) | @changed").Evaluate(10), Is.EqualTo(11));

    [Test]
    public void FailedInvocation_DoesNotLeakSuccessfullyEstablishedBindings()
    {
        var context = new Context();
        context.Variables.Add<int>("value", 100);
        var expression = Expression.Create("let(value := 10) | let(a := 1, b := @missing)", context);
        Assert.Catch(() => expression.Evaluate(1));
        Assert.That(Expression.CreateClosed("@value", context).Evaluate(null), Is.EqualTo(100));
    }

    [Test]
    public void SeparateExpressionInvocation_DoesNotInheritCallerBindings()
    {
        var callee = Expression.CreateClosed("@local");
        var context = new Context();
        context.Variables.Add<object?>("invoke", () => callee.Evaluate(null));
        var caller = Expression.Create("let(local := 10) | @invoke", context);
        Assert.Catch(() => caller.Evaluate(1));
    }

    [TestCase("let(a := \"literal | @missing\") | @a", "literal | @missing")]
    [TestCase("let(a := 10) /* | @missing */ | @a", 10)]
    [TestCase("let(a := 10) | // @missing\n @a", 10)]
    public void ValueStages_PreserveStringsAndComments(string source, object expected)
        => Assert.That(Expression.Create(source).Evaluate(1), Is.EqualTo(expected));

    [Test]
    public void Serialization_PreservesBindings()
    {
        const string source = "let(a := multiply(2), b := @_)";
        var bound = new ExpressifBinder().BindFunction(ExpressionParser.Parse(source));
        Assert.That(new FunctionSerializer().Serialize(bound), Is.EqualTo("let(a := multiply(2), b := ...)"));
    }

    [Test]
    public void Introspection_DescribesInputPreservationAndVariadicEntries()
    {
        var info = new FunctionIntrospector().Describe().Single(function => function.Name == "let");
        Assert.That(info.Reason, Does.Contain("preserves the pipeline input type"));
        Assert.That(info.Parameters.Single().Type, Is.EqualTo("entry"));
        Assert.That(info.Parameters.Single().Variadic, Is.True);
        Assert.That(info.Parameters.Single().MinimumCardinality, Is.EqualTo(1));
    }
}
