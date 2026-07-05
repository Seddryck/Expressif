using Expressif.Parsers;
using Sprache;
using System.Diagnostics;
using System.Linq;

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

    [Test]
    public void Parse_Function_MapWithOpenExpressionParameter_Valid()
    {
        var function = Expressif.Parsers.Function.Parser.Parse("map(upper | first-chars(2))");

        Assert.That(function.Name, Is.EqualTo("map"));
        Assert.That(function.Parameters, Has.Length.EqualTo(1));
        Assert.That(function.Parameters[0], Is.TypeOf<OpenExpressionParameter>());

        var parameter = (OpenExpressionParameter)function.Parameters[0];
        Assert.That(parameter.Expression.Members.Select(x => x.Name), Is.EqualTo(new[] { "upper", "first-chars" }));
    }

    [Test]
    public void Parse_Function_MapWithTwoParametersFunction_Valid()
    {
        var function = Expressif.Parsers.Function.Parser.Parse("map(add(10,2))");

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
        var function = Expressif.Parsers.Function.Parser.Parse("map(increment | add(10) | add(10,2))");

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
}
