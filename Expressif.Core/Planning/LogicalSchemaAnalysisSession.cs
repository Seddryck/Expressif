namespace Expressif.Planning;

internal sealed partial class LogicalSchemaAnalysisSession
{
    private const string ArraySchemaName = "array";
    private const string DictionarySchemaName = "dictionary";
    private const string DynamicDiagnosticCode = "schema.dynamic";
    private const string GroupingSchemaName = "grouping";
    private const string NullableSchemaName = "nullable";
    private const string SelectorParameterName = "selector";
    private const string SortTableSchemaName = "sort-table";
    private const string TupleSchemaName = "tuple";
    private const string UnionSchemaName = "union";
    private const string ValueParameterName = "value";
    private const string VariadicTupleSchemaName = "variadic-tuple";

    private readonly SchemaDiagnosticBag diagnostics = new();
    private readonly SchemaAnalysisTrace trace = new();
    private readonly IReadOnlyDictionary<string, LogicalNamedExpressionDefinition> definitions;
    private readonly SchemaAlgebra algebra;
    private readonly SchemaExpressionBinder schemaBinder;
    private readonly RecordSchemaInferenceRule recordInference;
    private readonly IntrinsicRuleRegistry intrinsicRules;

    public LogicalSchemaAnalysisSession(IReadOnlyList<LogicalNamedExpressionDefinition> definitions)
    {
        this.definitions = definitions.ToDictionary(definition => definition.Name, StringComparer.Ordinal);
        algebra = new SchemaAlgebra(diagnostics);
        schemaBinder = new SchemaExpressionBinder(algebra, FromType);
        recordInference = new RecordSchemaInferenceRule(diagnostics);
        intrinsicRules = IntrinsicRuleRegistry.Create(this);
    }

    public IReadOnlyList<SchemaAnalysisDiagnostic> Diagnostics => diagnostics;

    public IReadOnlyList<SchemaAnalysisNode> Nodes => trace.GetNodes();

    internal void RegisterIntrinsicRule(IIntrinsicSchemaRule rule)
        => intrinsicRules.Register(rule);

    public void AnalyzeDefinition(LogicalNamedExpressionDefinition definition, string path)
    {
        var input = definition.InputContract is null
            ? new AnyLogicalSchema()
            : FromType(definition.InputContract.Type);
        _ = RequirePipeline(definition.Body,
            definition.OutputContract is null ? new AnyLogicalSchema() : FromType(definition.OutputContract.Type),
            $"{path}.body");
        _ = InferPipeline(definition.Body, input, input, $"{path}.body");
    }

    public LogicalSchema RequirePipeline(LogicalPipeline pipeline, LogicalSchema expected, string path)
    {
        var current = expected;
        var enclosing = (LogicalSchema)new AnyLogicalSchema();
        for (var index = pipeline.Items.Count - 1; index >= 0; index--)
        {
            var requirement = Require(pipeline.Items[index], current, $"{path}.items[{index}]");
            current = requirement.Input;
            enclosing = Intersect(enclosing, requirement.Enclosing, $"{path}.enclosing");
        }
        return Intersect(current, enclosing, $"{path}.input");
    }

    public LogicalSchema InferPipeline(
        LogicalPipeline pipeline,
        LogicalSchema input,
        LogicalSchema enclosing,
        string path)
    {
        var current = input;
        for (var index = 0; index < pipeline.Items.Count; index++)
            current = Infer(pipeline.Items[index], current, enclosing, $"{path}.items[{index}]");
        Capture(pipeline, path, input, current);
        return current;
    }

    public LogicalSchema Intersect(LogicalSchema left, LogicalSchema right, string path)
        => algebra.Intersect(left, right, path);

    public SchemaAnalysisCompleteness Completeness(LogicalSchema input, LogicalSchema output)
    {
        if (ContainsConflict(input) || ContainsConflict(output))
            return SchemaAnalysisCompleteness.Conflicting;
        if (diagnostics.Any(diagnostic => diagnostic.Code == DynamicDiagnosticCode))
            return SchemaAnalysisCompleteness.Dynamic;
        return ContainsAny(input) || ContainsAny(output)
            ? SchemaAnalysisCompleteness.Partial
            : SchemaAnalysisCompleteness.Known;
    }

    private Requirement Require(LogicalValue value, LogicalSchema expected, string path) => value switch
    {
        LogicalLiteral => new Requirement(new NoInputLogicalSchema(), new NoInputLogicalSchema()),
        LogicalPipeline pipeline => new Requirement(RequirePipeline(pipeline, expected, path), new AnyLogicalSchema()),
        LogicalCall call => RequireCall(call, expected, path),
        LogicalNamedExpressionInvocation invocation => RequireInvocation(invocation, path),
        _ => DynamicRequirement(path, value.GetType().Name),
    };

    private Requirement RequireInvocation(LogicalNamedExpressionInvocation invocation, string path)
    {
        for (var index = 0; index < invocation.Arguments.Count; index++)
            _ = Require(invocation.Arguments[index], new AnyLogicalSchema(), $"{path}.arguments[{index}]");
        return definitions[invocation.Name].InputContract is { } contract
            ? new Requirement(FromType(contract.Type), new AnyLogicalSchema())
            : DynamicRequirement(path, $"input of named expression '{invocation.Name}'");
    }

    private Requirement RequireCall(LogicalCall call, LogicalSchema expected, string path)
    {
        if (call.Function.Kind == "extension"
            && call.Function.Name is ArraySchemaName or TupleSchemaName or "vector")
        {
            return RequireStructuralCollection(call, path);
        }
        var schema = call.Function.Schema;
        if (schema?.Classification == "intrinsic" && schema.Intrinsic is not null)
            return RequireIntrinsic(call, expected, path, schema.Intrinsic);
        return schema?.Classification == "contract"
            ? RequireContract(call, expected, path, schema)
            : RequireGenericCall(call, path);
    }

