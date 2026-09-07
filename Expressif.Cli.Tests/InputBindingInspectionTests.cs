using Expressif.Bindings;
using Expressif.Cli.Commands;
using Expressif.Syntax;

namespace Expressif.Cli.Tests;

public class InputBindingInspectionTests
{
    [TestCase("apply(input:> @input | add(1))", "BindingName")]
    [TestCase("apply(:> $0 | add($1))", "TupleProjection")]
    [TestCase("apply((input, other) :> @input | add(@other))", "PositionalBindingPattern")]
    public void Binding_PreservesAuthoredSyntaxAndBoundBody(string source, string bindingKind)
    {
        var syntax = ExpressionParser.Parse(source);
        var syntaxOutput = SyntaxTreeFormatter.Format(syntax, "json");
        var boundOutput = BoundTreeFormatter.Format(new ExpressifBinder().Bind(syntax), "json");
        Assert.Multiple(() =>
        {
            Assert.That(syntaxOutput, Does.Contain("InputBoundExpression"));
            Assert.That(syntaxOutput, Does.Contain(bindingKind));
            Assert.That(boundOutput, Does.Contain("InputBinding"));
            Assert.That(boundOutput, Does.Contain("Names"));
            Assert.That(boundOutput, Does.Contain("add"));
        });
    }
}
