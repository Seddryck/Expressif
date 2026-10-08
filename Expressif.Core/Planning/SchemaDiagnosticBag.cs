using System.Collections;

namespace Expressif.Planning;

internal sealed class SchemaDiagnosticBag : ICollection<SchemaAnalysisDiagnostic>, IReadOnlyList<SchemaAnalysisDiagnostic>
{
    private readonly List<SchemaAnalysisDiagnostic> items = [];

    public int Count => items.Count;
    public bool IsReadOnly => false;
    public SchemaAnalysisDiagnostic this[int index] => items[index];

    public void Add(SchemaAnalysisDiagnostic item) => items.Add(item);
    public void Clear() => items.Clear();
    public bool Contains(SchemaAnalysisDiagnostic item) => items.Contains(item);
    public void CopyTo(SchemaAnalysisDiagnostic[] array, int arrayIndex) => items.CopyTo(array, arrayIndex);
    public bool Remove(SchemaAnalysisDiagnostic item) => items.Remove(item);
    public IEnumerator<SchemaAnalysisDiagnostic> GetEnumerator() => items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
