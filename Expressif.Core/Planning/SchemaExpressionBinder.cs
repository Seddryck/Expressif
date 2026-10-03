namespace Expressif.Planning;

internal sealed class SchemaExpressionBinder
{
    private const string ArraySchemaName = "array";
    private const string DictionarySchemaName = "dictionary";
    private const string GroupingSchemaName = "grouping";
    private const string NullableSchemaName = "nullable";
    private const string PairSchemaName = "pair";
    private const string SortTableSchemaName = "sort-table";
    private const string TupleSchemaName = "tuple";
    private const string UnionSchemaName = "union";
    private const string VariadicTupleSchemaName = "variadic-tuple";

    private readonly SchemaAlgebra algebra;
    private readonly Func<string, LogicalSchema> fromType;

    public SchemaExpressionBinder(SchemaAlgebra algebra, Func<string, LogicalSchema> fromType)
    {
        this.algebra = algebra;
        this.fromType = fromType;
    }

    public void Bind(
        SchemaExpression expression,
        LogicalSchema actual,
        IDictionary<string, LogicalSchema> bindings,
        string path)
        => Bind(expression, actual, new SchemaBindingSet(algebra, bindings), path);

    public LogicalSchema Resolve(
        SchemaExpression expression,
        IReadOnlyDictionary<string, LogicalSchema> bindings)
    {
        if (expression.IsVariable)
            return bindings.TryGetValue(expression.Name, out var value) ? value : new AnyLogicalSchema();
        return expression.Name switch
        {
            NullableSchemaName => SchemaAlgebra.WithNullability(Resolve(expression.Arguments.Single(), bindings), true),
            UnionSchemaName => expression.Arguments.Select(item => Resolve(item, bindings)).Aggregate(SchemaAlgebra.Union),
            ArraySchemaName => new ArrayLogicalSchema(Resolve(expression.Arguments.Single(), bindings)),
            TupleSchemaName => new TupleLogicalSchema(expression.Arguments.Select(item => Resolve(item, bindings)).ToArray()),
            VariadicTupleSchemaName => new TupleLogicalSchema(
                [],
                AdditionalItems: Resolve(expression.Arguments.Single(), bindings)),
            PairSchemaName => new PairLogicalSchema(
                Resolve(expression.Arguments[0], bindings),
                Resolve(expression.Arguments[1], bindings)),
            DictionarySchemaName => new DictionaryLogicalSchema(
                Resolve(expression.Arguments[0], bindings),
                Resolve(expression.Arguments[1], bindings)),
            GroupingSchemaName => new GroupingLogicalSchema(
                Resolve(expression.Arguments[0], bindings),
                Resolve(expression.Arguments[1], bindings)),
            SortTableSchemaName => new SortTableLogicalSchema(Resolve(expression.Arguments.Single(), bindings)),
            _ when expression.Arguments.Count == 0 => fromType(expression.Name),
            _ => throw new InvalidOperationException($"Unsupported schema constructor '{expression.Name}'."),
        };
    }

    private void Bind(
        SchemaExpression expression,
        LogicalSchema actual,
        SchemaBindingSet bindings,
        string path)
    {
        if (actual is NoInputLogicalSchema or AnyLogicalSchema)
            return;
        if (expression.Name == UnionSchemaName)
            BindChoice(expression, actual, bindings, path);
        else if (expression.IsVariable)
            bindings.Constrain(expression.Name, actual, path);
        else if (expression.Name == NullableSchemaName)
            Bind(expression.Arguments.Single(), actual, bindings, path);
        else if (actual is UnionLogicalSchema union)
            BindAlternatives(expression, union, bindings, path);
        else
            BindStructure(expression, actual, bindings, path);
    }

    private void BindChoice(
        SchemaExpression expression,
        LogicalSchema actual,
        SchemaBindingSet bindings,
        string path)
    {
        var exact = expression.Arguments.Where(alternative => AcceptsExactRoot(alternative, actual)).ToArray();
        var choices = exact.Length > 0
            ? exact
            : expression.Arguments.Where(alternative => AcceptsRoot(alternative, actual)).ToArray();
        if (choices.Length == 1)
        {
            Bind(choices[0], actual, bindings, path);
            return;
        }

        foreach (var choice in choices)
            bindings.MergeChoice(BindCandidate(choice, actual, path));
    }

    private void BindAlternatives(
        SchemaExpression expression,
        UnionLogicalSchema union,
        SchemaBindingSet bindings,
        string path)
    {
        var alternatives = union.Alternatives
            .Select((schema, index) => new IndexedSchema(schema, index))
            .Where(item => AcceptsRoot(expression, item.Schema))
            .ToArray();
        if (alternatives.Length == 1)
        {
            Bind(expression, alternatives[0].Schema, bindings, AlternativePath(path, alternatives[0].Index));
            return;
        }

        var candidates = alternatives
            .Select(item => BindCandidate(expression, item.Schema, AlternativePath(path, item.Index)))
            .ToArray();
        bindings.MergeAlternatives(candidates, path);
    }

