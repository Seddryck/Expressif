using Expressif.Bindings;
using Expressif.Functions;
using Expressif.Functions.Introspection;
using Expressif.Semantics;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Functions.Tuple;

public class BindTest
{
    [Conformance]
    public void Bind_Invoke(string value, string function, string expected)
        => Assert.That(Expression.Create($"bind(\"{function}\")").Evaluate(Expression.Create(value).Evaluate(null)),
            Is.EqualTo(Expression.Create(expected).Evaluate(null)));

    [TestCase("T(120, 135) | subtract~", -15)]
    [TestCase("T(120, 135) | ~subtract", 15)]
    [TestCase("T(120, 135) | rotate(1) | bind(\"subtract\")", 15)]
    [TestCase("T(120, 135) | rotate | bind(\"subtract\")", 15)]
    public void Shorthand_Subtraction(string source, int expected)
        => Assert.That(Expression.Create(source).Evaluate(null), Is.EqualTo(expected));

    [TestCase("T(\"abcabc\", \"a\", \"x\") | replace-chars~", "xbcxbc")]
    [TestCase("T(\"a\", \"x\", \"abcabc\") | ~replace-chars", "xbcxbc")]
    public void Shorthand_PreservesMultipleArgumentOrder(string source, string expected)
        => Assert.That(Expression.Create(source).Evaluate(null), Is.EqualTo(expected));

    [TestCase("T(4, 2) | greater-than~", true)]
    [TestCase("T(4, 2) | ~greater-than", false)]
    public void Shorthand_Predicates(string source, bool expected)
        => Assert.That(Expression.Create(source).Evaluate(null), Is.EqualTo(expected));

    [TestCase("T(120, 135) | extend(~subtract)")]
    [TestCase("T(120, 135) | extend(rotate | bind(\"subtract\"))")]
    public void Nested_ExtendUsesItsTuple(string source)
        => Assert.That(Expression.Create(source).Evaluate(null), Is.EqualTo(new TupleValue(120m, 135m, 15m)));

    [TestCase("T(1, 2) | missing~", TupleBindingFailure.UnknownTarget)]
    [TestCase("T(1, 2) | map~", TupleBindingFailure.IneligibleTarget)]
    [TestCase("T(1, 2) | apply~", TupleBindingFailure.IneligibleTarget)]
    [TestCase("tuple(1) | subtract~", TupleBindingFailure.InvalidArity)]
    [TestCase("T(1, 2, 3, 4) | subtract~", TupleBindingFailure.InvalidArity)]
    [TestCase("tuple() | subtract~", TupleBindingFailure.InvalidInput)]
    [TestCase("1 | subtract~", TupleBindingFailure.InvalidInput)]
    [TestCase("T(1, \"bad\") | subtract~", TupleBindingFailure.IncompatibleValue)]
    [TestCase("tuple(tuple(1), 2) | subtract~", TupleBindingFailure.IncompatibleValue)]
    public void InvalidInvocation_DistinguishesFailures(string source, TupleBindingFailure failure)
    {
        var error = Assert.Throws<TupleBindingException>(() => Expression.Create(source).Evaluate(null));
        Assert.That(error!.Failure, Is.EqualTo(failure));
    }

    [Test]
    public void Diagnostics_RetainShorthandSpan()
    {
        var error = Assert.Throws<TupleBindingException>(() => Expression.Create("missing~"));
        Assert.That(error!.Span, Is.EqualTo(new Expressif.Syntax.SourceSpan(0, 8)));
    }

    [Test]
    public void OptionalAndVariadic_UseSharedSignatures()
    {
        var factory = new FunctionFactory();
        Assert.Multiple(() =>
        {
            Assert.That(factory.InvokeTuple("rotate", new TupleValue(new TupleValue(1, 2, 3))), Is.EqualTo(new TupleValue(3, 1, 2)));
            Assert.That(factory.InvokeTuple("tuple", new TupleValue(0, 1, null, 3)), Is.EqualTo(new TupleValue(1, null, 3)));
            Assert.That(factory.InvokeTuple("tuple", new TupleValue(0)), Is.EqualTo(new TupleValue()));
            Assert.That(factory.InvokeTuple("subtract", new TupleValue(null, 1)), Is.Null);
        });
    }

    [Test]
    public void Introspection_ExposesSignatureEligibility()
    {
        var catalog = new FunctionIntrospector().Locate().ToDictionary(info => info.Name);
        Assert.Multiple(() =>
        {
            Assert.That(catalog["subtract"].Signatures.All(signature => signature.SupportsTupleBinding), Is.True);
            Assert.That(catalog["map"].Signatures.Any(signature => signature.SupportsTupleBinding), Is.False);
            Assert.That(catalog["tuple"].Signatures.Any(signature => signature.Variadic && signature.SupportsTupleBinding), Is.True);
        });
    }

    [Test]
    public void CustomCallable_InvokedOnceAndKeepsValues()
    {
        var factory = new FunctionFactory(new CustomMapper());
        var value = new object();
        Counting.Calls = 0;
        Assert.That(factory.InvokeTuple("counting", new TupleValue(value, null)), Is.SameAs(value));
        Assert.That(Counting.Calls, Is.EqualTo(1));
    }

    [Test]
    public void ConcurrentReuse_DoesNotShareTupleArguments()
    {
        var expression = Expression.Create("subtract~");
        Parallel.For(0, 100, index => Assert.That(expression.Evaluate(new TupleValue(index, 7)), Is.EqualTo(index - 7)));
    }

    [Test]
    public void NameExpression_KeepsEnclosingContext()
        => Assert.That(Expression.Create("{target := \"subtract\", values := T(9, 4)} | .values | bind(.target)").Evaluate(null), Is.EqualTo(5));

    [TestCase("T(1, 2) | subtract~", null)]
    [TestCase("subtract~", null)]
    [TestCase("T(1, 2, 3, 4) | subtract~", TupleBindingFailure.InvalidArity)]
    [TestCase("T(1, \"bad\") | subtract~", TupleBindingFailure.IncompatibleValue)]
    [TestCase("T(1, 2) | map~", TupleBindingFailure.IneligibleTarget)]
    public void SemanticAnalysis_UsesRuntimeCapabilities(string source, TupleBindingFailure? failure)
    {
        var uses = new TupleBindingAnalyzer().Analyze(Expressif.Syntax.ExpressionParser.Parse(source));
        Assert.That(uses, Has.Count.EqualTo(1));
        Assert.That(uses[0].Failure, Is.EqualTo(failure));
    }

    [Test]
    public void InputBinding_RetainsDiagnosticOffsets()
    {
        const string source = "apply(@_ | input :> T(1, 2) | missing~)";
        var error = Assert.Throws<TupleBindingException>(() => Expression.Create(source));
        Assert.That(error!.Span!.Value.Start, Is.EqualTo(source.IndexOf("missing~", StringComparison.Ordinal)));
    }

    [TestCase("T(10, 20) | input :> @input | $1", 20)]
    [TestCase("10 | apply(@_ | input :> @input | add(2))", 12)]
    public void AuthoredPipelineBinding_RemainsSupported(string source, int expected)
        => Assert.That(Expression.Create(source).Evaluate(null), Is.EqualTo(expected));

    private sealed class CustomMapper : BaseTypeMapper
    {
        protected override IDictionary<string, Type> Initialize() => new Dictionary<string, Type> { ["counting"] = typeof(Counting) };
    }

    public sealed class Counting(Func<object?> argument) : IFunction<object?, object?>
    {
        public static int Calls { get; set; }
        public object? Evaluate(object? value) { Calls++; Assert.That(argument(), Is.Null); return value; }
    }
}
