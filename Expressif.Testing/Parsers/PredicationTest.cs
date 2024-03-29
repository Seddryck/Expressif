using Expressif.Parsers;
using Sprache;
using System.Diagnostics;

namespace Expressif.Testing.Parsers;

public class PredicationTest
{
    [SetUp]
    public void Setup()
    { }

    [Test]
    [TestCase("is-func(foo, @bar)", 1)]
    [TestCase("is-func", 1)]
    [TestCase("!is-func(foo)", 2)]
    [TestCase(" ! is-func(foo)", 2)]
    [TestCase("{is-func(foo)}", 2)]
    [TestCase("is-func(foo) |AND is-foo", 2)]
    [TestCase("{is-func(foo) |AND is-foo}", 2)]
    [TestCase("{is-func(foo) |AND is-foo} |OR bar(123)", 2)]
    [TestCase("{is-func(foo) |AND is-foo} |OR !bar(123)", 2)]
    public void Parse_Predication_Valid(string value, int count)
        => Assert.That(Expressif.Parsers.Predication.Parser.Parse(value), Is.Not.Null);

    [Test]
    [TestCase("123 |? !equal-to(125)")]
    [TestCase("123 |? ! equal-to(125) ")]
    [TestCase("123 |? !equal-to(125) |OR even ")]
    [TestCase("123 |? { ! equal-to(125) } ")]
    [TestCase("123 |? { ! equal-to(125) |OR even } |AND !null ")]
    public void Parse_ParametrizedPredication_Valid(string value)
        => Assert.That(InputPredication.Parser.Parse(value).Predication, Is.Not.Null);

    [Test]
    [TestCase("is-func")]
    [TestCase("is-func(foo, @bar)")]
    public void Parse_SinglePredication_Valid(string value)
        => Assert.That(SinglePredication.Parser.Parse(value), Is.Not.Null);

    [Test]
    [TestCase("!is-func")]
    [TestCase("!is-func(foo, @bar)")]
    public void Parse_UnaryPredication_Valid(string value)
        => Assert.That(UnaryPredication.Parser.Parse(value), Is.Not.Null);

    [Test]
    [TestCase("is-func(foo) |AND is-foo")]
    public void Parse_BinaryPredication_Valid(string value)
        => Assert.That(BinaryPredication.Parser.Parse(value), Is.Not.Null);

    [Test]
    [TestCase("{is-func}")]
    public void Parse_SubPredication_Valid(string value)
        => Assert.That(SubPredication.Parser.Parse(value), Is.Not.Null);

}
