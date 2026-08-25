using Expressif.Cli.Expressions;
using Expressif.Cli.Infrastructure;
using Expressif.Cli.Inputs;

namespace Expressif.Cli.Application;

internal sealed record CliServices(
    IExpressionService Expressions,
    IInputValueParser Values,
    IStrictUtf8TextReader TextFiles,
    Func<string, object?>? SourceResolver = null,
    IReadOnlyList<IFileSourceProvider>? SourceProviders = null)
{
    public static CliServices CreateDefault()
        => new(new ExpressionService(), new CliInputValueParser(), new StrictUtf8TextReader());
}
