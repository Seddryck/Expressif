using Expressif.Functions.Text;
using Expressif.Testing.Conformance;
using Expressif.Values.Special;

namespace Expressif.Testing.Functions.Text;

[TestFixture]
public class CasingFunctionsTests
{
    [Test]
    public void TextCasing_NullInput_ReturnsNull()
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Lower().Evaluate(null), Is.Null);
            Assert.That(new Upper().Evaluate(null), Is.Null);
            Assert.That(new SwapCase().Evaluate(null), Is.Null);
            Assert.That(new SentenceCase().Evaluate(null), Is.Null);
            Assert.That(new TitleCase().Evaluate(null), Is.Null);
        });
    }

    [Test]
    public void WordCasing_NullInput_ReturnsEmpty()
    {
        var expected = new Empty().Keyword;

        Assert.Multiple(() =>
        {
            Assert.That(new PascalCase().Evaluate(null), Is.EqualTo(expected));
            Assert.That(new CamelCase().Evaluate(null), Is.EqualTo(expected));
            Assert.That(new KebabCase().Evaluate(null), Is.EqualTo(expected));
            Assert.That(new SnakeCase().Evaluate(null), Is.EqualTo(expected));
            Assert.That(new CamelSnakeCase().Evaluate(null), Is.EqualTo(expected));
            Assert.That(new PascalSnakeCase().Evaluate(null), Is.EqualTo(expected));
            Assert.That(new DotCase().Evaluate(null), Is.EqualTo(expected));
            Assert.That(new ScreamingSnakeCase().Evaluate(null), Is.EqualTo(expected));
            Assert.That(new TrainCase().Evaluate(null), Is.EqualTo(expected));
            Assert.That(new FlatCase().Evaluate(null), Is.EqualTo(expected));
            Assert.That(new AllcapsCase().Evaluate(null), Is.EqualTo(expected));
            Assert.That(new CobolCase().Evaluate(null), Is.EqualTo(expected));
            Assert.That(new PathCase().Evaluate(null), Is.EqualTo(expected));
            Assert.That(new NamespaceCase().Evaluate(null), Is.EqualTo(expected));
        });
    }

    [Test]
    public void TextCasing_BlankInput_ReturnsBlank()
    {
        var expected = new Whitespace().Keyword;

        Assert.Multiple(() =>
        {
            Assert.That(new Lower().Evaluate("   "), Is.EqualTo(expected));
            Assert.That(new Upper().Evaluate("   "), Is.EqualTo(expected));
            Assert.That(new SwapCase().Evaluate("   "), Is.EqualTo(expected));
            Assert.That(new SentenceCase().Evaluate("   "), Is.EqualTo(expected));
            Assert.That(new TitleCase().Evaluate("   "), Is.EqualTo(expected));
        });
    }

    [Test]
    public void WordCasing_BlankInput_ReturnsEmpty()
    {
        var expected = new Empty().Keyword;

        Assert.Multiple(() =>
        {
            Assert.That(new PascalCase().Evaluate("   "), Is.EqualTo(expected));
            Assert.That(new CamelCase().Evaluate("   "), Is.EqualTo(expected));
            Assert.That(new KebabCase().Evaluate("   "), Is.EqualTo(expected));
            Assert.That(new SnakeCase().Evaluate("   "), Is.EqualTo(expected));
            Assert.That(new CamelSnakeCase().Evaluate("   "), Is.EqualTo(expected));
            Assert.That(new PascalSnakeCase().Evaluate("   "), Is.EqualTo(expected));
            Assert.That(new DotCase().Evaluate("   "), Is.EqualTo(expected));
            Assert.That(new ScreamingSnakeCase().Evaluate("   "), Is.EqualTo(expected));
            Assert.That(new TrainCase().Evaluate("   "), Is.EqualTo(expected));
            Assert.That(new FlatCase().Evaluate("   "), Is.EqualTo(expected));
            Assert.That(new AllcapsCase().Evaluate("   "), Is.EqualTo(expected));
            Assert.That(new CobolCase().Evaluate("   "), Is.EqualTo(expected));
            Assert.That(new PathCase().Evaluate("   "), Is.EqualTo(expected));
            Assert.That(new NamespaceCase().Evaluate("   "), Is.EqualTo(expected));
        });
    }


    [TestCase("{alice, bob}")]
    [TestCase("{`alice`,`bob`}")]
    public void TextCasing_StringArrayLikeInput_UsesArraySemantics(string value)
        => Assert.That(new Upper().Evaluate(value), Is.EqualTo("ALICE BOB"));

    [TestCase("{alice, bob}")]
    [TestCase("{`alice`,`bob`}")]
    public void WordCasing_StringArrayLikeInput_UsesArraySemantics(string value)
        => Assert.That(new PascalCase().Evaluate(value), Is.EqualTo("AliceBob"));

    [TestCase("`{alice, bob}`")]
    public void TextCasing_BacktickWrappedArrayLikeString_UsesStringSemantics(string value)
        => Assert.That(new Upper().Evaluate(), Is.EqualTo("`{ALICE, BOB}`"));

    [Test]
    public void WordCasing_BacktickWrappedArrayLikeString_UsesStringSemantics()
        => Assert.That(new PascalCase().Evaluate("`{alice, bob}`"), Is.EqualTo("`{alice,Bob}`"));

    [Conformance]
    public void Upper_Valid(object value, object expected)
        => Assert.That(new Upper().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Lower_Valid(object value, object expected)
        => Assert.That(new Lower().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void SwapCase_Valid(object? value, string expected)
        => Assert.That(new SwapCase().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void SentenceCase_Valid(object? value, string expected)
        => Assert.That(new SentenceCase().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void TitleCase_Valid(object? value, string expected)
        => Assert.That(new TitleCase().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void PascalCase_Valid(object? value, string expected)
        => Assert.That(new PascalCase().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void CamelCase_Valid(object? value, string expected)
        => Assert.That(new CamelCase().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void KebabCase_Valid(object? value, string expected)
        => Assert.That(new KebabCase().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void SnakeCase_Valid(object? value, string expected)
        => Assert.That(new SnakeCase().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void CamelSnakeCase_Valid(object? value, string expected)
        => Assert.That(new CamelSnakeCase().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void PascalSnakeCase_Valid(object? value, string expected)
        => Assert.That(new PascalSnakeCase().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void DotCase_Valid(object? value, string expected)
        => Assert.That(new DotCase().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void ScreamingSnakeCase_Valid(object? value, string expected)
        => Assert.That(new ScreamingSnakeCase().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void TrainCase_Valid(object? value, string expected)
        => Assert.That(new TrainCase().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void FlatCase_Valid(object? value, string expected)
        => Assert.That(new FlatCase().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void AllcapsCase_Valid(object? value, string expected)
        => Assert.That(new AllcapsCase().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void CobolCase_Valid(object? value, string expected)
        => Assert.That(new CobolCase().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void PathCase_Valid(object? value, string expected)
        => Assert.That(new PathCase().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void NamespaceCase_Valid(object? value, string expected)
        => Assert.That(new NamespaceCase().Evaluate(value), Is.EqualTo(expected));

}
