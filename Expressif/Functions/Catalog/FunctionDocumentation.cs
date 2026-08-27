namespace Expressif.Functions.Catalog;

public sealed record FunctionDocumentation(
    string Name,
    bool IsPublic,
    string[] Aliases,
    string Scope,
    string Input,
    string Output,
    string Summary,
    FunctionParameterDocumentation[] Parameters,
    string[]? Examples = null,
    string? Behavior = null);

public sealed record FunctionParameterDocumentation(
    string Name,
    string? Type,
    bool Optional,
    string Summary,
    bool Variadic = false,
    int MinimumCardinality = 1,
    string? Kind = null)
{
    public string TypeOrKind => Type ?? Kind ?? "any";
}
