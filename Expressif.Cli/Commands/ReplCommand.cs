using System.CommandLine;
using Expressif.Cli.Application;
using Expressif.Values;

namespace Expressif.Cli.Commands;

internal static class ReplCommand
{
    public static Command Create(Func<ReplHost> hostFactory)
    {
        var command = new Command("repl", "Start an interactive Expressif session.");
        var outputStyle = new Option<ValueFormat?>("--output-style") { Description = "Output style for all results: compact or pretty." };
        outputStyle.Aliases.Add("--style-output");
        var pretty = new Option<bool>("--pretty") { Description = "Shortcut for --output-style pretty." };
        var compact = new Option<bool>("--compact") { Description = "Shortcut for --output-style compact." };
        var indent = new Option<string?>("--indent") { Description = "Pretty-output indentation: a space count from 0 to 8, or tab." };
        command.Options.Add(outputStyle);
        command.Options.Add(pretty);
        command.Options.Add(compact);
        command.Options.Add(indent);
        command.SetAction(result =>
        {
            if (!OutputStyleSelection.TryResolve(
                    result.GetValue(outputStyle), result.GetValue(pretty), result.GetValue(compact), out var style, out var styleError))
            {
                Console.Error.WriteLine(styleError);
                return ExitCodes.InvalidExpressionOrInput;
            }

            if (!OutputIndentation.TryResolve(result.GetValue(indent), style, out var indentation, out var indentationError))
            {
                Console.Error.WriteLine(indentationError);
                return ExitCodes.InvalidExpressionOrInput;
            }

            return hostFactory().Run(outputStyle: style, indentation: indentation);
        });
        return command;
    }
}
