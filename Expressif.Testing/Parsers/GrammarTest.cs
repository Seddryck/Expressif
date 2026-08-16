using Expressif.Bindings;
using Expressif.Values;
using System.Diagnostics;

namespace Expressif.Testing.Parsers;

public class GrammarTest
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    [TestCase("foo")]
    [TestCase("foo  ")]
    [TestCase("  foo")]
    [TestCase("FoO")]
    [TestCase("foo-bar")]
    [TestCase("  foo-bar")]
    [TestCase("foo-bar  ")]
    [TestCase("foo-BAr-foo")]
    public void Parse_FunctionName_Valid(string value)
        => Assert.That(BindingTestAdapter.FunctionName(value), Is.EqualTo(value.Trim()));

    [Test]
    [TestCase("1foo")]
    [TestCase("fo1o")]
    [TestCase("foo1")]
    [TestCase("@foo")]
    [TestCase("-foo")]
    [TestCase("foo--bar")]
    [TestCase("foo-")]
    public void Parse_FunctionName_Invalid(string value)
        => Assert.That(() => BindingTestAdapter.FunctionName(value), Throws.TypeOf<ExpressifSyntaxException>());

    [Test]
    [TestCase("|")]
    [TestCase("  |")]
    [TestCase("|  ")]
    public void Parse_Delimitator_Valid(string value)
        => Assert.That(BindingTestAdapter.Delimiter(value), Is.EqualTo('|'));

    [Test]
    [TestCase("@")]
    [TestCase("foo")]
    public void Parse_Delimitator_Invalid(string value)
        => Assert.That(() => BindingTestAdapter.Delimiter(value), Throws.TypeOf<ExpressifSyntaxException>());

    [Test]
    [TestCase("@a")]
    [TestCase("@foo")]
    [TestCase("@foo1")]
    [TestCase("@fo1o")]
    [TestCase("@Foo  ")]
    [TestCase("  @Foo")]
    public void Parse_Variable_Valid(string value)
        => Assert.That(BindingTestAdapter.Variable(value), Is.EqualTo(value.Trim().TrimStart('@')));

    [Test]
    [TestCase("@")]
    [TestCase("foo")]
    [TestCase("@foo-1")]
    [TestCase("@ foo")]
    [TestCase("@1foo")]
    public void Parse_Variable_Invalid(string value)
        => Assert.That(() => BindingTestAdapter.Delimiter(value), Throws.TypeOf<ExpressifSyntaxException>());

    [Test]
    [TestCase("foo")]
    [TestCase("  foo")]
    [TestCase("foo  ")]
    [TestCase("foo-bar")]
    [TestCase("%foo!bar")]
    [TestCase("\"foo\"")]
    [TestCase("\"\"")]
    [TestCase("\" foo bar \"")]
    [TestCase("\"foo , bar\"")]
    [TestCase("`foo`")]
    [TestCase("` foo bar `")]
    [TestCase("`foo , bar`")]
    [TestCase("`(foo)`")]
    public void Parse_Literal_Valid(string value)
        => Assert.That(BindingTestAdapter.Literal(value), Is.EqualTo(value.Trim().Trim('\"').Trim('`')));

    [Test]
    public void Parse_Literal_DoubleQuotedEscapedCharacters_Unescaped()
        => Assert.That(BindingTestAdapter.Literal("\"Alice said \\\"hello\\\".\""), Is.EqualTo("Alice said \"hello\"."));

    [Test]
    [TestCase("@foo")]
    [TestCase("foo bar")]
    [TestCase("foo , bar")]
    [TestCase("(foo)")]
    public void Parse_Literal_Invalid(string value)
        => Assert.That(() => BindingTestAdapter.Literal(value), Throws.TypeOf<ExpressifSyntaxException>());
}