    private Requirement RequireStructuralCollection(LogicalCall call, string path)
    {
        var input = (LogicalSchema)new NoInputLogicalSchema();
        var enclosing = (LogicalSchema)new NoInputLogicalSchema();
        for (var index = 0; index < call.Arguments.Count; index++)
        {
            var argument = call.Arguments[index];
            if (!argument.IsExplicit || argument.Value is null)
                continue;
            var requirement = Require(
                argument.Value,
                new AnyLogicalSchema(),
                $"{path}.arguments[{index}]");
            input = Intersect(input, requirement.Input, $"{path}.input");
            enclosing = Intersect(enclosing, requirement.Enclosing, $"{path}.enclosing");
        }
        return new Requirement(input, enclosing);
    }

    private Requirement RequireIntrinsic(
        LogicalCall call,
        LogicalSchema expected,
        string path,
        string intrinsic)
        => intrinsicRules.Require(intrinsic, call, expected, path);

    private Requirement RequireContract(
        LogicalCall call,
        LogicalSchema expected,
        string path,
        PlannerSchemaDescriptor contract)
    {
        var bindings = new Dictionary<string, LogicalSchema>(StringComparer.Ordinal);
        if (contract.Output is not null)
            schemaBinder.Bind(SchemaExpressionParser.Parse(contract.Output), expected, bindings, $"{path}.output");

        var enclosing = (LogicalSchema)new AnyLogicalSchema();
        var argumentRequirements = new List<(LogicalArgument Argument, Requirement Requirement)>();
        var parameterCounts = call.Arguments
            .Where(argument => argument.IsExplicit && argument.Value is not null)
            .GroupBy(argument => argument.Parameter.Name, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
        var parameterOrdinals = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var index = 0; index < call.Arguments.Count; index++)
        {
            var argument = call.Arguments[index];
            if (!argument.IsExplicit || argument.Value is null)
                continue;
            PlannerParameterSchemaDescriptor? parameterContract = null;
            contract.Parameters?.TryGetValue(argument.Parameter.Name, out parameterContract);
            var ordinal = parameterOrdinals.GetValueOrDefault(argument.Parameter.Name);
            parameterOrdinals[argument.Parameter.Name] = ordinal + 1;
            var expectedArgument = ExpectedParameterOutput(
                argument,
                parameterContract,
                bindings,
                parameterCounts[argument.Parameter.Name],
                ordinal);
            var requirement = Require(argument.Value, expectedArgument, $"{path}.arguments[{index}]");
            argumentRequirements.Add((argument, requirement));
            if (parameterContract?.Input is not null)
            {
                var parameterInput = Intersect(
                    requirement.Input,
                    requirement.Enclosing,
                    $"{path}.parameters.{argument.Parameter.Name}");
                schemaBinder.Bind(
                    SchemaExpressionParser.Parse(parameterContract.Input),
                    parameterInput,
                    bindings,
                    $"{path}.parameters.{argument.Parameter.Name}");
            }
        }

        var input = contract.Input is null
            ? FromType(call.Function.Input)
            : schemaBinder.Resolve(SchemaExpressionParser.Parse(contract.Input), bindings);
        foreach (var (argument, requirement) in argumentRequirements)
        {
            PlannerParameterSchemaDescriptor? parameterContract = null;
            contract.Parameters?.TryGetValue(argument.Parameter.Name, out parameterContract);
            if (parameterContract?.Input is not null)
                continue;
            var applied = ApplyArgumentRequirement(
                argument.Parameter.Evaluation,
                requirement,
                input,
                enclosing,
                path);
            input = applied.Input;
            enclosing = applied.Enclosing;
        }
        return new Requirement(input, enclosing);
    }

    private Requirement RequireField(LogicalCall call, LogicalSchema expected, string path)
    {
        var name = LiteralText(call, "name");
        if (name is null)
            return DynamicRequirement(path, "field with a dynamic name");
        var fields = new SortedDictionary<string, LogicalSchemaField>(StringComparer.Ordinal)
        {
            [name] = new(expected, Optional: true),
        };
        var record = new RecordLogicalSchema(fields);
        return call.ContextDepth == 0
            ? new Requirement(record, new AnyLogicalSchema())
            : new Requirement(new AnyLogicalSchema(), record);
    }

    private Requirement RequireSelectFields(LogicalCall call, LogicalSchema expected, string path)
    {
        var generic = RequireGenericCall(call, path);
        var names = LiteralTexts(Argument(call, "names")?.Value);
        var fields = new SortedDictionary<string, LogicalSchemaField>(StringComparer.Ordinal);
        if (expected is RecordLogicalSchema record)
        {
            foreach (var field in record.Fields.Where(field =>
                names is null || names.Contains(field.Key, StringComparer.Ordinal)))
            {
                fields.Add(field.Key, field.Value);
            }
        }
        var required = new RecordLogicalSchema(fields);
        return new Requirement(
            Intersect(generic.Input, required, $"{path}.input"),
            generic.Enclosing);
    }

    private Requirement RequireExplode(LogicalCall call, LogicalSchema expected, string path)
    {
        var name = SelectedField(call, SelectorParameterName);
        if (name is null)
            return DynamicRequirement(path, "explode selector");

        var output = expected as ArrayLogicalSchema;
        var outputParent = output?.Items as RecordLogicalSchema;
        var child = outputParent is not null
            && outputParent.Fields.TryGetValue(name, out var selected)
                ? selected.Schema
                : new AnyLogicalSchema();
        var fields = new SortedDictionary<string, LogicalSchemaField>(StringComparer.Ordinal);
        if (outputParent is not null)
        {
            foreach (var field in outputParent.Fields)
                fields.Add(field.Key, field.Value);
        }
        fields[name] = new LogicalSchemaField(
            new ArrayLogicalSchema(child, IsNullable: true),
            Optional: true);
        var parent = new RecordLogicalSchema(
            fields,
            outputParent?.AllowsAdditionalFields ?? true);
        var input = new UnionLogicalSchema(
            [parent, new ArrayLogicalSchema(parent)],
            IsNullable(expected));
        return new Requirement(input, new AnyLogicalSchema());
    }

