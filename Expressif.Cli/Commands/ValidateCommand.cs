using System.CommandLine;
using Expressif.Cli.Application;

namespace Expressif.Cli.Commands;

internal static class ValidateCommand
{
    public static Command Create(CliServices services)
    {
        var expressionArgument = new Argument<string?>("expression")
        {
            Arity = ArgumentArity.ZeroOrOne,
            Description = "Expression to validate."
        };

        var expressionFileOption = new Option<string?>("--file")
        {
            Description = "Path to a UTF-8 file containing the expression to validate."
        };
        expressionFileOption.Aliases.Add("-f");

        var openOption = new Option<bool>("--open")
        {
            Description = "Validate the expression as an open expression (default behavior)."
        };

        var closedOption = new Option<bool>("--closed")
        {
            Description = "Validate the expression as a closed expression."
        };

        var command = new Command("validate", "Validate an Expressif expression.");

        command.Arguments.Add(expressionArgument);
        command.Options.Add(expressionFileOption);
        command.Options.Add(openOption);
        command.Options.Add(closedOption);

        command.SetAction(parseResult =>
        {
            var inlineExpression = parseResult.GetValue(expressionArgument);
            var expressionFilePath = parseResult.GetValue(expressionFileOption);
            var asOpenExpression = parseResult.GetValue(openOption);
            var asClosedExpression = parseResult.GetValue(closedOption);

            if (asOpenExpression && asClosedExpression)
            {
                Console.Error.WriteLine("Options --open and --closed cannot be used together.");
                return ExitCodes.InvalidExpressionOrInput;
            }

            var useClosedValidation = asClosedExpression;

            if (!ExpressionCommandCommon.TryResolveExpressionCode(
                    inlineExpression,
                    expressionFilePath,
                    services.TextFiles,
                    out var expressionCode,
                    out var hasExpressionFile))
            {
                return ExitCodes.InvalidExpressionOrInput;
            }

            try
            {
                if (useClosedValidation)
                {
                    _ = services.Expressions.CompileClosed(expressionCode, new Context());
                }
                else
                {
                    _ = services.Expressions.CompileOpen(expressionCode, new Context());
                }

                Console.Out.WriteLine("Expression is valid.");
                return ExitCodes.Success;
            }
            catch (ExpressionRequiresInputException exception)
            {
                Console.Error.WriteLine(exception.Message);
                return ExitCodes.InvalidExpressionOrInput;
            }
            catch (Exception exception) when (exception is Expressif.Syntax.ExpressifSyntaxException
                                              or Expressif.Bindings.BindingException
                                              or NotImplementedFunctionException
                                              or MissingOrUnexpectedParametersFunctionException)
            {
                return ExpressionCommandCommon.WriteValidationError(exception, expressionCode, hasExpressionFile, expressionFilePath);
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine($"Unexpected error: {exception.Message}");
                return ExitCodes.UnexpectedInternalError;
            }
        });

        return command;
    }
}
