using Expressif.Parsers;
using Sprache;
using System.Diagnostics;

namespace Expressif.Testing.Parsers;

public class FunctionTest
{
    [SetUp]
    public void Setup()
    { }

    [Test]
    [TestCase("text-to-func(foo)", 1)]
    [TestCase("text-to-func()", 0)]
    [TestCase("text-to-func", 0)]
    [TestCase("text-to-func(foo, @bar)", 2)]
    public void Parse_Function_Valid(string value, int count)
    {
        var function = Expressif.Parsers.Function.Parser.Parse(value);
        Assert.Multiple(() =>
        {
            Assert.That(function.Name, Is.EqualTo("text-to-func"));
            Assert.That(function.Parameters.Count, Is.EqualTo(count));
        });
    }
}