namespace Expressif.Planning;

internal sealed class SchemaBindingSet
{
    private readonly SchemaAlgebra algebra;
    private readonly IDictionary<string, LogicalSchema> values;

    public SchemaBindingSet(SchemaAlgebra algebra, IDictionary<string, LogicalSchema> values)
    {
        this.algebra = algebra;
        this.values = values;
    }

    public IEnumerable<KeyValuePair<string, LogicalSchema>> Entries => values;

    public IEnumerable<string> Names => values.Keys;

    public static SchemaBindingSet Empty(SchemaAlgebra algebra)
        => new(algebra, new Dictionary<string, LogicalSchema>(StringComparer.Ordinal));

    public bool TryGetValue(string name, out LogicalSchema value)
    {
        if (values.TryGetValue(name, out var binding))
        {
            value = binding;
            return true;
        }
        value = new AnyLogicalSchema();
        return false;
    }

    public void Constrain(string name, LogicalSchema value, string path)
        => values[name] = values.TryGetValue(name, out var existing)
            ? algebra.Intersect(existing, value, path)
            : value;

    public void MergeChoice(SchemaBindingSet choice)
    {
        foreach (var binding in choice.Entries)
        {
            values[binding.Key] = values.TryGetValue(binding.Key, out var existing)
                ? SchemaAlgebra.Union(existing, binding.Value)
                : binding.Value;
        }
    }

    public void MergeAlternatives(IReadOnlyList<SchemaBindingSet> alternatives, string path)
    {
        foreach (var name in alternatives.SelectMany(item => item.Names).Distinct(StringComparer.Ordinal))
        {
            var value = alternatives
                .Select(item => item.TryGetValue(name, out var binding) ? binding : null)
                .OfType<LogicalSchema>()
                .Aggregate(SchemaAlgebra.Union);
            Constrain(name, value, path);
        }
    }
}
