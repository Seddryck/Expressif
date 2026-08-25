using Expressif.Bindings;
using Expressif.Cli.Expressions;
using Expressif.Functions.Catalog;
using Expressif.Syntax;

namespace Expressif.Cli.Application;

internal enum TreeOutputFormat
{
    Tree,
    Json,
    Yaml,
}

internal static class TreeOutputFormatParser
{
    public static bool TryParse(string value, out TreeOutputFormat format)
        => Enum.TryParse(value, ignoreCase: true, out format);

    public static string ToToken(TreeOutputFormat format) => format.ToString().ToLowerInvariant();
}

internal sealed record ParseRequest(string Expression, TreeOutputFormat Output);

internal sealed class ParseHandler(ISyntaxService syntax)
{
    public RootExpressionSyntax Execute(ParseRequest request) => syntax.Parse(request.Expression);
}

internal sealed record BindRequest(string Expression, TreeOutputFormat Output);

internal sealed class BindHandler(ISyntaxService syntax)
{
    public IRootExpression Execute(BindRequest request)
    {
        var bound = syntax.Bind(syntax.Parse(request.Expression));
        syntax.Validate(bound, new Context());
        return bound;
    }
}

internal enum HelpMode
{
    Function,
    List,
    Scope,
}

internal sealed record HelpRequest(HelpMode Mode, string? Value)
{
    public static bool TryCreate(string? function, bool list, string? scope, out HelpRequest? request)
    {
        var selectedModes = (function is null ? 0 : 1) + (list ? 1 : 0) + (scope is null ? 0 : 1);
        request = selectedModes == 1
            ? function is not null
                ? new HelpRequest(HelpMode.Function, function)
                : scope is not null
                    ? new HelpRequest(HelpMode.Scope, scope)
                    : new HelpRequest(HelpMode.List, null)
            : null;
        return request is not null;
    }
}

internal abstract record HelpResult;

internal sealed record FunctionHelpResult(FunctionDocumentation Function) : HelpResult;

internal sealed record FunctionListHelpResult(IReadOnlyList<FunctionDocumentation> Functions) : HelpResult;

internal sealed record UnknownFunctionHelpResult(string Name, IReadOnlyList<string> Suggestions) : HelpResult;

internal sealed record UnknownScopeHelpResult(string Scope) : HelpResult;

internal sealed class HelpHandler(IFunctionCatalogService catalog)
{
    public HelpResult Execute(HelpRequest request)
        => request.Mode switch
        {
            HelpMode.List => new FunctionListHelpResult(catalog.Functions),
            HelpMode.Scope => Scope(request.Value!),
            _ => Function(request.Value!),
        };

    private HelpResult Scope(string scope)
    {
        var functions = catalog.ForScope(scope).ToArray();
        return functions.Length == 0
            ? new UnknownScopeHelpResult(scope)
            : new FunctionListHelpResult(functions);
    }

    private HelpResult Function(string name)
    {
        var match = catalog.Find(name);
        return match is not null
            ? new FunctionHelpResult(match)
            : new UnknownFunctionHelpResult(name, catalog.Suggest(name).Select(x => x.Name).ToArray());
    }
}
