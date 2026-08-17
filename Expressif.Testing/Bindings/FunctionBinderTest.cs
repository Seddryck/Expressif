using Expressif.Bindings;
using System.Diagnostics;
using System.Linq;

namespace Expressif.Testing.Bindings;

public class FunctionBinderTest
{
    [TestCase(".name", "name")]
    [TestCase(".birth-date", "birth-date")]
    [TestCase(".amount_tax", "amount_tax")]
    public void Parse_FieldShorthand_LowersToFieldFunction(string value, string expectedName)
    {
        var function = BindingTestAdapter.Function(value);

        Assert.Multiple(() =>
        {
            Assert.That(function.Name, Is.EqualTo("field"));
            Assert.That(function.Syntax, Is.EqualTo(FunctionSyntax.FieldShorthand));
            Assert.That(((LiteralParameter)function.Parameters.Single()).Value, Is.EqualTo(expectedName));
        });
    }

    [SetUp]
    public void Setup()
    { }

    [Test]
    [TestCase("text-to-func(\"foo\")", 1)]
    [TestCase("text-to-func()", 0)]
    [TestCase("text-to-func", 0)]
    [TestCase("text-to-func(\"foo\", @bar)", 2)]
    public void Parse_Function_Valid(string value, int count)
    {
        var function = BindingTestAdapter.Function(value);
        Assert.Multiple(() =>
        {
            Assert.That(function.Name, Is.EqualTo("text-to-func"));
            Assert.That(function.Parameters.Count, Is.EqualTo(count));
        });
    }

    [Test]
    public void Parse_Function_MapWithOpenExpressionParameter_Valid()
    {
        var function = BindingTestAdapter.Function("map(upper | first-chars(2))");

        Assert.That(function.Name, Is.EqualTo("map"));
        Assert.That(function.Parameters, Has.Length.EqualTo(1));
        Assert.That(function.Parameters[0], Is.TypeOf<OpenExpressionParameter>());

        var parameter = (OpenExpressionParameter)function.Parameters[0];
        Assert.That(parameter.Expression.Members.Select(x => x.Name), Is.EqualTo(new[] { "upper", "first-chars" }));
    }

    [Test]
    public void Parse_Function_MapWithPredicateParameter_Valid()
    {
        var function = BindingTestAdapter.Function("map(even)");

        Assert.That(function.Parameters, Has.Length.EqualTo(1));
        Assert.That(function.Parameters[0], Is.TypeOf<OpenExpressionParameter>());
        Assert.That(((OpenExpressionParameter)function.Parameters[0]).Expression.Members.Single().Name, Is.EqualTo("even"));
    }

    [Test]
    public void Parse_Function_FilterWithExpressionParameter_Valid()
    {
        var function = BindingTestAdapter.Function("filter(greater-than(2))");

        Assert.That(function.Name, Is.EqualTo("filter"));
        Assert.That(function.Parameters, Has.Length.EqualTo(1));
        Assert.That(function.Parameters[0], Is.TypeOf<OpenExpressionParameter>());

        var parameter = (OpenExpressionParameter)function.Parameters[0];
        Assert.That(parameter.Expression.Members.Single().Name, Is.EqualTo("greater-than"));
    }

    [Test]
    public void Parse_Function_FilterWithAndPredicate_Valid()
    {
        var function = BindingTestAdapter.Function("filter(greater-than(2) |AND less-than(5))");

        Assert.That(function.Name, Is.EqualTo("filter"));
        Assert.That(function.Parameters, Has.Length.EqualTo(1));
        Assert.That(function.Parameters[0], Is.TypeOf<PredicationParameter>());

        var parameter = (PredicationParameter)function.Parameters[0];
        Assert.That(parameter.Predication.GetType().Name, Is.EqualTo("BinaryPredication"));

        var left = parameter.Predication.GetType().GetProperty("LeftMember")?.GetValue(parameter.Predication);
        var right = parameter.Predication.GetType().GetProperty("RightMember")?.GetValue(parameter.Predication);
        var @operator = parameter.Predication.GetType().GetProperty("Operator")?.GetValue(parameter.Predication);

        Assert.That(@operator?.GetType().GetProperty("Name")?.GetValue(@operator)?.ToString(), Is.EqualTo("AND"));

        Assert.That(left, Is.TypeOf<SinglePredication>());
        var leftFunction = ((SinglePredication)left!).Members.Single();
        Assert.That(leftFunction.Name, Is.EqualTo("greater-than"));
        Assert.That(leftFunction.Parameters, Has.Length.EqualTo(1));
        Assert.That(((LiteralParameter)leftFunction.Parameters[0]).Value, Is.EqualTo(2m));

        Assert.That(right, Is.TypeOf<SinglePredication>());
        var rightFunction = ((SinglePredication)right!).Members.Single();
        Assert.That(rightFunction.Name, Is.EqualTo("less-than"));
        Assert.That(rightFunction.Parameters, Has.Length.EqualTo(1));
        Assert.That(((LiteralParameter)rightFunction.Parameters[0]).Value, Is.EqualTo(5m));
    }

