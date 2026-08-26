using System.CommandLine;
using Expressif.Cli.Application;

namespace Expressif.Cli.Commands;

internal static class RunCommand
{
    public static Command Create(RunHandler handler)
    {
        var expression = new Argument<string?>("expression") { Arity = ArgumentArity.ZeroOrOne, Description = "Expression to evaluate." };
        var input = new Option<string[]>("--input") { Description = "Input row passed to the expression. Repeat --input to add rows." };
        input.Aliases.Add("-i");
        var batch = new Option<string[]>("--batch") { Description = "Enumerable batch input. Each direct element is evaluated as one row." };
        var file = new Option<string?>("--file") { Description = "Path to a UTF-8 file containing the expression to evaluate." };
        file.Aliases.Add("-f");
        var source = new Option<string?>("--source") { Description = "Path to a source file returning rows as IEnumerable or IDataReader." };
        source.Aliases.Add("-s");
        var sourceOptions = new Option<string[]>("--source-option") { Description = "Source-specific setting in <name>=<value> form. Repeat to add settings." };
        var scalar = new Option<bool>("--scalar") { Description = "Treat each source row as a single value. The source must contain exactly one column." };
        var command = new Command("run", "Evaluate an Expressif expression for each element of an input sequence.");
        command.Arguments.Add(expression);
        command.Options.Add(input);
        command.Options.Add(batch);
        command.Options.Add(source);
        command.Options.Add(scalar);
        command.Options.Add(sourceOptions);
        command.Options.Add(file);
        command.SetAction(result => handler.Execute(new RunRequest(
            result.GetValue(expression), result.GetValue(file), result.GetValue(input) ?? [], result.GetValue(batch)?.FirstOrDefault(),
            result.GetValue(source), result.GetValue(sourceOptions) ?? [], result.GetValue(scalar),
            result.GetResult(input) is not null, result.GetResult(batch) is not null, result.GetResult(source) is not null,
            result.GetResult(sourceOptions) is not null, result.GetResult(batch)?.IdentifierTokenCount ?? 0)));
        return command;
    }
}