    private Requirement RequireImplode(LogicalCall call, LogicalSchema expected, string path)
    {
        var name = SelectedField(call, SelectorParameterName);
        if (name is null)
            return DynamicRequirement(path, "implode selector");

        var output = expected as ArrayLogicalSchema;
        var outputParent = output?.Items as RecordLogicalSchema;
        var child = outputParent is not null
            && outputParent.Fields.TryGetValue(name, out var selected)
            && selected.Schema is ArrayLogicalSchema array
                ? array.Items
                : new AnyLogicalSchema();
        var fields = new SortedDictionary<string, LogicalSchemaField>(StringComparer.Ordinal);
        if (outputParent is not null)
        {
            foreach (var field in outputParent.Fields)
                fields.Add(field.Key, field.Value);
        }
        fields[name] = new LogicalSchemaField(WithNullability(child, true), Optional: true);
        var parent = new RecordLogicalSchema(
            fields,
            outputParent?.AllowsAdditionalFields ?? true);
        return new Requirement(
            new ArrayLogicalSchema(parent, IsNullable(expected)),
            new AnyLogicalSchema());
    }

    private Requirement RequireWith(LogicalCall call, LogicalSchema expected, string path)
    {
        var bodyIndex = call.Arguments
            .Select((argument, index) => (argument, index))
            .Single(item => item.argument.Parameter.Name == "body").index;
        var body = call.Arguments[bodyIndex].Value
            ?? throw new InvalidOperationException("The 'with' body argument must have a value.");
        var bodyRequirement = Require(body, expected, $"{path}.arguments[{bodyIndex}].value");
        var temporary = Intersect(
            bodyRequirement.Input,
            bodyRequirement.Enclosing,
            $"{path}.temporary");
        var temporaryRecord = temporary as RecordLogicalSchema;
        var input = (LogicalSchema)new AnyLogicalSchema();
        for (var index = 0; index < call.Arguments.Count; index++)
        {
            var argument = call.Arguments[index];
            if (argument.Parameter.Name != "projections"
                || argument.Value is not LogicalCall entry)
                continue;
            var name = LiteralText(entry, "name");
            var projection = Argument(entry, ValueParameterName)?.Value;
            if (projection is null)
                continue;
            var projectionExpected = name is not null
                && temporaryRecord?.Fields.TryGetValue(name, out var field) == true
                    ? field.Schema
                    : new AnyLogicalSchema();
            var requirement = Require(
                projection,
                projectionExpected,
                $"{path}.arguments[{index}].value.arguments[1].value");
            input = Intersect(input, requirement.Input, $"{path}.projections[{index}].input");
            input = Intersect(input, requirement.Enclosing, $"{path}.projections[{index}].enclosing");
        }
        return new Requirement(input, new AnyLogicalSchema());
    }

    private Requirement RequireSpreadEntry(LogicalCall call, LogicalSchema expected, string path)
    {
        var value = Argument(call, ValueParameterName)?.Value;
        return value is null
            ? DynamicRequirement(path, "record spread")
            : Require(value, expected, $"{path}.arguments[0].value");
    }

    private Requirement RequireSortCriterion(LogicalCall call, string path)
    {
        var selector = Argument(call, SelectorParameterName)?.Value;
        if (selector is null)
            return DynamicRequirement(path, "sort criterion selector");
        var requirement = Require(selector, new AnyLogicalSchema(), $"{path}.arguments[0].value");
        return new Requirement(
            Intersect(requirement.Input, requirement.Enclosing, $"{path}.selector"),
            new AnyLogicalSchema());
    }

    private Requirement RequireGenericCall(LogicalCall call, string path)
    {
        var input = FromType(call.Function.Input);
        var enclosing = (LogicalSchema)new AnyLogicalSchema();
        for (var index = 0; index < call.Arguments.Count; index++)
        {
            var argument = call.Arguments[index];
            if (!argument.IsExplicit || argument.Value is null)
                continue;
            var expected = FromType(argument.Parameter.Type);
            var requirement = Require(argument.Value, expected, $"{path}.arguments[{index}]");
            var applied = ApplyArgumentRequirement(
                argument.Parameter.Evaluation,
                requirement,
                input,
                enclosing,
                path);
            input = applied.Input;
            enclosing = applied.Enclosing;
        }
        return new Requirement(input, enclosing);
    }

    private Requirement ApplyArgumentRequirement(
        PlannerEvaluationDescriptor? evaluation,
        Requirement requirement,
        LogicalSchema input,
        LogicalSchema enclosing,
        string path)
    {
        if (evaluation?.Context == "traversal" && input is ArrayLogicalSchema array)
        {
            input = array with
            {
                Items = Intersect(array.Items, requirement.Input, $"{path}.traversal"),
            };
        }
        else if (evaluation?.Source is "enclosing" or "surrounding")
        {
            enclosing = Intersect(enclosing, requirement.Input, $"{path}.enclosing");
        }
        else if (evaluation?.Source == "incoming")
        {
            input = Intersect(input, requirement.Input, $"{path}.incoming");
        }
        return new Requirement(input, enclosing);
    }

    private LogicalSchema Infer(
        LogicalValue value,
        LogicalSchema input,
        LogicalSchema enclosing,
        string path)
    {
        var output = value switch
        {
            LogicalLiteral literal => FromType(literal.Type),
            LogicalPipeline pipeline => InferPipeline(pipeline, input, enclosing, path),
            LogicalCall call => InferCall(call, input, enclosing, path),
            LogicalNamedExpressionInvocation invocation => InferInvocation(invocation, input, enclosing, path),
            _ => Dynamic(path, value.GetType().Name),
        };
        Capture(value, path, input, output);
        return output;
    }

    private LogicalSchema InferInvocation(
        LogicalNamedExpressionInvocation invocation,
        LogicalSchema input,
        LogicalSchema enclosing,
        string path)
    {
        for (var index = 0; index < invocation.Arguments.Count; index++)
            _ = Infer(invocation.Arguments[index], input, enclosing, $"{path}.arguments[{index}]");
        return definitions[invocation.Name].OutputContract is { } contract
            ? FromType(contract.Type)
            : Dynamic(path, $"output of named expression '{invocation.Name}'");
    }

