using Expressif.Bindings;
using Expressif.Functions;
using Expressif.Semantics;
using Expressif.Values;

namespace Expressif.Testing.Functions.Array;

public class TupleBindingIntegrationTest
{
    [TestCase("{20, 3, 2} | reduce(subtract~)", "15")]
    [TestCase("{20, 3, 2} | reduce(subtract~, 100)", "75")]
    [TestCase("{20, 3, 2} | reduce(~subtract)", "19")]
    [TestCase("{1, 2, 5} | adjacent(~subtract)", "{1, 3}")]
    [TestCase("{1, 2, 5} | adjacent(subtract~)", "{-1, -3}")]
    [TestCase("{1, 2, 5} | chunk-while($1 | subtract($0 | last) | is-less-than(2))", "{{1, 2}, {5}}")]
    [TestCase("{1, 2, 5} | chunk-while($0 | last | subtract($1) | is-less-than(-1))", "{{1}, {2, 5}}")]
    [TestCase("\"aaabb\" | split-while(starts-with~)", "{\"aaa\", \"bb\"}")]
    [TestCase("\"aaabb\" | split-while(~starts-with)", "{\"aa\", \"a\", \"bb\"}")]
    [TestCase("5 | map-over(subtract~, {10, 11})", "{-5, -6}")]
    [TestCase("5 | map-with(~subtract, {10, 11})", "{5, 6}")]
    [TestCase("20 | map-over(subtract~, {T(1, 2), T(3, 4)})", "{18, 8}")]
    [TestCase("5 | map-over(subtract~ | add(@_), {10, 11})", "{5, 5}")]
    [TestCase("5 | map-with(~subtract | add(@_), {10, 11})", "{15, 17}")]
    [TestCase("5 | map-over(tuple(20, 3) | subtract~, {100})", "{17}")]
    [TestCase("5 | map-with(tuple(20, 3) | subtract~, {100})", "{17}")]
    [TestCase("T(1, 2) | map-over(arity~, array(tuple()))", "{2}")]
    [TestCase("T(1, 2) | map-with(~tuple, {T(3, 4)})", "array(tuple(T(1, 2)))")]
    [TestCase("T(1, 2) | map-over(tuple~, {T(T(3, 4), 5)})", "{T(T(3, 4), 5)}")]
    [TestCase("{T(1, 2), T(3, 4)} | reduce(tuple~)", "tuple(T(3, 4))")]
    [TestCase("{} | reduce(subtract~)", "#null")]
    [TestCase("{20} | reduce(subtract~)", "20")]
    [TestCase("{} | adjacent(~subtract)", "{}")]
    [TestCase("{20} | adjacent(~subtract)", "{}")]
    [TestCase("{} | chunk-while(~subtract | less-than(2))", "{}")]
    [TestCase("{20} | chunk-while(~subtract | less-than(2))", "{{20}}")]
    [TestCase("\"\" | split-while(starts-with~)", "{}")]
    [TestCase("\"a\" | split-while(starts-with~)", "{\"a\"}")]
    [TestCase("\"ab\" | split-while(tuple~)", "#null")]
    [TestCase("{1, 2} | chunk-while(tuple~)", "#null")]
    public void ExplicitBinding_Composes(string source, string expected)
        => Assert.That(Expression.Create(source).Evaluate(null), Is.EqualTo(Expression.Create(expected).Evaluate(null)));

    [TestCase("map-over", "subtract~", "bind(\"subtract\")")]
    [TestCase("map-with", "~subtract", "rotate | bind(\"subtract\")")]
    [TestCase("map-with", "~subtract", "rotate(1) | bind(\"subtract\")")]
    [TestCase("map-over", "(subtract~)", "(bind(\"subtract\"))")]
    public void CanonicalForms_KeepFollowingArgumentContexts(string map, string shorthand, string canonical)
    {
        var suffix = " | add(@_), {10, 11})";
        Assert.That(Expression.Create(map + "(" + shorthand + suffix).Evaluate(5),
            Is.EqualTo(Expression.Create(map + "(" + canonical + suffix).Evaluate(5)));
    }

