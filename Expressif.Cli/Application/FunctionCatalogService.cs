using Expressif.Functions.Catalog;

namespace Expressif.Cli.Application;

internal interface IFunctionCatalogService
{
    IReadOnlyList<FunctionDocumentation> Functions { get; }

    FunctionDocumentation? Find(string name);

    IEnumerable<FunctionDocumentation> ForScope(string scope);

    IEnumerable<FunctionDocumentation> Suggest(string name);
}

internal sealed class FunctionCatalogService(FunctionCatalog catalog) : IFunctionCatalogService
{
    public IReadOnlyList<FunctionDocumentation> Functions => catalog.Functions;

    public FunctionDocumentation? Find(string name) => catalog.Find(name);

    public IEnumerable<FunctionDocumentation> ForScope(string scope) => catalog.ForScope(scope);

    public IEnumerable<FunctionDocumentation> Suggest(string name) => catalog.Suggest(name);
}
