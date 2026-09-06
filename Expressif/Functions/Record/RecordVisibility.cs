using Expressif.Values;

namespace Expressif.Functions.Record;

internal static class RecordVisibility
{
    internal static RecordValue Rename(object? value, HashSet<string>? names, bool makePublic)
    {
        var fields = RecordOperations.Enumerate(value);
        var existing = new HashSet<string>(fields.Select(field => field.Key), StringComparer.Ordinal);
        var result = new RecordValue();
        foreach (var field in fields)
        {
            var name = field.Key;
            var isPrivate = name.StartsWith('_');
            var publicName = isPrivate ? name[1..] : name;
            if (isPrivate == makePublic && (names is null || names.Contains(publicName)))
            {
                name = makePublic ? publicName : "_" + name;
                if (existing.Contains(name))
                    throw new InvalidOperationException($"Cannot change field visibility: destination field '{name}' already exists.");
            }

            result.Set(name, field.Value);
        }

        return result;
    }
}
