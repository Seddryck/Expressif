using Expressif.Predicates.Boolean;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Predicates.Boolean;

[TestFixture]
public class BooleanCombinatorsTest
{
    [Conformance]
    public void And_Valid_Expression(object? value, object? expression, bool expected)
        => Assert.That(new And(() => expression).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Or_Valid_Expression(object? value, object? expression, bool expected)
        => Assert.That(new Or(() => expression).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Xor_Valid_Expression(object? value, object? expression, bool expected)
        => Assert.That(new Xor(() => expression).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Not_Valid(object? value, bool expected)
        => Assert.That(new Not().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Nand_Valid_Expression(bool value, bool expression, bool expected)
        => Assert.That(new Nand(() => expression).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Nor_Valid_Expression(bool value, bool expression, bool expected)
        => Assert.That(new Nor(() => expression).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Xnor_Valid_Expression(bool value, bool expression, bool expected)
        => Assert.That(new Xnor(() => expression).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Implies_Valid_Expression(bool value, bool expression, bool expected)
        => Assert.That(new Implies(() => expression).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Majority_Zero(object? value, bool expected)
        => Assert.That(new Majority([]).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Majority_One(object? value, bool first, bool expected)
        => Assert.That(new Majority([() => first]).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Majority_Two(object? value, bool first, bool second, bool expected)
        => Assert.That(new Majority([() => first, () => second]).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Majority_Three(object? value, bool first, bool second, bool third, bool expected)
        => Assert.That(new Majority([() => first, () => second, () => third]).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Majority_Four(object? value, bool first, bool second, bool third, bool fourth, bool expected)
        => Assert.That(new Majority([() => first, () => second, () => third, () => fourth]).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Majority_Five(object? value, bool first, bool second, bool third, bool fourth, bool fifth, bool expected)
        => Assert.That(new Majority([() => first, () => second, () => third, () => fourth, () => fifth]).Evaluate(value), Is.EqualTo(expected));

    [Test]
    public void And_FalseInput_DoesNotEvaluateExpression()
        => Assert.That(
            () => new And(() => throw new InvalidOperationException()).Evaluate(false),
            Throws.Nothing);

    [Test]
    public void Or_TrueInput_DoesNotEvaluateExpression()
        => Assert.That(
            () => new Or(() => throw new InvalidOperationException()).Evaluate(true),
            Throws.Nothing);

    [Test]
    public void Xor_Input_EvaluatesExpression()
        => Assert.That(
            () => new Xor(() => throw new InvalidOperationException()).Evaluate(false),
            Throws.TypeOf<InvalidOperationException>());

    [Test]
    public void Nand_FalseInput_DoesNotEvaluateExpression()
        => Assert.That(
            () => new Nand(() => throw new InvalidOperationException()).Evaluate(false),
            Throws.Nothing);

    [Test]
    public void Nor_TrueInput_DoesNotEvaluateExpression()
        => Assert.That(
            () => new Nor(() => throw new InvalidOperationException()).Evaluate(true),
            Throws.Nothing);

    [Test]
    public void Xnor_Input_EvaluatesExpression()
        => Assert.That(
            () => new Xnor(() => throw new InvalidOperationException()).Evaluate(false),
            Throws.TypeOf<InvalidOperationException>());

    [Test]
    public void Implies_FalseInput_DoesNotEvaluateExpression()
        => Assert.That(
            () => new Implies(() => throw new InvalidOperationException()).Evaluate(false),
            Throws.Nothing);

    [Test]
    public void Majority_ResultKnown_DoesNotEvaluateRemainingPredicates()
        => Assert.That(
            () => new Majority([() => true, () => true, () => throw new InvalidOperationException()]).Evaluate(null),
            Throws.Nothing);

    [TestCase("majority()", 3, false)]
    [TestCase("majority(is-positive)", 3, true)]
    [TestCase("majority(is-positive, is-even, is-less-than(10))", 4, true)]
    public void Majority_ParsedAndEvaluated(string code, object? value, bool expected)
        => Assert.That(new Predication(code).Evaluate(value), Is.EqualTo(expected));

    [TestCase("and(#true)", true, true)]
    [TestCase("or(#false)", false, false)]
    [TestCase("xor(#true)", false, true)]
    [TestCase("not", true, false)]
    [TestCase("nand(#true)", true, false)]
    [TestCase("nor(#false)", false, true)]
    [TestCase("xnor(#true)", true, true)]
    [TestCase("implies(#false)", true, false)]
    public void PredicateName_ParsedAndEvaluated(string code, object? value, bool expected)
        => Assert.That(new Predication(code).Evaluate(value), Is.EqualTo(expected));
}
