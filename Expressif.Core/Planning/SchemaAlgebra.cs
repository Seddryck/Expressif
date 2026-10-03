namespace Expressif.Planning;

internal sealed class SchemaAlgebra(ICollection<SchemaAnalysisDiagnostic> diagnostics)
{
    private const string NumericTypeName = "numeric";
    private const string TemporalTypeName = "temporal";

    public LogicalSchema Intersect(LogicalSchema left, LogicalSchema right, string path)
    {
        var special = IntersectSpecial(left, right);
        if (special is not null)
            return special;

        var union = IntersectUnion(left, right, path);
        if (union is not null)
            return union;

        return IntersectSameShape(left, right, path)
            ?? CreateConflict(left, right, path);
    }

    private static LogicalSchema? IntersectSpecial(LogicalSchema left, LogicalSchema right)
        => (left, right) switch
        {
            (NoInputLogicalSchema, AnyLogicalSchema) => left,
            (AnyLogicalSchema, NoInputLogicalSchema) => right,
            (NoInputLogicalSchema, _) => right,
            (_, NoInputLogicalSchema) => left,
            (AnyLogicalSchema, _) => WithCombinedNullability(right, left),
            (_, AnyLogicalSchema) => WithCombinedNullability(left, right),
            (ConflictingLogicalSchema, _) => left,
            (_, ConflictingLogicalSchema) => right,
            _ => null,
        };

    private LogicalSchema? IntersectUnion(LogicalSchema left, LogicalSchema right, string path)
    {
        if (left is UnionLogicalSchema leftUnion)
        {
            var result = IntersectAlternatives(leftUnion, right, path, unionOnLeft: true);
            if (result is not null)
                return result;
        }
        return right is UnionLogicalSchema rightUnion
            ? IntersectAlternatives(rightUnion, left, path, unionOnLeft: false)
            : null;
    }

    private LogicalSchema? IntersectAlternatives(
        UnionLogicalSchema union,
        LogicalSchema other,
        string path,
        bool unionOnLeft)
    {
        var alternatives = union.Alternatives
            .Where(alternative => unionOnLeft
                ? CanIntersectRoot(alternative, other)
                : CanIntersectRoot(other, alternative))
            .Select((alternative, index) => unionOnLeft
                ? Intersect(alternative, other, $"{path}.alternatives[{index}]")
                : Intersect(other, alternative, $"{path}.alternatives[{index}]"))
            .ToArray();
        return alternatives.Length == 0
            ? null
            : WithNullability(
                alternatives.Aggregate(Union),
                IsNullable(union) || IsNullable(other));
    }

    private LogicalSchema? IntersectSameShape(
        LogicalSchema left,
        LogicalSchema right,
        string path)
        => (left, right) switch
        {
            (ScalarLogicalSchema leftScalar, ScalarLogicalSchema rightScalar)
                => IntersectScalars(leftScalar, rightScalar),
            (RecordLogicalSchema leftRecord, RecordLogicalSchema rightRecord)
                => IntersectRecords(leftRecord, rightRecord, path),
            (ArrayLogicalSchema leftArray, ArrayLogicalSchema rightArray)
                => IntersectArrays(leftArray, rightArray, path),
            (TupleLogicalSchema leftTuple, TupleLogicalSchema rightTuple)
                => IntersectTuple(leftTuple, rightTuple, path),
            (PairLogicalSchema leftPair, PairLogicalSchema rightPair)
                => IntersectPairs(leftPair, rightPair, path),
            (DictionaryLogicalSchema leftDictionary, DictionaryLogicalSchema rightDictionary)
                => IntersectDictionaries(leftDictionary, rightDictionary, path),
            (GroupingLogicalSchema leftGrouping, GroupingLogicalSchema rightGrouping)
                => IntersectGroupings(leftGrouping, rightGrouping, path),
            (SortTableLogicalSchema leftSortTable, SortTableLogicalSchema rightSortTable)
                => IntersectSortTables(leftSortTable, rightSortTable, path),
            _ => null,
        };

