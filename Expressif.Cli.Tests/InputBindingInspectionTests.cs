using Expressif.Bindings;
using Expressif.Cli.Commands;
using Expressif.Syntax;

namespace Expressif.Cli.Tests;

public class InputBindingInspectionTests
{
    [TestCase("apply(@_ | input:> @input | add(1))", "BindingName")]
    [TestCase("apply(@_ | :> $0 | add($1))", "TupleProjection")]
    [TestCase("apply(@_ | (input, other) :> @input | add(@other))", "PositionalBindingPattern")]
    public void Binding_PreservesCanonicalSyntaxAndBoundBody(string source, string bindingKind)
    {
        var syntax = ExpressionParser.Parse(source);
        var syntaxOutput = SyntaxTreeFormatter.Format(syntax, "json");
        var boundOutput = BoundTreeFormatter.Format(new ExpressifBinder().Bind(syntax), "json");
        Assert.Multiple(() =>
        {
            Assert.That(syntaxOutput, Does.Contain("InputBindingExpression"));
            Assert.That(syntaxOutput, Does.Contain(bindingKind));
            Assert.That(boundOutput, Does.Contain("InputBinding"));
            Assert.That(boundOutput, Does.Contain("Names"));
            Assert.That(boundOutput, Does.Contain("add"));
        });
    }
}