    private LogicalSchema InferCall(
        LogicalCall call,
        LogicalSchema input,
        LogicalSchema enclosing,
        string path)
    {
        var schema = call.Function.Schema;
        if (schema?.Classification == "dynamic")
            return InferDynamicCall(call, input, enclosing, path, schema.DynamicReason);
        if (schema?.Classification == "intrinsic" && schema.Intrinsic is not null)
            return InferIntrinsic(call, input, enclosing, path, schema.Intrinsic);
        return schema?.Classification == "contract"
            ? InferContract(call, input, enclosing, path, schema)
            : InferGenericCall(call, input, enclosing, path);
    }

    private LogicalSchema InferIntrinsic(
        LogicalCall call,
        LogicalSchema input,
        LogicalSchema enclosing,
        string path,
        string intrinsic)
        => intrinsicRules.Infer(intrinsic, call, input, enclosing, path);

    private LogicalSchema InferContract(
        LogicalCall call,
        LogicalSchema input,
        LogicalSchema enclosing,
        string path,
        PlannerSchemaDescriptor contract)
    {
        var bindings = new Dictionary<string, LogicalSchema>(StringComparer.Ordinal);
        if (contract.Input is not null)
            schemaBinder.Bind(SchemaExpressionParser.Parse(contract.Input), input, bindings, $"{path}.input");
        var parameterResults = InferContractArguments(call, input, enclosing, path, contract, bindings);
        if (contract.Input is not null && contract.Output == contract.Input
            && SchemaExpressionParser.Parse(contract.Input).ContainsVariable)
        {
            return input;
        }
        var output = contract.Output is null
            ? FromType(call.Function.Output)
            : schemaBinder.Resolve(SchemaExpressionParser.Parse(contract.Output), bindings);
        return IsConditionallyNullable(contract, input, parameterResults)
            ? WithNullability(output, true)
            : output;
    }

    private IReadOnlyDictionary<string, List<LogicalSchema>> InferContractArguments(
        LogicalCall call,
        LogicalSchema input,
        LogicalSchema enclosing,
        string path,
        PlannerSchemaDescriptor contract,
        Dictionary<string, LogicalSchema> bindings)
    {
        var parameterOutputs = new Dictionary<
            string,
            (PlannerParameterSchemaDescriptor Contract, List<LogicalSchema> Outputs)>(StringComparer.Ordinal);
        var parameterResults = new Dictionary<string, List<LogicalSchema>>(StringComparer.Ordinal);
        for (var index = 0; index < call.Arguments.Count; index++)
        {
            var argument = call.Arguments[index];
            if (!argument.IsExplicit || argument.Value is null)
                continue;
            InferContractArgument(
                argument,
                argument.Value,
                index,
                input,
                enclosing,
                path,
                contract,
                bindings,
                parameterResults,
                parameterOutputs);
        }
        BindParameterOutputs(parameterOutputs, bindings, path);
        return parameterResults;
    }

    private void InferContractArgument(
        LogicalArgument argument,
        LogicalValue value,
        int index,
        LogicalSchema input,
        LogicalSchema enclosing,
        string path,
        PlannerSchemaDescriptor contract,
        Dictionary<string, LogicalSchema> bindings,
        IDictionary<string, List<LogicalSchema>> parameterResults,
        IDictionary<string, (PlannerParameterSchemaDescriptor Contract, List<LogicalSchema> Outputs)> parameterOutputs)
    {
        PlannerParameterSchemaDescriptor? parameterContract = null;
        contract.Parameters?.TryGetValue(argument.Parameter.Name, out parameterContract);
        var context = parameterContract?.Input is null
            ? ArgumentContext(argument, input, enclosing)
            : schemaBinder.Resolve(SchemaExpressionParser.Parse(parameterContract.Input), bindings);
        if (parameterContract?.Input is not null)
        {
            schemaBinder.Bind(
                SchemaExpressionParser.Parse(parameterContract.Input),
                context,
                bindings,
                $"{path}.parameters.{argument.Parameter.Name}.input");
        }
        var result = Infer(value, context, context, $"{path}.arguments[{index}].value");
        CollectParameterResult(parameterResults, argument.Parameter.Name, result);
        CollectParameterOutput(parameterOutputs, argument.Parameter.Name, parameterContract, result);
    }

    private static void CollectParameterResult(
        IDictionary<string, List<LogicalSchema>> parameterResults,
        string name,
        LogicalSchema result)
    {
        if (!parameterResults.TryGetValue(name, out var results))
        {
            results = [];
            parameterResults.Add(name, results);
        }
        results.Add(result);
    }

    private static void CollectParameterOutput(
        IDictionary<string, (PlannerParameterSchemaDescriptor Contract, List<LogicalSchema> Outputs)> parameterOutputs,
        string name,
        PlannerParameterSchemaDescriptor? contract,
        LogicalSchema result)
    {
        if (contract?.Output is null)
            return;
        if (!parameterOutputs.TryGetValue(name, out var collected))
        {
            collected = (contract, []);
            parameterOutputs.Add(name, collected);
        }
        collected.Outputs.Add(result);
    }

    private void BindParameterOutputs(
        IReadOnlyDictionary<string, (PlannerParameterSchemaDescriptor Contract, List<LogicalSchema> Outputs)> parameterOutputs,
        IDictionary<string, LogicalSchema> bindings,
        string path)
    {
        foreach (var parameter in parameterOutputs)
        {
            if (parameter.Value.Contract.Output is not string expression)
                continue;
            schemaBinder.Bind(
                SchemaExpressionParser.Parse(expression),
                CombineParameterOutputs(parameter.Value.Outputs, parameter.Value.Contract.Combine),
                bindings,
                $"{path}.parameters.{parameter.Key}.output");
        }
    }

    private static bool IsConditionallyNullable(
        PlannerSchemaDescriptor contract,
        LogicalSchema input,
        IReadOnlyDictionary<string, List<LogicalSchema>> parameterResults)
    {
        if (contract.Nullability == "propagate-input" && IsNullable(input))
            return true;
        return contract.NullableWhen?.Any(source => source == "input"
            ? IsNullable(input)
            : parameterResults.TryGetValue(source, out var results) && results.Any(IsNullable)) == true;
    }

