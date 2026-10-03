namespace Expressif.Planning;

internal sealed class SchemaAlgebra(ICollection<SchemaAnalysisDiagnostic> diagnostics)
{
    private const string NumericTypeName = "numeric";
    private const string TemporalTypeName = "temporal";

    public LogicalSchema Intersect(LogicalSchema left, LogicalSchema right, string path)
    {
        if (left is NoInputLogicalSchema && right is AnyLogicalSchema)
            return left;
        if (right is NoInputLogicalSchema && left is AnyLogicalSchema)
            return right;
        if (left is NoInputLogicalSchema)
            return right;
        if (right is NoInputLogicalSchema)
            return left;
        if (left is AnyLogicalSchema)
            return WithNullability(right, IsNullable(left) || IsNullable(right));
        if (right is AnyLogicalSchema)
            return WithNullability(left, IsNullable(left) || IsNullable(right));
        if (left is ConflictingLogicalSchema)
            return left;
        if (right is ConflictingLogicalSchema)
            return right;
        if (left is UnionLogicalSchema leftUnion)
        {
            var alternatives = leftUnion.Alternatives
                .Where(alternative => CanIntersectRoot(alternative, right))
                .Select((alternative, index) => Intersect(alternative, right, $"{path}.alternatives[{index}]"))
                .ToArray();
            if (alternatives.Length > 0)
            {
                return WithNullability(
                    alternatives.Aggregate(Union),
                    IsNullable(left) || IsNullable(right));
            }
        }
        if (right is UnionLogicalSchema rightUnion)
        {
            var alternatives = rightUnion.Alternatives
                .Where(alternative => CanIntersectRoot(left, alternative))
                .Select((alternative, index) => Intersect(left, alternative, $"{path}.alternatives[{index}]"))
                .ToArray();
            if (alternatives.Length > 0)
            {
                return WithNullability(
                    alternatives.Aggregate(Union),
                    IsNullable(left) || IsNullable(right));
            }
        }
        if (left is ScalarLogicalSchema leftScalar && right is ScalarLogicalSchema rightScalar)
        {
            var type = IntersectScalar(leftScalar.Type, rightScalar.Type);
            if (type is not null)
                return new ScalarLogicalSchema(type, IsNullable(left) || IsNullable(right));
        }
        if (left is RecordLogicalSchema leftRecord && right is RecordLogicalSchema rightRecord)
        {
            var fields = new SortedDictionary<string, LogicalSchemaField>(StringComparer.Ordinal);
            foreach (var field in leftRecord.Fields)
                fields.Add(field.Key, field.Value);
            foreach (var field in rightRecord.Fields)
            {
                if (fields.TryGetValue(field.Key, out var existing))
                {
                    fields[field.Key] = new LogicalSchemaField(
                        Intersect(existing.Schema, field.Value.Schema, $"{path}.{field.Key}"),
                        existing.Optional || field.Value.Optional);
                }
                else
                {
                    fields.Add(field.Key, field.Value);
                }
            }
            return new RecordLogicalSchema(
                fields,
                leftRecord.AllowsAdditionalFields && rightRecord.AllowsAdditionalFields,
                IsNullable(left) || IsNullable(right));
        }
        if (left is ArrayLogicalSchema leftArray && right is ArrayLogicalSchema rightArray)
        {
            return new ArrayLogicalSchema(
                Intersect(leftArray.Items, rightArray.Items, $"{path}.items"),
                IsNullable(left) || IsNullable(right));
        }
        if (left is TupleLogicalSchema leftTuple && right is TupleLogicalSchema rightTuple)
        {
            var tuple = IntersectTuple(leftTuple, rightTuple, path);
            if (tuple is not null)
                return tuple;
        }
        if (left is PairLogicalSchema leftPair && right is PairLogicalSchema rightPair)
        {
            return new PairLogicalSchema(
                Intersect(leftPair.Key, rightPair.Key, $"{path}.key"),
                Intersect(leftPair.Value, rightPair.Value, $"{path}.value"),
                IsNullable(left) || IsNullable(right));
        }
        if (left is DictionaryLogicalSchema leftDictionary
            && right is DictionaryLogicalSchema rightDictionary)
        {
            return new DictionaryLogicalSchema(
                Intersect(leftDictionary.Keys, rightDictionary.Keys, $"{path}.keys"),
                Intersect(leftDictionary.Values, rightDictionary.Values, $"{path}.values"),
                IsNullable(left) || IsNullable(right));
        }
        if (left is GroupingLogicalSchema leftGrouping && right is GroupingLogicalSchema rightGrouping)
        {
            return new GroupingLogicalSchema(
                Intersect(leftGrouping.Keys, rightGrouping.Keys, $"{path}.keys"),
                Intersect(leftGrouping.Items, rightGrouping.Items, $"{path}.items"),
                IsNullable(left) || IsNullable(right));
        }
        if (left is SortTableLogicalSchema leftSortTable
            && right is SortTableLogicalSchema rightSortTable)
        {
            return new SortTableLogicalSchema(
                Intersect(leftSortTable.Items, rightSortTable.Items, $"{path}.items"),
                IsNullable(left) || IsNullable(right));
        }

        var conflict = new ConflictingLogicalSchema(left, right, IsNullable(left) || IsNullable(right));
        diagnostics.Add(new SchemaAnalysisDiagnostic(
            "schema.conflict",
            path,
            $"Schema constraints '{Describe(left)}' and '{Describe(right)}' are incompatible."));
        return conflict;
    }

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
