namespace Expressif.Planning;

internal sealed record SchemaExpression(string Name, IReadOnlyList<SchemaExpression> Arguments)
{
    public bool IsVariable => Arguments.Count == 0 && Name.Length > 0 && char.IsUpper(Name[0]);

    public bool ContainsVariable => IsVariable || Arguments.Any(argument => argument.ContainsVariable);
}
