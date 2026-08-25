using System.CommandLine;
using Expressif.Bindings;
using Expressif.Functions;
using Expressif.Syntax;

namespace Expressif.Cli.Commands;

internal static class BindCommand
{
    internal static Func<string, RootExpressionSyntax> ParseExpression { get; set; }
        = ExpressionParser.Parse;

    internal static Func<RootExpressionSyntax, IRootExpression> BindExpression { get; set; }
        = static syntax => new ExpressifBinder().Bind(syntax);

    internal static Action<IRootExpression> ValidateExpression { get; set; }
        = static expression => _ = new FunctionFactory().Instantiate(expression, new Context());

    public static Command Create()
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
        command.SetAction(parseResult =>
        {
            var expression = parseResult.GetValue(expressionArgument)!;
            var output = parseResult.GetValue(outputOption)!;

            if (!TreeDocumentFormatter.IsSupported(output))
            {
                Console.Error.WriteLine("Option --output must be one of: tree, json, yaml.");
                return ExitCodes.InvalidExpressionOrInput;
            }

            try
            {
                var bound = BindExpression(ParseExpression(expression));
                ValidateExpression(bound);
                Console.Out.WriteLine(BoundTreeFormatter.Format(bound, output));
                return ExitCodes.Success;
            }
            catch (Exception exception) when (exception is ExpressifSyntaxException
                                              or BindingException
                                              or NotImplementedFunctionException
                                              or ArgumentException)
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

    internal static void ResetDelegates()
    {
        ParseExpression = ExpressionParser.Parse;
        BindExpression = static syntax => new ExpressifBinder().Bind(syntax);
        ValidateExpression = static expression => _ = new FunctionFactory().Instantiate(expression, new Context());
    }
}