    private static LogicalSchema? IntersectScalars(
        ScalarLogicalSchema left,
        ScalarLogicalSchema right)
    {
        var type = IntersectScalar(left.Type, right.Type);
        return type is null
            ? null
            : new ScalarLogicalSchema(type, left.IsNullable || right.IsNullable);
    }

    private LogicalSchema IntersectRecords(
        RecordLogicalSchema left,
        RecordLogicalSchema right,
        string path)
    {
        var fields = new SortedDictionary<string, LogicalSchemaField>(StringComparer.Ordinal);
        foreach (var field in left.Fields)
            fields.Add(field.Key, field.Value);
        foreach (var field in right.Fields)
        {
            fields[field.Key] = fields.TryGetValue(field.Key, out var existing)
                ? new LogicalSchemaField(
                    Intersect(existing.Schema, field.Value.Schema, $"{path}.{field.Key}"),
                    existing.Optional || field.Value.Optional)
                : field.Value;
        }
        return new RecordLogicalSchema(
            fields,
            left.AllowsAdditionalFields && right.AllowsAdditionalFields,
            left.IsNullable || right.IsNullable);
    }

    private LogicalSchema IntersectArrays(
        ArrayLogicalSchema left,
        ArrayLogicalSchema right,
        string path)
        => new ArrayLogicalSchema(
            Intersect(left.Items, right.Items, $"{path}.items"),
            left.IsNullable || right.IsNullable);

    private LogicalSchema IntersectPairs(
        PairLogicalSchema left,
        PairLogicalSchema right,
        string path)
        => new PairLogicalSchema(
            Intersect(left.Key, right.Key, $"{path}.key"),
            Intersect(left.Value, right.Value, $"{path}.value"),
            left.IsNullable || right.IsNullable);

    private LogicalSchema IntersectDictionaries(
        DictionaryLogicalSchema left,
        DictionaryLogicalSchema right,
        string path)
        => new DictionaryLogicalSchema(
            Intersect(left.Keys, right.Keys, $"{path}.keys"),
            Intersect(left.Values, right.Values, $"{path}.values"),
            left.IsNullable || right.IsNullable);

    private LogicalSchema IntersectGroupings(
        GroupingLogicalSchema left,
        GroupingLogicalSchema right,
        string path)
        => new GroupingLogicalSchema(
            Intersect(left.Keys, right.Keys, $"{path}.keys"),
            Intersect(left.Items, right.Items, $"{path}.items"),
            left.IsNullable || right.IsNullable);

    private LogicalSchema IntersectSortTables(
        SortTableLogicalSchema left,
        SortTableLogicalSchema right,
        string path)
        => new SortTableLogicalSchema(
            Intersect(left.Items, right.Items, $"{path}.items"),
            left.IsNullable || right.IsNullable);

    private LogicalSchema CreateConflict(LogicalSchema left, LogicalSchema right, string path)
    {
        var conflict = new ConflictingLogicalSchema(left, right, IsNullable(left) || IsNullable(right));
        diagnostics.Add(new SchemaAnalysisDiagnostic(
            "schema.conflict",
            path,
            $"Schema constraints '{Describe(left)}' and '{Describe(right)}' are incompatible."));
        return conflict;
    }

    private static LogicalSchema WithCombinedNullability(LogicalSchema schema, LogicalSchema other)
        => WithNullability(schema, IsNullable(schema) || IsNullable(other));

    public static LogicalSchema Union(LogicalSchema left, LogicalSchema right)
    {
        if (left is AnyLogicalSchema leftAny)
            return leftAny.IsNullable ? WithNullability(right, true) : left;
        if (right is AnyLogicalSchema rightAny)
            return rightAny.IsNullable ? WithNullability(left, true) : right;
        if (left == right)
            return left;
        if (left is ScalarLogicalSchema leftScalar && right is ScalarLogicalSchema rightScalar)
        {
            if (IsNumeric(leftScalar.Type) && IsNumeric(rightScalar.Type))
                return new ScalarLogicalSchema(NumericTypeName, IsNullable(left) || IsNullable(right));
            if (IsTemporal(leftScalar.Type) && IsTemporal(rightScalar.Type))
                return new ScalarLogicalSchema(TemporalTypeName, IsNullable(left) || IsNullable(right));
        }
        var alternatives = FlattenUnion(left)
            .Concat(FlattenUnion(right))
            .Select(schema => WithNullability(schema, false))
            .Distinct()
            .ToArray();
        return alternatives.Length == 1
            ? WithNullability(alternatives[0], IsNullable(left) || IsNullable(right))
            : new UnionLogicalSchema(alternatives, IsNullable(left) || IsNullable(right));
    }