    private LogicalSchema ExpectedParameterOutput(
        LogicalArgument argument,
        PlannerParameterSchemaDescriptor? contract,
        IReadOnlyDictionary<string, LogicalSchema> bindings,
        int count,
        int ordinal)
    {
        if (contract?.Output is null)
            return FromType(argument.Parameter.Type);
        var expected = schemaBinder.Resolve(SchemaExpressionParser.Parse(contract.Output), bindings);
        return contract.Combine == TupleSchemaName && count > 1
            && expected is TupleLogicalSchema tuple && tuple.Items.Count == count
                ? tuple.Items[ordinal]
                : expected;
    }

    private LogicalSchema CombineParameterOutputs(
        IReadOnlyList<LogicalSchema> outputs,
        string? combination) => combination switch
        {
            UnionSchemaName => outputs.Aggregate(Union),
            TupleSchemaName when outputs.Count > 1 => new TupleLogicalSchema(outputs),
            TupleSchemaName => outputs.Single(),
            null when outputs.Count == 1 => outputs[0],
            null => outputs.Aggregate(IntersectForParameter),
            _ => throw new InvalidOperationException($"Unsupported schema combination '{combination}'."),
        };

    private LogicalSchema IntersectForParameter(LogicalSchema left, LogicalSchema right)
        => Intersect(left, right, "plan.parameter");

    private LogicalSchema InferField(
        LogicalCall call,
        LogicalSchema input,
        LogicalSchema enclosing,
        string path)
    {
        InferArgument(call, "name", input, enclosing, path);
        var name = LiteralText(call, "name");
        var source = call.ContextDepth == 0 ? input : enclosing;
        if (name is not null
            && source is RecordLogicalSchema record
            && record.Fields.TryGetValue(name, out var field))
        {
            return field.Optional ? WithNullability(field.Schema, true) : field.Schema;
        }
        return Dynamic(path, name is null ? "field with a dynamic name" : $"field '{name}'");
    }

    private LogicalSchema InferSelectFields(
        LogicalCall call,
        LogicalSchema input,
        LogicalSchema enclosing,
        string path)
    {
        InferArgument(call, "names", input, enclosing, path);
        var names = LiteralTexts(Argument(call, "names")?.Value);
        if (input is not RecordLogicalSchema record || names is null)
            return Dynamic(path, "selected record fields");
        var fields = new SortedDictionary<string, LogicalSchemaField>(StringComparer.Ordinal);
        foreach (var name in names.Distinct(StringComparer.Ordinal))
        {
            if (record.Fields.TryGetValue(name, out var field))
                fields.Add(name, field);
            else if (record.AllowsAdditionalFields)
                fields.Add(name, new LogicalSchemaField(new AnyLogicalSchema(), Optional: true));
        }
        return new RecordLogicalSchema(fields, AllowsAdditionalFields: false, record.IsNullable);
    }

    private LogicalSchema InferExplode(
        LogicalCall call,
        LogicalSchema input,
        string path,
        bool preserveParent)
    {
        var name = SelectedField(call, SelectorParameterName);
        if (name is null)
            return Dynamic(path, "explode selector");

        var parent = input switch
        {
            RecordLogicalSchema record => record,
            ArrayLogicalSchema { Items: RecordLogicalSchema record } => record,
            _ => null,
        };
        if (parent is null)
            return Dynamic(path, "explode parent record");

        var selector = Argument(call, SelectorParameterName);
        if (selector?.Value is not null)
        {
            var selectorIndex = call.Arguments
                .Select((argument, index) => (argument, index))
                .Single(item => item.argument.Parameter.Name == SelectorParameterName)
                .index;
            Infer(
                selector.Value,
                parent,
                parent,
                $"{path}.arguments[{selectorIndex}].value");
        }

        LogicalSchema child;
        if (parent.Fields.TryGetValue(name, out var selected))
        {
            if (selected.Schema is not ArrayLogicalSchema array)
            {
                diagnostics.Add(new SchemaAnalysisDiagnostic(
                    "schema.conflict",
                    path,
                    $"Field '{name}' must be an array to be exploded."));
                child = new AnyLogicalSchema();
            }
            else
            {
                child = array.Items;
            }
        }
        else
        {
            child = new AnyLogicalSchema();
        }

        if (preserveParent)
            child = WithNullability(child, true);
        var fields = new SortedDictionary<string, LogicalSchemaField>(StringComparer.Ordinal);
        foreach (var field in parent.Fields)
            fields.Add(field.Key, field.Value);
        fields[name] = new LogicalSchemaField(child);
        return new ArrayLogicalSchema(
            new RecordLogicalSchema(fields, parent.AllowsAdditionalFields),
            IsNullable(input));
    }

    private LogicalSchema InferImplode(
        LogicalCall call,
        LogicalSchema input,
        string path,
        bool preserveNull)
    {
        var name = SelectedField(call, SelectorParameterName);
        if (name is null)
            return Dynamic(path, "implode selector");

        if (input is not ArrayLogicalSchema { Items: RecordLogicalSchema parent })
            return Dynamic(path, "implode parent record");

        var child = parent.Fields.TryGetValue(name, out var selected)
            ? selected.Schema
            : new AnyLogicalSchema(IsNullable: true);
        if (!preserveNull)
            child = WithNullability(child, false);
        var selector = Argument(call, SelectorParameterName);
        if (selector?.Value is not null)
        {
            var selectorIndex = call.Arguments
                .Select((argument, index) => (argument, index))
                .Single(item => item.argument.Parameter.Name == SelectorParameterName)
                .index;
            Infer(
                selector.Value,
                parent,
                parent,
                $"{path}.arguments[{selectorIndex}].value");
        }

        var fields = new SortedDictionary<string, LogicalSchemaField>(StringComparer.Ordinal);
        foreach (var field in parent.Fields)
            fields.Add(field.Key, field.Value);
        fields[name] = new LogicalSchemaField(new ArrayLogicalSchema(child));
        return new ArrayLogicalSchema(
            new RecordLogicalSchema(fields, parent.AllowsAdditionalFields),
            IsNullable(input));
    }