    [TestCase("{1, 2, 5} | adjacent(subtract)", "{1, 2, 5} | adjacent(~subtract)")]
    [TestCase("{1, 2, 5} | adjacent(greater-than)", "{1, 2, 5} | adjacent(~greater-than)")]
    [TestCase("{1, #null, 3} | adjacent(subtract)", "{1, #null, 3} | adjacent(~subtract)")]
    [TestCase("5 | map-over(subtract, {10, 11})", "5 | map-over(subtract~, {10, 11})")]
    [TestCase("5 | map-with(subtract, {10, 11})", "5 | map-with(~subtract, {10, 11})")]
    [TestCase("20 | map-over(subtract, {T(1, 2), T(3, 4)})", "20 | map-over(subtract~, {T(1, 2), T(3, 4)})")]
    public void Migration_PreservesLegacyResults(string legacy, string explicitSource)
        => Assert.That(Expression.Create(explicitSource).Evaluate(null), Is.EqualTo(Expression.Create(legacy).Evaluate(null)));

    [Test]
    public void Boundaries_DoNotPrepareLaterBindingsOrUnrelatedRotations()
    {
        Assert.That(TupleBindingOperations.LeadingLength(new OpenExpression([
            new Expressif.Bindings.Function("rotate", [new LiteralParameter("-1")]),
            new Expressif.Bindings.Function("bind", [new QuotedLiteralParameter("subtract")])])), Is.Zero);
    }
    [TestCase("{1, 2, 5} | adjacent(subtract)", "adjacent", "~subtract")]
    [TestCase("5 | map-over(subtract, {10, 11})", "map-over", "subtract~")]
    [TestCase("5 | map-with(subtract, {10, 11})", "map-with", "~subtract")]
    [TestCase("{1, 2} | adjacent(greater-than)", "adjacent", "~greater-than")]
    public void DeprecatedUse_IdentifiesSignatureAndSafeReplacement(string source, string consumer, string replacement)
    {
        var uses = new LegacyTupleBindingAnalyzer().Analyze(Expressif.Syntax.ExpressionParser.Parse(source));
        Assert.That(uses, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(uses[0].Operator, Is.EqualTo(consumer));
            Assert.That(uses[0].Signatures, Is.Not.Empty);
            Assert.That(uses[0].CanRewrite, Is.True);
            Assert.That(uses[0].Replacement, Is.EqualTo(replacement));
            Assert.That(uses[0].Span, Is.Not.Null);
        });
    }

    [TestCase("adjacent(arity)")]
    [TestCase("adjacent(~subtract)")]
    [TestCase("chunk-while(rotate | bind(\"subtract\") | less-than(2))")]
    [TestCase("reduce($0 | subtract($1))")]
    [TestCase("split-while($0 | starts-with($1))")]
    [TestCase("map-over(subtract~, {1, 2})")]
    public void CompleteExpression_IsNotDeprecated(string source)
        => Assert.That(new LegacyTupleBindingAnalyzer().Analyze(Expressif.Syntax.ExpressionParser.Parse(source)), Is.Empty);

    [TestCase("{1, 2, 5} | chunk-while(subtract | less-than(2))")]
    [TestCase("adjacent(subtract)")]
    [TestCase("{T(1, 2), T(3, 4)} | adjacent(subtract)")]
    [TestCase("5 | map-over(subtract, {T(1, 2, 3)})")]
    public void UnknownOrInvalidInvocation_WithholdsAutomaticRewrite(string source)
    {
        var uses = new LegacyTupleBindingAnalyzer().Analyze(Expressif.Syntax.ExpressionParser.Parse(source));
        Assert.That(uses, Has.Count.EqualTo(1));
        Assert.That(uses[0].CanRewrite, Is.False);
    }

    [TestCase("adjacent(counted)", "adjacent(~counted)", 2)]
    [TestCase("map-over(counted, {3, 2})", "map-over(counted~, {3, 2})", 2)]
    [TestCase("map-with(counted, {3, 2})", "map-with(~counted, {3, 2})", 2)]
    public void CustomCallables_MigrateWithIdenticalEvaluationCounts(string legacy, string replacement, int count)
    {
        object input = legacy.StartsWith("map-", StringComparison.Ordinal) ? 20 : new[] { 1, 2, 5 };
        Counted.Calls = 0;
        var expected = EvaluateCustom(legacy, input);
        Assert.That(Counted.Calls, Is.EqualTo(count));
        Counted.Calls = 0;
        Assert.That(EvaluateCustom(replacement, input), Is.EqualTo(expected));
        Assert.That(Counted.Calls, Is.EqualTo(count));
    }

    [Test]
    public void CustomPredicate_SplitInvokesOncePerCandidate()
    {
        CountedStarts.Calls = 0;
        Assert.That(EvaluateCustom("split-while(counted-starts~)", "aaabb"), Is.EqualTo(new[] { "aaa", "bb" }));
        Assert.That(CountedStarts.Calls, Is.EqualTo(4));
    }

    [Test]
    public void CustomReduce_InvokesOncePerItemAfterSeed()
    {
        Counted.Calls = 0;
        Assert.That(EvaluateCustom("reduce(counted~)", new[] { 20, 3, 2 }), Is.EqualTo(15));
        Assert.That(Counted.Calls, Is.EqualTo(2));
    }

    [Test]
    public void SplitFollowingArguments_RetainEnclosingContext()
        => Assert.That(Expression.Create("{text := \"aaabb\", flag := #true} | .text | split-while(starts-with~ | is-equal-to(.flag))")
            .Evaluate(null), Is.EqualTo(new[] { "aaa", "bb" }));

    private static object? EvaluateCustom(string source, object? input)
    {
        var factory = new FunctionFactory(new CustomMapper());
        var bound = new ExpressifBinder().Bind(Expressif.Syntax.ExpressionParser.Parse(source));
        var result = new Expression(factory.Instantiate(bound, new Context())).Evaluate(input);
        return result is System.Collections.IEnumerable values and not string ? values.Cast<object?>().ToArray() : result;
    }

    private sealed class CustomMapper : BaseTypeMapper
    {
        protected override IDictionary<string, Type> Initialize()
        {
            var mapping = new Dictionary<string, Type>();
            foreach (var info in new Expressif.Functions.Introspection.FunctionIntrospector().Locate())
                foreach (var name in info.Aliases.Prepend(info.Name)) mapping[name] = info.ImplementationType;
            mapping["counted"] = typeof(Counted);
            mapping["chunk-counted"] = typeof(ChunkCounted);
            mapping["counted-starts"] = typeof(CountedStarts);
            return mapping;
        }
    }

    [TestCase("chunk-while(chunk-counted)")]
    [TestCase("chunk-while(~chunk-counted)")]
    public void CustomChunkPredicate_ReceivesWholeChunkOncePerCandidate(string source)
    {
        ChunkCounted.Calls = 0;
        Assert.That(EvaluateCustom(source, new[] { 1, 2, 3, 4, 5 }),
            Is.EqualTo(new object?[][] { [1, 2], [3, 4], [5] }));
        Assert.That(ChunkCounted.Calls, Is.EqualTo(4));
    }

    public sealed class ChunkCounted(Func<object?[]> argument) : IFunction<object?, bool>
    {
        public static int Calls { get; set; }
        public bool Evaluate(object? value) { Calls++; return argument().Length < 2; }
        object? IFunction.Evaluate(object? value) => Evaluate(value);
    }
    public sealed class Counted(Func<decimal> argument) : IFunction<object?, object?>
    {
        public static int Calls { get; set; }
        public object? Evaluate(object? value)
        {
            Calls++;
            return value is null ? null : new Expressif.Values.Casters.NumericCaster().Cast(value) - argument();
        }
    }

    public sealed class CountedStarts(Func<string> argument) : IFunction<string?, bool>
    {
        public static int Calls { get; set; }
        public bool Evaluate(string? value) { Calls++; return value?.StartsWith(argument(), StringComparison.Ordinal) ?? false; }
        object? IFunction.Evaluate(object? value) => Evaluate((string?)value);
    }
}