    [Test]
    public void Parse_Function_MapWithTwoParametersFunction_Valid()
    {
        var function = BindingTestAdapter.Function("map(add(10,2))");

        Assert.That(function.Name, Is.EqualTo("map"));
        Assert.That(function.Parameters, Has.Length.EqualTo(1));
        Assert.That(function.Parameters[0], Is.TypeOf<OpenExpressionParameter>());

        var parameter = (OpenExpressionParameter)function.Parameters[0];
        Assert.That(parameter.Expression.Members.Count(), Is.EqualTo(1));
        Assert.That(parameter.Expression.Members.Single().Name, Is.EqualTo("add"));
        Assert.That(parameter.Expression.Members.Single().Parameters, Has.Length.EqualTo(2));
    }

    [Test]
    public void Parse_Function_MapWithThreeFunctionsMixedArity_Valid()
    {
        var function = BindingTestAdapter.Function("map(increment | add(10) | add(10,2))");

        Assert.That(function.Name, Is.EqualTo("map"));
        Assert.That(function.Parameters, Has.Length.EqualTo(1));
        Assert.That(function.Parameters[0], Is.TypeOf<OpenExpressionParameter>());

        var parameter = (OpenExpressionParameter)function.Parameters[0];
        var members = parameter.Expression.Members.ToArray();

        Assert.That(members, Has.Length.EqualTo(3));
        Assert.That(members[0].Name, Is.EqualTo("increment"));
        Assert.That(members[0].Parameters, Has.Length.EqualTo(0));
        Assert.That(members[1].Name, Is.EqualTo("add"));
        Assert.That(members[1].Parameters, Has.Length.EqualTo(1));
        Assert.That(members[2].Name, Is.EqualTo("add"));
        Assert.That(members[2].Parameters.Length, Is.GreaterThanOrEqualTo(2));
    }

    [TestCase("some(less-than(5))")]
    [TestCase("some(.active)")]
    [TestCase("some(.age | less-than(18))")]
    [TestCase("map(.name | upper)")]
    public void Parse_Function_WithExpressionParameter_Valid(string value)
    {
        var function = BindingTestAdapter.Function(value);

        Assert.That(function.Parameters, Has.Length.EqualTo(1));
        Assert.That(function.Parameters[0], Is.TypeOf<OpenExpressionParameter>());
    }

    [Test]
    public void Parse_Function_WithMultipleExpressionParameters_Valid()
    {
        var function = BindingTestAdapter.Function(
            "coalesce(.nickname, field(name), .display-name | upper)");

        Assert.That(function.Parameters, Has.Length.EqualTo(3));
        Assert.That(function.Parameters, Has.All.TypeOf<OpenExpressionParameter>());
    }

    [Test]
    public void Parse_Function_WithNestedLongFormExpressionParameters_Valid()
    {
        var function = BindingTestAdapter.Function(
            "coalesce(field(nickname), field(name))");

        var expressions = function.Parameters.Cast<OpenExpressionParameter>().ToArray();
        Assert.That(expressions, Has.Length.EqualTo(2));
        Assert.That(expressions.Select(x => x.Expression.Members.Single().Name),
            Is.EqualTo(new[] { "field", "field" }));
    }

    [TestCase("\"value\"", typeof(QuotedLiteralParameter))]
    [TestCase("\"name\"", typeof(QuotedLiteralParameter))]
    [TestCase("@foo", typeof(VariableParameter))]
    [TestCase("^.name", typeof(ObjectPropertyParameter))]
    public void Parse_Function_WithScalarParameter_PreservesParameterType(string value, Type expectedType)
    {
        var function = BindingTestAdapter.Function($"example({value})");

        Assert.That(function.Parameters.Single(), Is.TypeOf(expectedType));
    }

    [Test]
    public void Parse_Function_RecordWithEntries_Valid()
    {
        var function = BindingTestAdapter.Function("record(..., name := field(name) | upper, original := ..., active := #true)");

        Assert.That(function.Name, Is.EqualTo("record"));
        Assert.That(function.Parameters, Has.Length.EqualTo(1));
        Assert.That(function.Parameters[0], Is.TypeOf<RecordDefinitionParameter>());

        var parameter = (RecordDefinitionParameter)function.Parameters[0];
        Assert.That(parameter.Entries, Has.Length.EqualTo(4));
        Assert.That(parameter.Entries[0], Is.TypeOf<RecordSpreadEntry>());
        Assert.That(parameter.Entries[1], Is.TypeOf<RecordNamedEntry>());
        Assert.That(parameter.Entries[2], Is.TypeOf<RecordNamedEntry>());
        Assert.That(parameter.Entries[3], Is.TypeOf<RecordNamedEntry>());
    }

    [Test]
    public void Parse_Function_RecordWithTrailingComma_Valid()
    {
        var function = BindingTestAdapter.Function("record(name := \"Alice\",)");

        Assert.That(function.Name, Is.EqualTo("record"));
        Assert.That(function.Parameters, Has.Length.EqualTo(1));
        Assert.That(function.Parameters[0], Is.TypeOf<RecordDefinitionParameter>());
    }
}
