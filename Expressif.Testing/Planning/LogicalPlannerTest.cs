using Expressif.Planning;
using Expressif.Syntax;

namespace Expressif.Testing.Planning;

public class LogicalPlannerTest
{
    [Test]
    public void Plan_AliasAndCanonicalSyntax_ProduceEquivalentCalls()
    {
        var canonical = LogicalPlanner.Plan(ExpressionParser.Parse("upper"));
        var alias = LogicalPlanner.Plan(ExpressionParser.Parse("text-to-upper"));

        Assert.Multiple(() =>
        {
            Assert.That(((LogicalCall)alias.Pipeline.Items.Single()).Function,
                Is.EqualTo(((LogicalCall)canonical.Pipeline.Items.Single()).Function));
            Assert.That(((LogicalCall)alias.Pipeline.Items.Single()).Arguments, Is.Empty);
        });
    }

    [Test]
    public void Plan_NamedArguments_AreOrderedByCanonicalParameters()
    {
        var call = SingleCall("add(times := 2, value := 5)");

        Assert.That(call.Arguments.Select(argument => argument.Parameter.Name),
            Is.EqualTo(new[] { "value", "times" }));
        Assert.That(call.Arguments.Select(argument => ((LogicalLiteral)argument.Value!).Value),
            Is.EqualTo(new object[] { 5m, 2m }));
    }

    [Test]
    public void Plan_OmittedArgument_PreservesCatalogOmissionInsteadOfMaterializingDefault()
    {
        var call = SingleCall("add(5)");
        var omitted = call.Arguments.Single(argument => argument.Parameter.Name == "times");

        Assert.Multiple(() =>
        {
            Assert.That(omitted.IsExplicit, Is.False);
            Assert.That(omitted.Value, Is.Null);
            Assert.That(omitted.Omission?.Mode, Is.EqualTo(global::Expressif.Functions.Catalog.ParameterOmissionMode.Constant));
            Assert.That(omitted.Omission?.Value.GetDecimal(), Is.EqualTo(1m));
        });
    }

    [Test]
    public void Plan_SpreadArgument_PreservesSpreadOnCanonicalParameter()
    {
        var call = SingleCall("array(...{1, 2})");

        Assert.Multiple(() =>
        {
            Assert.That(call.Function.Name, Is.EqualTo("array"));
            Assert.That(call.Arguments.Single().Parameter.Name, Is.EqualTo("values"));
            Assert.That(call.Arguments.Single().IsSpread, Is.True);
        });
    }

    [TestCase(".age", "field", 0)]
    [TestCase("^.age", "field", 1)]
    [TestCase("$1", "tuple-at", 0)]
    public void Plan_ReferenceSyntax_UsesOrdinaryCatalogCalls(string source, string name, int contextDepth)
    {
        var call = SingleCall(source);

        Assert.Multiple(() =>
        {
            Assert.That(call.Function.Name, Is.EqualTo(name));
            Assert.That(call.ContextDepth, Is.EqualTo(contextDepth));
            if (name == "tuple-at")
                Assert.That(((LogicalLiteral)call.Arguments.Single().Value!).Type, Is.EqualTo("integer"));
        });
    }

    [TestCase("{1, 2, 3}", "array")]
    [TestCase("T(1, 2)", "tuple")]
    public void Plan_ValueConstructorSyntax_UsesOrdinaryCalls(string source, string name)
    {
        var plan = LogicalPlanner.Plan(ExpressionParser.Parse(source));

        Assert.That(((LogicalCall)plan.Pipeline.Items.Single()).Function.Name, Is.EqualTo(name));
    }

    [Test]
    public void Plan_DoesNotApplyRuntimeCoercionInsertion()
    {
        var plan = LogicalPlanner.Plan(ExpressionParser.Parse("split(\",\") | length"));

        Assert.That(plan.Pipeline.Items.OfType<LogicalCall>().Select(call => call.Function.Name),
            Is.EqualTo(new[] { "split", "length" }));
    }

    private static LogicalCall SingleCall(string source)
        => (LogicalCall)LogicalPlanner.Plan(ExpressionParser.Parse(source)).Pipeline.Items.Single();
}
