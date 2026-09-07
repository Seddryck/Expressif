using Expressif.Bindings;
using Expressif.Cli.Commands;
using Expressif.Syntax;

namespace Expressif.Cli.Tests;

public class InputBindingInspectionTests
{
    [Test]
    public void NamedBinding_PreservesAuthoredSyntaxAndBoundBody()
    {
        const string source = "apply(input:> @input | add(1))";
        var syntax = ExpressionParser.Parse(source);
        var syntaxOutput = SyntaxTreeFormatter.Format(syntax, "json");
        var boundOutput = BoundTreeFormatter.Format(new ExpressifBinder().Bind(syntax), "json");
        Assert.Multiple(() =>
        {
            Assert.That(syntaxOutput, Does.Contain("InputBoundExpression"));
            Assert.That(syntaxOutput, Does.Contain("BindingName"));
            Assert.That(boundOutput, Does.Contain("InputBinding"));
            Assert.That(boundOutput, Does.Contain("input"));
            Assert.That(boundOutput, Does.Contain("add"));
        });
    }
}