    private LogicalSchema InferArray(
        LogicalCall call,
        LogicalSchema input,
        LogicalSchema enclosing,
        string path)
    {
        LogicalSchema? item = null;
        for (var index = 0; index < call.Arguments.Count; index++)
        {
            var value = InferArgument(call, index, input, enclosing, path);
            if (value is not null)
                item = item is null ? value : Union(item, value);
        }
        return new ArrayLogicalSchema(item ?? new AnyLogicalSchema());
    }

    private LogicalSchema InferTuple(
        LogicalCall call,
        LogicalSchema input,
        LogicalSchema enclosing,
        string path)
    {
        var items = new List<LogicalSchema>();
        LogicalSchema? additionalItems = null;
        for (var index = 0; index < call.Arguments.Count; index++)
        {
            var argument = call.Arguments[index];
            var value = InferArgument(call, index, input, enclosing, path);
            if (value is null)
                continue;
            if (!argument.IsSpread && additionalItems is null)
            {
                items.Add(value);
                continue;
            }
            if (!argument.IsSpread && additionalItems is LogicalSchema existing)
            {
                additionalItems = Union(existing, value);
                continue;
            }
            switch (value)
            {
                case TupleLogicalSchema tuple when additionalItems is null:
                    items.AddRange(tuple.Items);
                    additionalItems = tuple.AdditionalItems;
                    break;
                case TupleLogicalSchema tuple:
                    additionalItems = tuple.Items
                        .Append(tuple.AdditionalItems ?? new AnyLogicalSchema())
                        .Aggregate(additionalItems, Union);
                    break;
                case ArrayLogicalSchema array:
                    additionalItems = additionalItems is null
                        ? array.Items
                        : Union(additionalItems, array.Items);
                    break;
                default:
                    additionalItems = new AnyLogicalSchema();
                    diagnostics.Add(new SchemaAnalysisDiagnostic(
                        DynamicDiagnosticCode,
                        $"{path}.arguments[{index}]",
                        "Tuple spread has an unknown item schema."));
                    break;
            }
        }
        return new TupleLogicalSchema(items, AdditionalItems: additionalItems);
    }

    private LogicalSchema InferAssociative(
        LogicalCall call,
        LogicalSchema input,
        LogicalSchema enclosing,
        string path,
        bool grouping)
    {
        LogicalSchema? keys = null;
        LogicalSchema? values = null;
        for (var index = 0; index < call.Arguments.Count; index++)
        {
            var argument = call.Arguments[index];
            var result = InferArgument(call, index, input, enclosing, path);
            var pair = result switch
            {
                PairLogicalSchema direct => direct,
                ArrayLogicalSchema { Items: PairLogicalSchema spread } when argument.IsSpread => spread,
                _ => null,
            };
            if (pair is null)
                continue;
            keys = keys is null ? pair.Key : Union(keys, pair.Key);
            values = values is null ? pair.Value : Union(values, pair.Value);
        }
        keys ??= new AnyLogicalSchema();
        values ??= new AnyLogicalSchema();
        return grouping
            ? new GroupingLogicalSchema(keys, values)
            : new DictionaryLogicalSchema(keys, values);
    }

    private LogicalSchema InferRecord(
        LogicalCall call,
        LogicalSchema input,
        LogicalSchema enclosing,
        string path)
        => recordInference.Infer(
            call,
            path,
            index => InferArgument(call, index, input, enclosing, path),
            entry => LiteralText(entry, "name"));

    private LogicalSchema InferWith(
        LogicalCall call,
        LogicalSchema input,
        LogicalSchema enclosing,
        string path)
    {
        var fields = new SortedDictionary<string, LogicalSchemaField>(StringComparer.Ordinal);
        for (var index = 0; index < call.Arguments.Count; index++)
        {
            var argument = call.Arguments[index];
            if (argument.Parameter.Name != "projections"
                || argument.Value is not LogicalCall entry)
                continue;
            var value = InferArgument(call, index, input, enclosing, path);
            var name = LiteralText(entry, "name");
            if (name is not null && value is not null)
                fields[name] = new LogicalSchemaField(value);
        }

        var temporary = new RecordLogicalSchema(fields);
        var body = call.Arguments
            .Select((argument, index) => (argument, index))
            .Single(item => item.argument.Parameter.Name == "body");
        var bodyValue = body.argument.Value
            ?? throw new InvalidOperationException("The 'with' body argument must have a value.");
        return Infer(
            bodyValue,
            temporary,
            temporary,
            $"{path}.arguments[{body.index}].value");
    }

    private LogicalSchema InferPut(
        LogicalCall call,
        LogicalSchema input,
        LogicalSchema enclosing,
        string path,
        RecordMutation mutation)
    {
        var source = input as RecordLogicalSchema
            ?? new RecordLogicalSchema(new SortedDictionary<string, LogicalSchemaField>(StringComparer.Ordinal));
        var fields = new SortedDictionary<string, LogicalSchemaField>(StringComparer.Ordinal);
        foreach (var field in source.Fields)
            fields.Add(field.Key, field.Value);
        for (var index = 0; index < call.Arguments.Count; index++)
        {
            var argument = call.Arguments[index];
            if (!argument.IsExplicit || argument.Value is not LogicalCall entry)
                continue;
            var valueSchema = InferArgument(call, index, input, enclosing, path);
            if (entry.Function.Schema?.Intrinsic != "named-entry")
                continue;
            var name = LiteralText(entry, "name");
            if (name is not null && valueSchema is not null)
                ApplyRecordMutation(fields, source, name, valueSchema, mutation);
        }
        return new RecordLogicalSchema(fields, source.AllowsAdditionalFields, source.IsNullable);
    }

