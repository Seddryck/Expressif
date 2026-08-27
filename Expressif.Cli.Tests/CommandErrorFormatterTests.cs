using Expressif.Bindings;
using Expressif.Cli.Commands;
using Expressif.Syntax;

namespace Expressif.Cli.Tests;

[NonParallelizable]
public class CommandErrorFormatterTests
{
    [Test]
    public void FormatValidationError_BindingFailure_UsesBindingDiagnostic()
        => Assert.That(
            CommandErrorFormatter.FormatValidationError(new BindingException("invalid binding")),
            Is.EqualTo($"Binding error [EXPR2001]:{Environment.NewLine}invalid binding"));

    [Test]
    public void FormatValidationError_UnnamedMissingFunction_PreservesOriginalMessage()
    {
        var exception = new NotImplementedFunctionException(string.Empty);

        Assert.That(CommandErrorFormatter.FormatValidationError(exception), Is.EqualTo(exception.Message));
    }

    [Test]
    public void WriteValidationError_WritesDiagnosticAndReturnsInvalidInput()
    {
        var originalError = Console.Error;
        using var stderr = new StringWriter();
        Console.SetError(stderr);
        try
        {
            Assert.That(
                CommandErrorFormatter.WriteValidationError(new BindingException("invalid binding")),
                Is.EqualTo(ExitCodes.InvalidExpressionOrInput));
            Assert.That(stderr.ToString(), Does.Contain("invalid binding"));
        }
        finally
        {
            Console.SetError(originalError);
        }
    }

    [Test]
    public void FormatValidationError_SyntaxFailureWithoutSource_UsesAggregateMessage()
    {
        var exception = Assert.Throws<ExpressifSyntaxException>(() => Expression.Create("add("));

        Assert.That(
            CommandErrorFormatter.FormatValidationError(exception),
            Does.StartWith("Syntax error [EXPR1001]:"));
    }

    [Test]
    public void FormatValidationError_SyntaxFailureOnSecondLine_LocatesLineAndColumn()
    {
        const string source = "trim |\nadd(";
        var exception = Assert.Throws<ExpressifSyntaxException>(() => Expression.Create(source));

        Assert.That(
            CommandErrorFormatter.FormatValidationError(exception, source),
            Does.Contain("at line 2"));
    }
}
