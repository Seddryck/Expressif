using Expressif.Cli.Expressions;
using Expressif.Cli.Infrastructure;
using Expressif.Cli.Inputs;
using Expressif.Functions.Catalog;

namespace Expressif.Cli.Application;

internal sealed record CliComposition(
    ParseHandler Parse,
    BindHandler Bind,
    EvaluateHandler Evaluate,
    RunHandler Run,
    ValidateHandler Validate,
    HelpHandler Help,
    Func<ReplHost> Repl,
    IStrictUtf8TextReader TextFiles)
{
    public static CliComposition CreateDefault()
    {
        var expressions = new ExpressionService();
        var syntax = new SyntaxService();
        var values = new CliInputValueParser();
        var textFiles = new StrictUtf8TextReader();
        var sourceInfrastructure = new SourceInfrastructure(expressions, values, textFiles);
        IFileSourceProvider[] providers =
        [
            new CsvFileSourceProvider(sourceInfrastructure),
            new JsonFileSourceProvider(sourceInfrastructure),
            new ExpressionFileSourceProvider(sourceInfrastructure),
        ];
        var sources = new SourcePipeline(providers, sourceInfrastructure);

        return new CliComposition(
            new ParseHandler(syntax),
            new BindHandler(syntax),
            new EvaluateHandler(expressions, values, sources),
            new RunHandler(expressions, values, textFiles, sources),
            new ValidateHandler(expressions),
            new HelpHandler(new FunctionCatalogService(FunctionCatalog.Default)),
            () => new ReplHost(new ReplSession(expressions), new ConsoleReplTerminal()),
            textFiles);
    }
}