    private static void ApplyRecordMutation(
        IDictionary<string, LogicalSchemaField> fields,
        RecordLogicalSchema source,
        string name,
        LogicalSchema value,
        RecordMutation mutation)
    {
        fields.TryGetValue(name, out var existing);
        switch (mutation)
        {
            case RecordMutation.Always:
                fields[name] = new LogicalSchemaField(value);
                break;
            case RecordMutation.WhenPresent when existing is not null:
                fields[name] = new LogicalSchemaField(
                    existing.Optional ? Union(existing.Schema, value) : value,
                    existing.Optional);
                break;
            case RecordMutation.WhenPresent when source.AllowsAdditionalFields:
                fields[name] = new LogicalSchemaField(
                    Union(new AnyLogicalSchema(), value),
                    Optional: true);
                break;
            case RecordMutation.WhenAbsent when existing is not null:
                fields[name] = existing.Optional
                    ? new LogicalSchemaField(Union(existing.Schema, value))
                    : existing;
                break;
            case RecordMutation.WhenAbsent:
                fields[name] = source.AllowsAdditionalFields
                    ? new LogicalSchemaField(new AnyLogicalSchema())
                    : new LogicalSchemaField(value);
                break;
        }
    }

    private LogicalSchema InferNamedEntry(
        LogicalCall call,
        LogicalSchema input,
        LogicalSchema enclosing,
        string path)
    {
        InferArgument(call, "name", input, enclosing, path);
        return InferArgument(call, ValueParameterName, input, enclosing, path) ?? new AnyLogicalSchema();
    }

    private LogicalSchema InferSpreadEntry(
        LogicalCall call,
        LogicalSchema input,
        LogicalSchema enclosing,
        string path)
        => InferArgument(call, ValueParameterName, input, enclosing, path) ?? new AnyLogicalSchema();

    private LogicalSchema InferSortCriterion(
        LogicalCall call,
        LogicalSchema input,
        LogicalSchema enclosing,
        string path)
    {
        InferArgument(call, SelectorParameterName, input, enclosing, path);
        InferArgument(call, "type", input, enclosing, path);
        InferArgument(call, "ascending", input, enclosing, path);
        InferArgument(call, "nulls-first", input, enclosing, path);
        var type = LiteralText(call, "type");
        return type is null
            ? Dynamic(path, "sort criterion coercion type")
            : WithNullability(FromType(type), true);
    }

    private Requirement RequireTuplePosition(
        LogicalCall call,
        string path)
    {
        var generic = RequireGenericCall(call, path);
        var pair = new PairLogicalSchema(new AnyLogicalSchema(), new AnyLogicalSchema());
        return new Requirement(Union(generic.Input, pair), generic.Enclosing);
    }

    private LogicalSchema InferTuplePosition(
        LogicalCall call,
        LogicalSchema input,
        LogicalSchema enclosing,
        string path,
        string intrinsic)
    {
        InferArgument(call, "position", input, enclosing, path);
        var position = TuplePosition(call, intrinsic);
        return position is null
            ? Dynamic(path, "tuple position selected by a non-literal expression")
            : SelectTuplePosition(input, position.Value, path);
    }

    private LogicalSchema SelectTuplePosition(LogicalSchema input, int position, string path)
    {
        if (input is UnionLogicalSchema union)
        {
            return union.Alternatives
                .Select(item => SelectTuplePosition(item, position, path))
                .Aggregate(Union);
        }
        return input switch
        {
            PairLogicalSchema pair => SelectPairPosition(pair, position),
            TupleLogicalSchema tuple => SelectTuplePosition(tuple, position),
            _ => Dynamic(path, "tuple position selected from an unknown positional shape"),
        };
    }

    private static LogicalSchema SelectPairPosition(PairLogicalSchema pair, int position)
    {
        var selected = position switch
        {
            0 or -2 => pair.Key,
            1 or -1 => pair.Value,
            _ => new AnyLogicalSchema(true),
        };
        return pair.IsNullable ? WithNullability(selected, true) : selected;
    }

    private static LogicalSchema SelectTuplePosition(TupleLogicalSchema tuple, int position)
    {
        var index = position < 0 && tuple.AdditionalItems is null
            ? tuple.Items.Count + position
            : position;
        var selected = SelectTupleItem(tuple, position, index);
        return tuple.IsNullable ? WithNullability(selected, true) : selected;
    }

    private static LogicalSchema SelectTupleItem(TupleLogicalSchema tuple, int position, int index)
    {
        if (index >= 0 && index < tuple.Items.Count)
            return tuple.Items[index];
        if (index >= tuple.Items.Count && tuple.AdditionalItems is not null)
            return WithNullability(tuple.AdditionalItems, true);
        if (position < 0 && tuple.AdditionalItems is not null)
        {
            var selected = tuple.Items
                .Append(tuple.AdditionalItems)
                .Aggregate(Union);
            return WithNullability(selected, true);
        }
        return new AnyLogicalSchema(true);
    }

    private LogicalSchema InferGenericCall(
        LogicalCall call,
        LogicalSchema input,
        LogicalSchema enclosing,
        string path)
    {
        for (var index = 0; index < call.Arguments.Count; index++)
            InferArgument(call, index, input, enclosing, path);
        var result = FromType(call.Function.Output);
        if (result is AnyLogicalSchema)
            return Dynamic(path, $"output of '{call.Function.Name}'");
        return IsNullable(input) ? WithNullability(result, true) : result;
    }

    private LogicalSchema InferDynamicCall(
        LogicalCall call,
        LogicalSchema input,
        LogicalSchema enclosing,
        string path,
        string? reason)
    {
        for (var index = 0; index < call.Arguments.Count; index++)
            InferArgument(call, index, input, enclosing, path);
        if (reason is null)
            return Dynamic(path, $"output of '{call.Function.Name}'");
        diagnostics.Add(new SchemaAnalysisDiagnostic(DynamicDiagnosticCode, path, reason));
        return new AnyLogicalSchema();
    }

    private LogicalSchema? InferArgument(
        LogicalCall call,
        string name,
        LogicalSchema input,
        LogicalSchema enclosing,
        string path)
    {
        var index = call.Arguments
            .Select((argument, index) => (argument, index))
            .FirstOrDefault(item => item.argument.Parameter.Name == name).index;
        return index < call.Arguments.Count && call.Arguments[index].Parameter.Name == name
            ? InferArgument(call, index, input, enclosing, path)
            : null;
    }

