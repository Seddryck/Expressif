using Expressif.Bindings;
using System.Diagnostics;

namespace Expressif.Testing.Bindings;

public class PredicationBinderTest
{
    [SetUp]
    public void Setup()
    { }

    [Test]
    [TestCase("is-func(foo, @bar)", 1)]
    [TestCase("is-func", 1)]
    [TestCase("!is-func(foo)", 2)]
    [TestCase(" ! is-func(foo)", 2)]
    [TestCase("(is-func(foo))", 2)]
    [TestCase("is-func(foo) |AND is-foo", 2)]
    [TestCase("(is-func(foo) |AND is-foo)", 2)]
    [TestCase("(is-func(foo) |AND is-foo) |OR bar(123)", 2)]
    [TestCase("(is-func(foo) |AND is-foo) |OR !bar(123)", 2)]
    public void Parse_Predication_Valid(string value, int count)
        => Assert.That(BindingTestAdapter.Predication(value), Is.Not.Null);

    [Test]
    [TestCase("123 | !equal-to(125)")]
    [TestCase("123 | ! equal-to(125) ")]
    [TestCase("123 | !equal-to(125) |OR even ")]
    [TestCase("123 | ( ! equal-to(125) ) ")]
    [TestCase("123 | ( ! equal-to(125) |OR even ) |AND !null ")]
    public void Parse_ParametrizedPredication_Valid(string value)
        => Assert.That(BindingTestAdapter.Predication(value), Is.Not.Null);

    [Test]
    [TestCase("is-func")]
    [TestCase("is-func(foo, @bar)")]
    public void Parse_SinglePredication_Valid(string value)
        => Assert.That(BindingTestAdapter.Predication(value), Is.Not.Null);

    [Test]
    [TestCase("!is-func")]
    [TestCase("!is-func(foo, @bar)")]
    public void Parse_UnaryPredication_Valid(string value)
        => Assert.That(BindingTestAdapter.Predication(value), Is.Not.Null);

    [Test]
    [TestCase("is-func(foo) |AND is-foo")]
    public void Parse_BinaryPredication_Valid(string value)
        => Assert.That(BindingTestAdapter.Predication(value), Is.Not.Null);

    public void Parse_NotShorthand_LowersToCombinator()
    {
        var predication = (SinglePredication)BindingTestAdapter.Predication("!even");

        Assert.That(predication.Members.Last().Name, Is.EqualTo("not"));
    }

    [TestCase("even |AND odd", "and")]
    [TestCase("even |OR odd", "or")]
    [TestCase("even |XOR odd", "xor")]
    public void Parse_BinaryShorthand_PreservesBothOperands(string value, string expected)
    {
        var predication = (BinaryPredication)BindingTestAdapter.Predication(value);

        Assert.Multiple(() =>
        {
            Assert.That(predication.Operator.Name, Is.EqualTo(expected));
            Assert.That(predication.LeftMember, Is.TypeOf<SinglePredication>());
            Assert.That(predication.RightMember, Is.TypeOf<SinglePredication>());
        });
    }

    [Test]
    [TestCase("(is-func)")]
    public void Parse_SubPredication_Valid(string value)
        => Assert.That(BindingTestAdapter.Predication(value), Is.Not.Null);
}