    public static LogicalSchema WithNullability(LogicalSchema schema, bool nullable) => schema switch
    {
        NoInputLogicalSchema => schema,
        AnyLogicalSchema value => value with { IsNullable = nullable },
        ScalarLogicalSchema value => value with { IsNullable = nullable },
        RecordLogicalSchema value => value with { IsNullable = nullable },
        ArrayLogicalSchema value => value with { IsNullable = nullable },
        TupleLogicalSchema value => value with { IsNullable = nullable },
        PairLogicalSchema value => value with { IsNullable = nullable },
        DictionaryLogicalSchema value => value with { IsNullable = nullable },
        GroupingLogicalSchema value => value with { IsNullable = nullable },
        SortTableLogicalSchema value => value with { IsNullable = nullable },
        UnionLogicalSchema value => value with { IsNullable = nullable },
        ConflictingLogicalSchema value => value with { IsNullable = nullable },
        _ => schema,
    };

    public static bool IsNullable(LogicalSchema schema) => schema switch
    {
        NoInputLogicalSchema => false,
        AnyLogicalSchema value => value.IsNullable,
        ScalarLogicalSchema value => value.IsNullable,
        RecordLogicalSchema value => value.IsNullable,
        ArrayLogicalSchema value => value.IsNullable,
        TupleLogicalSchema value => value.IsNullable,
        PairLogicalSchema value => value.IsNullable,
        DictionaryLogicalSchema value => value.IsNullable,
        GroupingLogicalSchema value => value.IsNullable,
        SortTableLogicalSchema value => value.IsNullable,
        UnionLogicalSchema value => value.IsNullable,
        ConflictingLogicalSchema value => value.IsNullable,
        _ => false,
    };

    public static bool ContainsAny(LogicalSchema schema) => schema switch
    {
        AnyLogicalSchema => true,
        RecordLogicalSchema record => record.Fields.Values.Any(field => ContainsAny(field.Schema)),
        ArrayLogicalSchema array => ContainsAny(array.Items),
        TupleLogicalSchema tuple => tuple.Items.Any(ContainsAny)
            || (tuple.AdditionalItems is not null && ContainsAny(tuple.AdditionalItems)),
        PairLogicalSchema pair => ContainsAny(pair.Key) || ContainsAny(pair.Value),
        DictionaryLogicalSchema dictionary => ContainsAny(dictionary.Keys) || ContainsAny(dictionary.Values),
        GroupingLogicalSchema grouping => ContainsAny(grouping.Keys) || ContainsAny(grouping.Items),
        SortTableLogicalSchema sortTable => ContainsAny(sortTable.Items),
        UnionLogicalSchema union => union.Alternatives.Any(ContainsAny),
        ConflictingLogicalSchema conflict => ContainsAny(conflict.Left) || ContainsAny(conflict.Right),
        _ => false,
    };

    public static bool ContainsConflict(LogicalSchema schema) => schema switch
    {
        ConflictingLogicalSchema => true,
        RecordLogicalSchema record => record.Fields.Values.Any(field => ContainsConflict(field.Schema)),
        ArrayLogicalSchema array => ContainsConflict(array.Items),
        TupleLogicalSchema tuple => tuple.Items.Any(ContainsConflict)
            || (tuple.AdditionalItems is not null && ContainsConflict(tuple.AdditionalItems)),
        PairLogicalSchema pair => ContainsConflict(pair.Key) || ContainsConflict(pair.Value),
        DictionaryLogicalSchema dictionary => ContainsConflict(dictionary.Keys) || ContainsConflict(dictionary.Values),
        GroupingLogicalSchema grouping => ContainsConflict(grouping.Keys) || ContainsConflict(grouping.Items),
        SortTableLogicalSchema sortTable => ContainsConflict(sortTable.Items),
        UnionLogicalSchema union => union.Alternatives.Any(ContainsConflict),
        _ => false,
    };