    private LogicalSchema? InferArgument(
        LogicalCall call,
        int index,
        LogicalSchema input,
        LogicalSchema enclosing,
        string path)
    {
        var argument = call.Arguments[index];
        if (!argument.IsExplicit || argument.Value is null)
            return null;
        var context = ArgumentContext(argument, input, enclosing);
        return Infer(argument.Value, context, context, $"{path}.arguments[{index}].value");
    }

    private static LogicalSchema ArgumentContext(
        LogicalArgument argument,
        LogicalSchema input,
        LogicalSchema enclosing)
    {
        if (argument.Parameter.Evaluation?.Context == "traversal")
            return input is ArrayLogicalSchema array ? array.Items : new AnyLogicalSchema();
        return argument.Parameter.Evaluation?.Source switch
        {
            "enclosing" or "surrounding" => enclosing,
            "incoming" => input,
            _ => enclosing,
        };
    }

    private void Capture(LogicalValue value, string path, LogicalSchema input, LogicalSchema output)
        => trace.Capture(value, path, input, output);

    private static LogicalSchema FromType(string type)
    {
        var normalized = Normalize(type);
        if (normalized.Contains('|', StringComparison.Ordinal))
        {
            var members = normalized.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (members.All(member => member is "integer" or "decimal" or "numeric"))
                return new ScalarLogicalSchema("numeric");
            if (members.All(member => member is "date" or "datetime" or "time" or "temporal"))
                return new ScalarLogicalSchema("temporal");
            return new AnyLogicalSchema();
        }
        return normalized switch
        {
            "any" or "expression" or "predicate" or "accumulator" or "entry" => new AnyLogicalSchema(),
            ArraySchemaName => new ArrayLogicalSchema(new AnyLogicalSchema()),
            "record" => new RecordLogicalSchema(
                new SortedDictionary<string, LogicalSchemaField>(StringComparer.Ordinal)),
            TupleSchemaName => new TupleLogicalSchema([], AdditionalItems: new AnyLogicalSchema()),
            "pair" => new PairLogicalSchema(new AnyLogicalSchema(), new AnyLogicalSchema()),
            DictionarySchemaName => new DictionaryLogicalSchema(new AnyLogicalSchema(), new AnyLogicalSchema()),
            GroupingSchemaName => new GroupingLogicalSchema(new AnyLogicalSchema(), new AnyLogicalSchema()),
            SortTableSchemaName => new SortTableLogicalSchema(new AnyLogicalSchema()),
            "null" => new AnyLogicalSchema(true),
            _ => new ScalarLogicalSchema(normalized),
        };
    }

    private static LogicalArgument? Argument(LogicalCall call, string name)
        => call.Arguments.FirstOrDefault(argument => argument.Parameter.Name == name);

    private static string? LiteralText(LogicalCall call, string parameter)
    {
        var value = Argument(call, parameter)?.Value
            ?? call.Arguments.FirstOrDefault()?.Value;
        return value is LogicalLiteral { Value: string text } ? text : null;
    }

    private static int? TuplePosition(LogicalCall call, string intrinsic)
    {
        const string prefix = "tuple-position:";
        if (intrinsic.StartsWith(prefix, StringComparison.Ordinal)
            && int.TryParse(intrinsic[prefix.Length..], out var configured))
        {
            return configured;
        }

        var value = Argument(call, "position")?.Value;
        return value switch
        {
            LogicalLiteral { Value: int position } => position,
            LogicalLiteral { Value: long position } when position is >= int.MinValue and <= int.MaxValue
                => (int)position,
            LogicalLiteral { Value: decimal position }
                when position == decimal.Truncate(position)
                    && position is >= int.MinValue and <= int.MaxValue
                => (int)position,
            _ => null,
        };
    }

    private static string? SelectedField(LogicalCall call, string parameter)
    {
        var value = Argument(call, parameter)?.Value;
        var selector = value switch
        {
            LogicalCall direct => direct,
            LogicalPipeline { Items.Count: 1 } pipeline => pipeline.Items[0] as LogicalCall,
            _ => null,
        };
        return selector?.Function.Schema?.Intrinsic == "field"
            ? LiteralText(selector, "name")
            : null;
    }

    private static string[]? LiteralTexts(LogicalValue? value) => value switch
    {
        LogicalPipeline { Items.Count: 1 } pipeline => LiteralTexts(pipeline.Items[0]),
        LogicalCall { Function.Schema.Intrinsic: ArraySchemaName } array
            when array.Arguments.Where(argument => argument.IsExplicit)
                .All(argument => argument.Value is LogicalLiteral { Value: string })
            => array.Arguments.Where(argument => argument.IsExplicit)
                .Select(argument => argument.Value)
                .OfType<LogicalLiteral>()
                .Select(literal => literal.Value)
                .OfType<string>()
                .ToArray(),
        _ => null,
    };

    private Requirement DynamicRequirement(string path, string description)
    {
        Dynamic(path, description);
        return new Requirement(new AnyLogicalSchema(), new AnyLogicalSchema());
    }

    private LogicalSchema Dynamic(string path, string description)
    {
        diagnostics.Add(new SchemaAnalysisDiagnostic(
            DynamicDiagnosticCode,
            path,
            $"The schema of {description} cannot be determined statically."));
        return new AnyLogicalSchema();
    }

    private static LogicalSchema Union(LogicalSchema left, LogicalSchema right)
        => SchemaAlgebra.Union(left, right);

    private static string Normalize(string type) => SchemaAlgebra.Normalize(type);
    private static LogicalSchema WithNullability(LogicalSchema schema, bool nullable)
        => SchemaAlgebra.WithNullability(schema, nullable);
    private static bool IsNullable(LogicalSchema schema) => SchemaAlgebra.IsNullable(schema);
    private static bool ContainsAny(LogicalSchema schema) => SchemaAlgebra.ContainsAny(schema);
    private static bool ContainsConflict(LogicalSchema schema) => SchemaAlgebra.ContainsConflict(schema);
    private static string Describe(LogicalSchema schema) => SchemaAlgebra.Describe(schema);

    internal sealed record Requirement(LogicalSchema Input, LogicalSchema Enclosing);

    private enum RecordMutation
    {
        Always,
        WhenPresent,
        WhenAbsent,
    }
}
