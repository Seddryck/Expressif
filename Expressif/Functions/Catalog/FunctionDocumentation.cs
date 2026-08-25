namespace Expressif.Functions.Catalog;

public sealed record FunctionDocumentation(
    string Name,
    bool IsPublic,
    string[] Aliases,
    string Scope,
    string Input,
    string Output,
    string Summary,
    FunctionParameterDocumentation[] Parameters);

public sealed record FunctionParameterDocumentation(
    string Name,
    string Type,
    bool Optional,
    string Summary);