    public static string Normalize(string type)
        => type.Trim().ToLowerInvariant() switch
        {
            "date-time" => "datetime",
            var value => value,
        };

    public static string Describe(LogicalSchema schema) => schema switch
    {
        NoInputLogicalSchema => "no-input",
        AnyLogicalSchema => "any",
        ScalarLogicalSchema scalar => scalar.Type,
        RecordLogicalSchema => "record",
        ArrayLogicalSchema => "array",
        TupleLogicalSchema => "tuple",
        PairLogicalSchema => "pair",
        DictionaryLogicalSchema => "dictionary",
        GroupingLogicalSchema => "grouping",
        SortTableLogicalSchema => "sort-table",
        UnionLogicalSchema => "union",
        ConflictingLogicalSchema => "conflict",
        _ => schema.GetType().Name,
    };

    private static bool CanIntersectRoot(LogicalSchema left, LogicalSchema right) => (left, right) switch
    {
        (AnyLogicalSchema or NoInputLogicalSchema, _) => true,
        (_, AnyLogicalSchema or NoInputLogicalSchema) => true,
        (UnionLogicalSchema union, _) => union.Alternatives.Any(alternative => CanIntersectRoot(alternative, right)),
        (_, UnionLogicalSchema union) => union.Alternatives.Any(alternative => CanIntersectRoot(left, alternative)),
        (ScalarLogicalSchema leftScalar, ScalarLogicalSchema rightScalar)
            => IntersectScalar(leftScalar.Type, rightScalar.Type) is not null,
        (ArrayLogicalSchema, ArrayLogicalSchema) => true,
        (RecordLogicalSchema, RecordLogicalSchema) => true,
        (TupleLogicalSchema, TupleLogicalSchema) => true,
        (PairLogicalSchema, PairLogicalSchema) => true,
        (DictionaryLogicalSchema, DictionaryLogicalSchema) => true,
        (GroupingLogicalSchema, GroupingLogicalSchema) => true,
        (SortTableLogicalSchema, SortTableLogicalSchema) => true,
        _ => false,
    };

    private TupleLogicalSchema? IntersectTuple(TupleLogicalSchema left, TupleLogicalSchema right, string path)
    {
        if ((left.Items.Count > right.Items.Count && right.AdditionalItems is null)
            || (right.Items.Count > left.Items.Count && left.AdditionalItems is null))
        {
            return null;
        }
        var count = Math.Max(left.Items.Count, right.Items.Count);
        var items = new LogicalSchema[count];
        for (var index = 0; index < count; index++)
        {
            var leftItem = index < left.Items.Count ? left.Items[index] : left.AdditionalItems!;
            var rightItem = index < right.Items.Count ? right.Items[index] : right.AdditionalItems!;
            items[index] = Intersect(leftItem, rightItem, $"{path}.items[{index}]");
        }
        var additionalItems = left.AdditionalItems is not null && right.AdditionalItems is not null
            ? Intersect(left.AdditionalItems, right.AdditionalItems, $"{path}.additionalItems")
            : null;
        return new TupleLogicalSchema(items, IsNullable(left) || IsNullable(right), additionalItems);
    }

    private static IEnumerable<LogicalSchema> FlattenUnion(LogicalSchema schema)
        => schema is UnionLogicalSchema union ? union.Alternatives : [schema];

    public static string? IntersectScalar(string left, string right)
    {
        left = Normalize(left);
        right = Normalize(right);
        if (left == right)
            return left;
        if (left == "scalar")
            return right;
        if (right == "scalar")
            return left;
        if (left == NumericTypeName && IsNumeric(right))
            return right;
        if (right == NumericTypeName && IsNumeric(left))
            return left;
        if (left == TemporalTypeName && IsTemporal(right))
            return right;
        if (right == TemporalTypeName && IsTemporal(left))
            return left;
        return null;
    }

    private static bool IsNumeric(string type) => Normalize(type) is "integer" or "decimal" or NumericTypeName;

    private static bool IsTemporal(string type) => Normalize(type) is "date" or "datetime" or "time" or TemporalTypeName;
}
