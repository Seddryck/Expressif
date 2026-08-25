using System.CommandLine;
using Expressif.Cli.Application;
using Expressif.Syntax;

namespace Expressif.Cli.Commands;

internal static class ParseCommand
{
    public static Command Create(CliServices services)
    {
        var expressionArgument = new Argument<string>("expression")
        {
            Description = "Expression to parse."
        };

        var outputOption = new Option<string>("--output")
        {
            Description = "Output representation: tree, json, or yaml.",
            DefaultValueFactory = static _ => "tree"
        };

        var command = new Command("parse", "Parse an Expressif expression and display its syntax tree.");
        command.Arguments.Add(expressionArgument);
        command.Options.Add(outputOption);
        var handler = new ParseHandler(services.Syntax);
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
                var request = new ParseRequest(expression, outputFormat);
                var syntax = handler.Execute(request);
                Console.Out.WriteLine(SyntaxTreeFormatter.Format(syntax, TreeOutputFormatParser.ToToken(request.Output)));
                return ExitCodes.Success;
            }
            catch (ExpressifSyntaxException exception)
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
