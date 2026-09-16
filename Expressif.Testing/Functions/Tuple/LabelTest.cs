using Expressif.Functions;
using Expressif.Functions.Tuple;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Functions.Tuple;

public class LabelTest
{
    [Conformance]
    public void Label_Valid(string value, string[] names, string expected)
        => AssertResult("label", value, names, expected);

    [Conformance]
    public void LabelConflicts_Valid(string value, string[] names, string expected)
        => AssertResult("label-conflicts", value, names, expected);

    [Test]
    public void Label_ExposesClosedTypedContract()
    {
        IFunction<IPositionalValue, RecordValue> function = new Label(() => ["value"]);

        Assert.That(
            ValueFormatter.Format(function.Evaluate(new TupleValue(42))),
            Is.EqualTo("{value := 42}"));
    }

    [Test]
    public void Label_NonTupleInput_ReturnsNull()
        => Assert.That(((IFunction)new Label(() => ["value"])).Evaluate(42), Is.Null);

    [Test]
    public void Label_ArityMismatch_Throws()
        => Assert.That(
            () => new Label(() => ["value"]).Evaluate(new TupleValue(1, 2)),
            Throws.ArgumentException.With.Message.StartWith("Number of labels must match tuple arity."));

    [Test]
    public void Label_DuplicateResultingField_Throws()
        => Assert.That(
            () => Expression.CreateClosed("T(1, 2) | label(\"same\", \"same\")").Evaluate(null),
            Throws.ArgumentException.With.Message.EqualTo("Tuple labels produce duplicate field 'same'."));

    [Test]
    public void LabelConflicts_ScalarLabelConflictingWithRecordField_QualifiesRecordField()
        => Assert.That(
            ValueFormatter.Format(Expression.CreateClosed("T(42, {id := 1}) | label-conflicts(\"id\", \"record\")").Evaluate(null)),
            Is.EqualTo("{id := 42, \"record.id\" := 1}"));

    [Test]
    public void Label_ArgumentsUseEnclosingContext()
        => Assert.That(
            ValueFormatter.Format(Expression.CreateClosed(
                "{pair := T(1, 2), first := \"a\", second := \"b\"} | .pair | label(.first, .second)").Evaluate(null)),
            Is.EqualTo("{a := 1, b := 2}"));

    [TestCase("label")]
    [TestCase("label-conflicts")]
    public void Label_SpreadArgumentsAreRejected(string function)
        => Assert.That(
            () => Expression.CreateClosed($"T(1, 2) | {function}(...{{\"a\", \"b\"}})").Evaluate(null),
            Throws.TypeOf<BindingException>()
                .With.Message.EqualTo($"Function '{function}' does not support spread arguments."));

    private static void AssertResult(string function, string value, string[] names, string expected)
    {
        var quotedNames = names.Select(ValueFormatter.Format);
        var call = names.Length == 0 ? function : $"{function}({string.Join(", ", quotedNames)})";
        var result = Expression.Create($"{value} | {call}").Evaluate(null);
        Assert.That(ValueFormatter.Format(result), Is.EqualTo(expected));
    }
}
