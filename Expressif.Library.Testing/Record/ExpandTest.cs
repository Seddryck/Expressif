using Expressif.Introspection;
using Expressif.Bindings;
using Expressif.Functions;
using Expressif.Discovery;
using Expressif.Library.Numeric;
using Expressif.Library.Record;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Record;

public class ExpandTest
{
    [Conformance]
    public void Expand_Valid_Record(object? value, string expression, string expected)
        => Assert.That(TestExpression.Create(expression).Evaluate(value)?.ToString(), Is.EqualTo(expected));

    [Conformance]
    public void Expand_Invalid_Selection(object? value, string expression, object? expected)
        => Assert.That(TestExpression.Create(expression).Evaluate(value), Is.EqualTo(expected));

    [TestCase("expand(.customer | select-fields({\"name\"}))")]
    [TestCase("expand({name := 1})")]
    [TestCase("expand(field(\"customer\"))")]
    [TestCase("expand()")]
    [TestCase("expand(.customer, \"buyer\", \"extra\")")]
    [TestCase("expand(unknown := .customer)")]
    [TestCase("expand(...{1, 2})")]
    public void InvalidArguments_FailBinding(string expression)
        => Assert.That(() => TestExpression.Create(expression), Throws.InstanceOf<BindingException>());

    [Test]
    public void ComputedSelectorDiagnostic_RequiresExplicitLabel()
        => Assert.That(() => TestExpression.Create("expand(.customer | identity)"),
            Throws.TypeOf<BindingException>().With.Message.Contains("explicit label"));

    [TestCase("#null")]
    [TestCase("42")]
    [TestCase("\"text\"")]
    public void NonRecordInput_IsRejected(string input)
        => Assert.That(() => TestExpression.Create($"{input} | expand(.customer)").Evaluate(null),
            Throws.InstanceOf<ArgumentException>());

    [TestCase("42")]
    [TestCase("#null")]
    public void NonTextLabel_IsRejected(string label)
        => Assert.That(() => TestExpression.Create($"{{customer := {{name := 1}}}} | expand(.customer, {label})").Evaluate(null),
            Throws.TypeOf<InvalidOperationException>().With.Message.Contains("must return text"));

    [Test]
    public void Label_ReadsIncomingRecord()
        => Assert.That(TestExpression.Create("{label := \"old\", customer := {name := 1}} | put(label := \"buyer\") | expand(.customer, .label)")
            .Evaluate(null)?.ToString(), Is.EqualTo("{label := \"buyer\", \"buyer.name\" := 1}"));

    [Test]
    public void EmptyLabel_QualifiesFields()
        => Assert.That(TestExpression.Create("{customer := {name := 1}} | expand(.customer, \"\")").Evaluate(null)?.ToString(),
            Is.EqualTo("{\".name\" := 1}"));

    [Test]
    public void Evaluation_PreservesValuesOrderAndInput()
    {
        var deeper = new RecordValue();
        deeper.Set("country", "BE");
        var nested = new RecordValue();
        nested.Set("address", deeper);
        nested.Set("nullable", null);
        var input = new RecordValue();
        input.Set("head", 1);
        input.Set("customer", nested);
        input.Set("tail", 2);
        var result = (RecordValue)TestExpression.Create("expand(.customer)").Evaluate(input)!;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Keys, Is.EqualTo(new[] { "head", "address", "nullable", "tail" }));
            Assert.That(result["address"], Is.SameAs(deeper));
            Assert.That(result["nullable"], Is.Null);
            Assert.That(input["customer"], Is.SameAs(nested));
            Assert.That(input.Keys, Is.EqualTo(new[] { "head", "customer", "tail" }));
        }
    }

    [Test]
    public void Arguments_AreEvaluatedOnceInOrder()
    {
        var calls = new List<string>();
        var nested = new RecordValue();
        nested.Set("name", 1);
        var input = new RecordValue();
        input.Set("customer", nested);
        var function = new Expand(new RecordExpansionSelector("customer", value =>
        {
            Assert.That(value, Is.SameAs(input));
            calls.Add("selector");
            return nested;
        }), value =>
        {
            Assert.That(value, Is.SameAs(input));
            calls.Add("label");
            return "buyer";
        });
        Assert.That(function.Evaluate(input)?.Keys, Is.EqualTo(new[] { "buyer.name" }));
        Assert.That(calls, Is.EqualTo(new[] { "selector", "label" }));
    }

    [Test]
    public void TypedEvaluation_ExposesRecordContract()
    {
        var nested = new RecordValue();
        nested.Set("name", "Alice");
        IFunction<RecordValue, RecordValue?> function = new Expand(new RecordExpansionSelector(null, _ => nested), _ => "buyer");
        Assert.That(function.Evaluate(new RecordValue())?.Keys, Is.EqualTo(new[] { "buyer.name" }));
    }

    [Test]
    public void BoundExpression_CanBeReusedConcurrently()
    {
        var expression = TestExpression.Create("expand(.customer, .label)");
        Parallel.For(0, 30, i =>
        {
            var nested = new RecordValue();
            nested.Set("name", i);
            var input = new RecordValue();
            input.Set("label", $"buyer{i}");
            input.Set("customer", nested);
            var result = (RecordValue)expression.Evaluate(input)!;
            Assert.That(result[$"buyer{i}.name"], Is.EqualTo(i));
            Assert.That(result.ContainsKey("customer"), Is.False);
        });
    }

    [Test]
    public void Introspection_DescribesClosedContractAndOptionalLabel()
    {
        var info = ExpressifIntrospection.Functions.Describe().Single(info => info.Name == "expand");
        using (Assert.EnterMultipleScope())
        {
            Assert.That(info.ImplementationType, Is.EqualTo(typeof(Expand)));
            Assert.That(info.Input, Is.EqualTo("record"));
            Assert.That(info.Output, Is.EqualTo("record"));
            Assert.That(info.Parameters.Select(parameter => parameter.Name), Is.EqualTo(new[] { "selector", "label" }));
            Assert.That(info.Parameters.Select(parameter => parameter.Type), Is.EqualTo(new[] { "expression", "text" }));
            Assert.That(info.Parameters.Select(parameter => parameter.Optional), Is.EqualTo(new[] { false, true }));
        }
    }
}

