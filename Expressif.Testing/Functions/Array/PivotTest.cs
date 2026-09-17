using System.Collections;
using Expressif.Bindings;
using Expressif.Functions;
using Expressif.Functions.Introspection;
using Expressif.Testing.Conformance;
using Expressif.Values;
using PivotFunction = Expressif.Functions.Array.Pivot;
using GroupingValue = Expressif.Values.Grouping;

namespace Expressif.Testing.Functions.Array;

public class PivotTest
{
    [Conformance]
    public void Pivot_Valid_Records(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(Expression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [Conformance]
    public void Pivot_Valid_Empty(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(Expression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [TestCase(".r | upper")]
    [TestCase(".r.nested")]
    [TestCase("$0")]
    [TestCase("upper")]
    public void ComputedRows_RejectDuringBinding(string row)
        => Assert.That(() => Expression.Create($"pivot({row}, .c, summarize(first))"),
            Throws.TypeOf<BindingException>().With.Message.Contains("direct field selector"));

    [TestCase("summarize(first) | map($value)")]
    [TestCase("!{}")]
    [TestCase("!{(T(1, 2) => 3)}")]
    [TestCase("!{(T(1, 2, 3) => 4)}")]
    public void IncompatibleSummary_RejectsExplicitly(string summary)
        => Assert.That(() => Expression.Create($"{{{{r := \"BE\", c := \"a\"}}}} | pivot(.r, .c, {summary})").Evaluate(null),
            Throws.ArgumentException.With.Message.Contains("preserving every generated"));

    [TestCase("{{r := 1, c := 2025}, {r := 1, c := \"2025\"}}")]
    [TestCase("{{r := 1, c := \"r\"}}")]
    public void FieldCollisions_UseFromPairsDiagnostic(string input)
        => Assert.That(() => Expression.Create($"{input} | pivot(.r, .c, summarize(cardinality))").Evaluate(null),
            Throws.ArgumentException.With.Message.StartsWith("Duplicate record field name"));

    [Test]
    public void KeysAndSummary_EvaluateOnceInOrder()
    {
        var calls = new List<string>();
        IFunction<IEnumerable, RecordValue[]?> function = new PivotFunction(
            new NamedFieldSelector("r", value => { calls.Add($"row:{value}"); return value; }),
            value => { calls.Add($"column:{value}"); return "c"; },
            value =>
            {
                calls.Add("summary");
                var grouping = (GroupingValue)value!;
                return new Expressif.Values.Dictionary(grouping.Select(group => new PairValue(group.Key, group.Count)));
            });

        var result = function.Evaluate(new[] { 1, 1, 2 });
        Assert.Multiple(() =>
        {
            Assert.That(calls, Is.EqualTo(new[] { "row:1", "column:1", "row:1", "column:1", "row:2", "column:2", "summary" }));
            Assert.That(ValueFormatter.Format(result), Is.EqualTo("{{r := 1, c := 2}, {r := 2, c := 1}}"));
        });
    }

    [Test]
    public void EmptyInput_StillEvaluatesSummary()
    {
        var calls = 0;
        var function = new PivotFunction(new NamedFieldSelector("r", _ => throw new InvalidOperationException()),
            _ => throw new InvalidOperationException(), value =>
            {
                calls++;
                Assert.That(((GroupingValue)value!).Count, Is.Zero);
                return new Expressif.Values.Dictionary([]);
            });
        Assert.That(function.Evaluate(System.Array.Empty<object>()), Is.Empty);
        Assert.That(calls, Is.EqualTo(1));
    }

    [Test]
    public void Metadata_ExposesClosedContractAndExpressions()
    {
        var info = new FunctionIntrospector().Describe().Single(info => info.Name == "pivot");
        Assert.That(info.ImplementationType, Is.EqualTo(typeof(PivotFunction)));
        Assert.That(info.Input, Is.EqualTo("array"));
        Assert.That(info.Output, Is.EqualTo("array"));
        Assert.That(info.Parameters.Select(parameter => (parameter.Name, parameter.Type, parameter.Optional)),
            Is.EqualTo(new[] { ("row", "expression", false), ("column", "expression", false), ("summary", "expression", false) }));
        Assert.That(typeof(IFunction<IEnumerable, RecordValue[]?>).IsAssignableFrom(typeof(PivotFunction)), Is.True);
    }

    [Test]
    public void Pivot_MatchesCanonicalDecomposition()
    {
        var input = Expression.Create("{{r := \"BE\", c := 2025, v := 3}, {r := \"FR\", c := 2026, v := 7}, {r := \"BE\", c := 2025, v := 2}}").Evaluate(null);
        var pivot = Expression.Create("pivot(.r, .c, summarize(|> .v | sum))").Evaluate(input);
        var composition = Expression.Create("group-by(.r, .c) | summarize(|> .v | sum) | nest | map(record(r := $key, ...($value | from-pairs)))").Evaluate(input);
        Assert.That(ValueFormatter.Format(pivot), Is.EqualTo(ValueFormatter.Format(composition)));
    }

    [Test]
    public void NestedCall_UsesTheArrayEnteringPivot()
    {
        var result = Expression.Create("{r := \"outer\", c := \"outer\", values := {{r := \"BE\", c := \" A \"}}} | .values | pivot(.r, .c | trim | lower, summarize(cardinality))").Evaluate(null);
        Assert.That(ValueFormatter.Format(result), Is.EqualTo("{{r := \"BE\", a := 1}}"));
    }

    [Test]
    public void NullColumnKey_UsesFromPairsDiagnostic()
        => Assert.That(() => Expression.Create("{{r := #null, c := #null}} | pivot(.r, .c, summarize(first))").Evaluate(null),
            Throws.ArgumentException.With.Message.StartsWith("Every pair key must be coercible to text"));

    [Test]
    public void BoundExpression_SupportsConcurrentReuse()
    {
        var expression = Expression.Create("pivot(.r, .c, summarize(|> .v | sum))");
        Parallel.For(0, 20, index =>
        {
            var input = Expression.Create($"{{{{r := {index}, c := \"a\", v := {index}}}}}").Evaluate(null);
            Assert.That(ValueFormatter.Format(expression.Evaluate(input)), Is.EqualTo($"{{{{r := {index}, a := {index}}}}}"));
        });
    }
}
