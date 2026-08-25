using System.CommandLine;
using Expressif.Cli.Application;
using Expressif.Bindings;
using Expressif.Functions;
using Expressif.Syntax;

namespace Expressif.Cli.Commands;

internal static class BindCommand
{
    public static Command Create(CliServices services)
    {
        var expressionArgument = new Argument<string>("expression")
        {
            Description = "Expression to bind."
        };

        var outputOption = new Option<string>("--output")
        {
            Description = "Output representation: tree, json, or yaml.",
            DefaultValueFactory = static _ => "tree"
        };

        var command = new Command("bind", "Bind an Expressif expression and display its bound expression tree.");
        command.Arguments.Add(expressionArgument);
        command.Options.Add(outputOption);
        var handler = new BindHandler(services.Syntax);
        command.SetAction(parseResult =>
        {
            var expression = parseResult.GetValue(expressionArgument)!;
            var output = parseResult.GetValue(outputOption)!;

            if (!TreeOutputFormatParser.TryParse(output, out var outputFormat))
            {
                Console.Error.WriteLine("Option --output must be one of: tree, json, yaml.");
                return ExitCodes.InvalidExpressionOrInput;
            }

            try
            {
                var request = new BindRequest(expression, outputFormat);
                var bound = handler.Execute(request);
                Console.Out.WriteLine(BoundTreeFormatter.Format(bound, TreeOutputFormatParser.ToToken(request.Output)));
                return ExitCodes.Success;
            }
            catch (Exception exception) when (exception is ExpressifSyntaxException
                                              or BindingException
                                              or NotImplementedFunctionException)
            {
                return ExpressionCommandCommon.WriteValidationError(
                    exception,
                    expression,
                    hasExpressionFile: false,
                    expressionFilePath: null);
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
