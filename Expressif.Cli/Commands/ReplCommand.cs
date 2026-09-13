using System.CommandLine;
using Expressif.Cli.Application;
using Expressif.Cli.Configuration;
using Expressif.Serialization;
using Expressif.Values;

namespace Expressif.Cli.Commands;

internal static class ReplCommand
{
    public static Command Create(Func<ReplHost> hostFactory, CliConfiguration? configuration = null)
    {
        var command = new Command("repl", "Start an interactive Expressif session.");
        var output = new Option<ValueSerializationFormat?>("--output") { Description = "Output format for all results: raw or json." };
        var raw = new Option<bool>("--raw") { Description = "Shortcut for --output raw." };
        var outputStyle = new Option<ValueFormat?>("--output-style") { Description = "Output style for all results: compact or pretty." };
        outputStyle.Aliases.Add("--style-output");
        var pretty = new Option<bool>("--pretty") { Description = "Shortcut for --output-style pretty." };
        var compact = new Option<bool>("--compact") { Description = "Shortcut for --output-style compact." };
        var indent = new Option<string?>("--indent") { Description = "Pretty-output indentation: a space count from 0 to 8, or tab." };
        command.Options.Add(output);
        command.Options.Add(raw);
        command.Options.Add(outputStyle);
        command.Options.Add(pretty);
        command.Options.Add(compact);
        command.Options.Add(indent);
        command.SetAction(result =>
        {
            if (!ConfiguredOutput.TryResolve(configuration ?? CliConfiguration.CreateDefault(), "repl",
                    result.GetValue(output), result.GetValue(raw), result.GetValue(outputStyle), result.GetValue(pretty), result.GetValue(compact), result.GetValue(indent),
                    out var serializer, out var style, out var indentation, out var error))
            {
                Console.Error.WriteLine(error);
                return ExitCodes.InvalidExpressionOrInput;
            }

            return hostFactory().Run(serializer: serializer, outputStyle: style, indentation: indentation);
        });
        return command;
    }
}
