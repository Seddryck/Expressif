using Expressif.Introspection;
using Expressif.Library.Array.Aggregation;
using Expressif.Library.Array.Combination;
using Expressif.Library.Array.Grouping;
using Expressif.Library.Array.Partitioning;
using Expressif.Library.Array.Selection;
using Expressif.Library.Array.Sequencing;
using Expressif.Library.Array.Set;
using System.Collections;
using Expressif.Functions;
using Expressif.Discovery;
using Expressif.Testing.Conformance;
using Expressif.Values;
using UnpivotFunction = Expressif.Library.Array.Grouping.Unpivot;

namespace Expressif.Testing.Array.Grouping;

public class UnpivotTest
{
    [Conformance]
    public void Unpivot_Valid_Records(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(TestExpression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [Conformance]
    public void Unpivot_Valid_Empty(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(TestExpression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [TestCase("{{x := 1}} | unpivot({\"x\", \"x\"}, \"n\", \"v\")", "Duplicate unpivot")]
    [TestCase("{} | unpivot({\"x\", \"x\"}, \"n\", \"v\")", "Duplicate unpivot")]
    [TestCase("{{x := 1}} | unpivot({1}, \"n\", \"v\")", "must be text")]
    [TestCase("{{x := 1}} | unpivot({#null}, \"n\", \"v\")", "must be text")]
    [TestCase("{{x := 1}} | unpivot({\"x\"}, \"n\", \"n\")", "must be distinct")]
    [TestCase("{{x := 1, n := 2}} | unpivot({\"x\"}, \"n\", \"v\")", "Duplicate record field name")]
    [TestCase("{{x := 1, v := #null}} | unpivot({\"x\"}, \"n\", \"v\")", "Duplicate record field name")]
    [TestCase("{{n := 1}} | unpivot({\"missing\"}, \"n\", \"v\")", "Duplicate record field name")]
    [TestCase("{1} | unpivot({\"x\"}, \"n\", \"v\")", "must be a record")]
    [TestCase("{#null} | unpivot({}, \"n\", \"v\")", "must be a record")]
    public void InvalidArguments_RejectDuringEvaluation(string expression, string diagnostic)
        => Assert.That(() => TestExpression.Create(expression).Evaluate(null),
            Throws.ArgumentException.With.Message.Contains(diagnostic));

    [Test]
    public void Arguments_EvaluateOnceInOrderIncludingEmptyInput()
    {
        var calls = new List<string>();
        IFunction<IEnumerable, RecordValue[]?> function = new UnpivotFunction(
            () => { calls.Add("fields"); return ["x"]; },
            () => { calls.Add("name"); return "n"; },
            () => { calls.Add("value"); return "v"; });
        Assert.That(function.Evaluate(System.Array.Empty<object>()), Is.Empty);
        Assert.That(calls, Is.EqualTo(new[] { "fields", "name", "value" }));
    }

    [Test]
    public void NestedCall_UsesEnclosingArgumentContext()
    {
        var result = TestExpression.Create("{fields := {\"x\"}, n := \"name\", v := \"value\", rows := {{x := 42}}} | .rows | unpivot(.fields, .n, .v)").Evaluate(null);
        Assert.That(ValueFormatter.Format(result), Is.EqualTo("{{name := \"x\", value := 42}}"));
    }

    [Test]
    public void Metadata_ExposesClosedContract()
    {
        var info = ExpressifIntrospection.Functions.Describe().Single(info => info.Name == "unpivot");
        Assert.That(info.ImplementationType, Is.EqualTo(typeof(UnpivotFunction)));
        Assert.That(info.Input, Is.EqualTo("array"));
        Assert.That(info.Output, Is.EqualTo("array"));
        Assert.That(info.Parameters.Select(parameter => (parameter.Name, parameter.Type, parameter.Optional)),
            Is.EqualTo(new[] { ("fields", "array", false), ("name-field", "text", false), ("value-field", "text", false) }));
        Assert.That(typeof(IFunction<IEnumerable, RecordValue[]?>).IsAssignableFrom(typeof(UnpivotFunction)), Is.True);
    }

    [Test]
    public void BoundExpression_SupportsConcurrentReuse()
    {
        var expression = TestExpression.Create("unpivot({\"x\"}, \"n\", \"v\")");
        Parallel.For(0, 20, index =>
        {
            var input = TestExpression.Create($"{{{{x := {index}}}}}").Evaluate(null);
            Assert.That(ValueFormatter.Format(expression.Evaluate(input)), Is.EqualTo($"{{{{n := \"x\", v := {index}}}}}"));
        });
    }

    [Test]
    public void NullOutputName_Rejects()
        => Assert.That(() => new UnpivotFunction(() => ["x"], () => null!, () => "v").Evaluate(new object[0]),
            Throws.ArgumentException.With.Message.Contains("non-null"));

    [Test]
    public void HostRecord_PreservesPresenceAndDoesNotMutateInput()
    {
        var input = new Dictionary<string, object?> { ["id"] = 1, ["x"] = null };
        var function = new UnpivotFunction(() => ["x", "missing"], () => "n", () => "v");
        Assert.That(ValueFormatter.Format(function.Evaluate(new[] { input })), Is.EqualTo("{{id := 1, n := \"x\", v := null}}"));
        Assert.That(input.Keys, Is.EqualTo(new[] { "id", "x" }));
    }
}
