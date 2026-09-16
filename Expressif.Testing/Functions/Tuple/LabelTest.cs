using Expressif.Functions.Introspection;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Functions.Tuple;

public class LabelTest
{
    [Conformance]
    public void Label_Valid(string value, string[] names, string expected)
        => AssertFormatted(value, "label", names, expected);

    [Conformance]
    public void LabelConflicts_Valid(string value, string[] names, string expected)
        => AssertFormatted(value, "label-conflicts", names, expected);

    private static void AssertFormatted(string value, string function, string[] names, string expected)
    {
        var arguments = string.Join(", ", names.Select(name => $"\"{name}\""));
        var result = Expression.CreateClosed($"{value} | {function}({arguments})").Evaluate(null);
        Assert.That(ValueFormatter.Format(result), Is.EqualTo(expected));
    }

    [TestCase("T(1, 2) | label(\"a\")")]
    [TestCase("T(1) | label(\"a\", \"b\")")]
    [TestCase("T(1, 2) | label-conflicts(\"a\")")]
    [TestCase("T(1) | label-conflicts(\"a\", \"b\")")]
    public void LabelCount_DifferentFromArity_Throws(string source)
        => Assert.That(
            () => Expression.CreateClosed(source).Evaluate(null),
            Throws.ArgumentException.With.Message.StartsWith("Number of labels must match tuple arity."));

    [TestCase("T(1) | label(1)")]
    [TestCase("T(1) | label(#null)")]
    [TestCase("T(1) | label-conflicts(1)")]
    [TestCase("T(1) | label-conflicts(#null)")]
    public void NonTextLabel_Throws(string source)
        => Assert.That(
            () => Expression.CreateClosed(source).Evaluate(null),
            Throws.ArgumentException.With.Message.StartsWith("Every label must be text."));

    [TestCase("T(1, 2) | label(\"same\", \"same\")")]
    [TestCase("T(1, 2) | label-conflicts(\"same\", \"same\")")]
    [TestCase("T(1, {id := 2}) | label(\"left.id\", \"left\")")]
    public void UnresolvableOutputNameCollision_Throws(string source)
        => Assert.That(
            () => Expression.CreateClosed(source).Evaluate(null),
            Throws.ArgumentException.With.Message.StartsWith("Tuple labels produce duplicate field"));

    [TestCase("label(\"value\")")]
    [TestCase("label-conflicts(\"value\")")]
    public void NonPositionalInput_ReturnsNull(string expression)
        => Assert.That(Expression.Create(expression).Evaluate(42), Is.Null);

    [TestCase("T(1, 2) | label(...{\"left\", \"right\"})", "{left := 1, right := 2}")]
    [TestCase("T(1, 2) | label-conflicts(...{\"left\", \"right\"})", "{left := 1, right := 2}")]
    public void Names_SupportValueSpread(string source, string expected)
        => Assert.That(ValueFormatter.Format(Expression.CreateClosed(source).Evaluate(null)), Is.EqualTo(expected));

    [Test]
    public void LabelConflicts_QualifiesOnlyConflictingRecordFields()
    {
        var result = Expression.CreateClosed(
            "T({key := 1, value := \"Brol\"}, {key := 2, score := 10}) | label-conflicts(\"left\", \"right\")")
            .Evaluate(null);

        Assert.That(
            ValueFormatter.Format(result),
            Is.EqualTo("{left.key := 1, value := \"Brol\", right.key := 2, score := 10}"));
    }

    [Test]
    public void Introspection_ReportsTupleToRecordVariadicTextContract()
    {
        var infos = new FunctionIntrospector().Describe()
            .Where(info => info.Name is "label" or "label-conflicts")
            .OrderBy(info => info.Name)
            .ToArray();

        Assert.That(infos, Has.Length.EqualTo(2));
        foreach (var info in infos)
        {
            Assert.Multiple(() =>
            {
                Assert.That(info.Input, Is.EqualTo("tuple"));
                Assert.That(info.Output, Is.EqualTo("record"));
                Assert.That(info.Parameters, Has.Length.EqualTo(1));
                Assert.That(info.Parameters[0].Name, Is.EqualTo("names"));
                Assert.That(info.Parameters[0].Type, Is.EqualTo("text"));
                Assert.That(info.Parameters[0].Optional, Is.True);
                Assert.That(info.Parameters[0].Variadic, Is.True);
                Assert.That(info.Parameters[0].MinimumCardinality, Is.Zero);
            });
        }
    }
}