    private void BindStructure(
        SchemaExpression expression,
        LogicalSchema actual,
        SchemaBindingSet bindings,
        string path)
    {
        foreach (var child in Children(expression, actual, path))
            Bind(child.Expression, child.Actual, bindings, child.Path);
    }

    private static IReadOnlyList<BindingChild> Children(
        SchemaExpression expression,
        LogicalSchema actual,
        string path)
        => (expression.Name, actual) switch
        {
            (ArraySchemaName, ArrayLogicalSchema array) =>
                [new(expression.Arguments.Single(), array.Items, $"{path}.items")],
            (ArraySchemaName, DictionaryLogicalSchema dictionary) =>
                [new(expression.Arguments.Single(), new PairLogicalSchema(dictionary.Keys, dictionary.Values), $"{path}.items")],
            (ArraySchemaName, GroupingLogicalSchema grouping) =>
                [new(expression.Arguments.Single(), new PairLogicalSchema(
                    grouping.Keys,
                    new ArrayLogicalSchema(grouping.Items)), $"{path}.items")],
            (TupleSchemaName, TupleLogicalSchema tuple) when expression.Arguments.Count == tuple.Items.Count =>
                expression.Arguments.Select((item, index) =>
                    new BindingChild(item, tuple.Items[index], $"{path}.items[{index}]")).ToArray(),
            (VariadicTupleSchemaName, TupleLogicalSchema tuple) =>
                [new(expression.Arguments.Single(), VariadicItem(tuple), $"{path}.items")],
            (PairSchemaName, PairLogicalSchema pair) =>
                [new(expression.Arguments[0], pair.Key, $"{path}.key"),
                    new(expression.Arguments[1], pair.Value, $"{path}.value")],
            (DictionarySchemaName, DictionaryLogicalSchema dictionary) =>
                [new(expression.Arguments[0], dictionary.Keys, $"{path}.keys"),
                    new(expression.Arguments[1], dictionary.Values, $"{path}.values")],
            (GroupingSchemaName, GroupingLogicalSchema grouping) =>
                [new(expression.Arguments[0], grouping.Keys, $"{path}.keys"),
                    new(expression.Arguments[1], grouping.Items, $"{path}.items")],
            (SortTableSchemaName, SortTableLogicalSchema sortTable) =>
                [new(expression.Arguments.Single(), sortTable.Items, $"{path}.items")],
            _ => [],
        };

    private SchemaBindingSet BindCandidate(SchemaExpression expression, LogicalSchema actual, string path)
    {
        var candidate = SchemaBindingSet.Empty(algebra);
        Bind(expression, actual, candidate, path);
        return candidate;
    }

    private static LogicalSchema VariadicItem(TupleLogicalSchema tuple)
        => tuple.Items
            .Concat(tuple.AdditionalItems is null ? [] : [tuple.AdditionalItems])
            .DefaultIfEmpty(new AnyLogicalSchema())
            .Aggregate(SchemaAlgebra.Union);

    private static bool AcceptsRoot(SchemaExpression expression, LogicalSchema actual)
    {
        if (expression.IsVariable)
            return true;
        if (actual is UnionLogicalSchema union)
            return union.Alternatives.Any(alternative => AcceptsRoot(expression, alternative));
        if (expression.Name == UnionSchemaName)
            return expression.Arguments.Any(alternative => AcceptsRoot(alternative, actual));
        if (expression.Name == NullableSchemaName)
            return AcceptsRoot(expression.Arguments.Single(), actual);
        return MatchesRoot(expression, actual, exact: false);
    }

    private static bool AcceptsExactRoot(SchemaExpression expression, LogicalSchema actual)
    {
        if (expression.IsVariable)
            return true;
        if (expression.Name == NullableSchemaName)
            return AcceptsExactRoot(expression.Arguments.Single(), actual);
        return MatchesRoot(expression, actual, exact: true);
    }

    private static bool MatchesRoot(SchemaExpression expression, LogicalSchema actual, bool exact)
        => (expression.Name, actual) switch
        {
            (ArraySchemaName, ArrayLogicalSchema) => true,
            (ArraySchemaName, DictionaryLogicalSchema or GroupingLogicalSchema) => !exact,
            ("record", RecordLogicalSchema) => exact,
            (TupleSchemaName or VariadicTupleSchemaName, TupleLogicalSchema) => true,
            (PairSchemaName, PairLogicalSchema) => true,
            (DictionarySchemaName, DictionaryLogicalSchema) => true,
            (GroupingSchemaName, GroupingLogicalSchema) => true,
            (SortTableSchemaName, SortTableLogicalSchema) => true,
            _ => expression.Arguments.Count == 0 && actual is ScalarLogicalSchema scalar
                && SchemaAlgebra.IntersectScalar(expression.Name, scalar.Type) is not null,
        };

    private static string AlternativePath(string path, int index) => $"{path}.alternatives[{index}]";

    private sealed record BindingChild(SchemaExpression Expression, LogicalSchema Actual, string Path);

    private sealed record IndexedSchema(LogicalSchema Schema, int Index);
}
