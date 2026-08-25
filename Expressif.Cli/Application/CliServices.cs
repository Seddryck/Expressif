using Expressif.Cli.Expressions;
using Expressif.Cli.Infrastructure;
using Expressif.Cli.Inputs;

namespace Expressif.Cli.Application;

internal sealed record CliServices(
    IExpressionService Expressions,
    ISyntaxService Syntax,
    IInputValueParser Values,
    IStrictUtf8TextReader TextFiles,
    IFunctionCatalogService FunctionCatalog,
    Func<string, object?>? SourceResolver = null,
    IReadOnlyList<IFileSourceProvider>? SourceProviders = null)
{
    public static CliServices CreateDefault()
        => new(
            new ExpressionService(),
            new SyntaxService(),
            new CliInputValueParser(),
            new StrictUtf8TextReader(),
            new FunctionCatalogService(Expressif.Functions.Catalog.FunctionCatalog.Default));
}
