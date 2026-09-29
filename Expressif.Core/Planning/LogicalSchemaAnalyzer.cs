namespace Expressif.Planning;

/// <summary>
/// Discovers input requirements and output schema from a portable logical plan without executing it.
/// </summary>
public static class LogicalSchemaAnalyzer
{
    public static SchemaAnalysis Analyze(LogicalPlan plan, LogicalSchema? declaredInput = null)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var analyzer = new Analyzer();
        var requirement = analyzer.RequirePipeline(plan.Pipeline, new AnyLogicalSchema(), "plan");
        var input = declaredInput is null
            ? requirement
            : analyzer.Intersect(declaredInput, requirement, "plan.input");
        var output = analyzer.InferPipeline(plan.Pipeline, input, input, "plan");
        var completeness = analyzer.Completeness(input, output);
        return new SchemaAnalysis(input, output, completeness, analyzer.Diagnostics, analyzer.Nodes);
    }

    private sealed class Analyzer
    {
        private readonly List<SchemaAnalysisDiagnostic> diagnostics = [];
        private readonly Dictionary<string, SchemaAnalysisNode> nodes = new(StringComparer.Ordinal);

        public IReadOnlyList<SchemaAnalysisDiagnostic> Diagnostics => diagnostics;

        public IReadOnlyList<SchemaAnalysisNode> Nodes
            => nodes.Values.OrderBy(node => node.Path, StringComparer.Ordinal).ToArray();

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

            var conflict = new ConflictingLogicalSchema(left, right, IsNullable(left) || IsNullable(right));
            diagnostics.Add(new SchemaAnalysisDiagnostic(
                "schema.conflict",
                path,
                $"Schema constraints '{Describe(left)}' and '{Describe(right)}' are incompatible."));
            return conflict;
        }

        public SchemaAnalysisCompleteness Completeness(LogicalSchema input, LogicalSchema output)
        {
            if (ContainsConflict(input) || ContainsConflict(output))
                return SchemaAnalysisCompleteness.Conflicting;
            if (diagnostics.Any(diagnostic => diagnostic.Code == "schema.dynamic"))
                return SchemaAnalysisCompleteness.Dynamic;
            return ContainsAny(input) || ContainsAny(output)
                ? SchemaAnalysisCompleteness.Partial
                : SchemaAnalysisCompleteness.Known;
        }

        private TupleLogicalSchema? IntersectTuple(
            TupleLogicalSchema left,
            TupleLogicalSchema right,
            string path)
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
            return new TupleLogicalSchema(
                items,
                IsNullable(left) || IsNullable(right),
                additionalItems);
        }

        private Requirement Require(LogicalValue value, LogicalSchema expected, string path) => value switch
        {
            LogicalLiteral => new Requirement(new NoInputLogicalSchema(), new NoInputLogicalSchema()),
            LogicalPipeline pipeline => new Requirement(RequirePipeline(pipeline, expected, path), new AnyLogicalSchema()),
            LogicalCall call => RequireCall(call, expected, path),
            _ => DynamicRequirement(path, value.GetType().Name),
        };

        private Requirement RequireCall(LogicalCall call, LogicalSchema expected, string path)
        {
            var schema = call.Function.Schema;
            if (schema?.Classification == "intrinsic" && schema.Intrinsic is not null)
                return RequireIntrinsic(call, expected, path, schema.Intrinsic);
            return schema?.Classification == "contract"
                ? RequireContract(call, expected, path, schema)
                : RequireGenericCall(call, path);
        }

        private Requirement RequireIntrinsic(
            LogicalCall call,
            LogicalSchema expected,
            string path,
            string intrinsic) => intrinsic switch
            {
                "field" => RequireField(call, expected, path),
                "select-fields" => RequireSelectFields(call, expected, path),
                "explode-field" or "explode-field-outer" => RequireExplode(call, expected, path),
                "with" => RequireWith(call, expected, path),
                "spread-entry" => RequireSpreadEntry(call, expected, path),
                "sort-criterion" => RequireSortCriterion(call, path),
                "array" or "tuple" or "record" or "dictionary" or "grouping"
                    or "put" or "put-present" or "put-absent" or "named-entry"
                    => RequireGenericCall(call, path),
                _ => throw new InvalidOperationException(
                    $"Unsupported schema intrinsic '{intrinsic}' for '{call.Function.Name}'."),
            };

        private Requirement RequireContract(
            LogicalCall call,
            LogicalSchema expected,
            string path,
            PlannerSchemaDescriptor contract)
        {
            var bindings = new Dictionary<string, LogicalSchema>(StringComparer.Ordinal);
            if (contract.Output is not null)
                Bind(ParseSchema(contract.Output), expected, bindings, $"{path}.output");

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
                    Bind(
                        ParseSchema(parameterContract.Input),
                        parameterInput,
                        bindings,
                        $"{path}.parameters.{argument.Parameter.Name}");
                }
            }

            var input = contract.Input is null
                ? FromType(call.Function.Input)
                : Resolve(ParseSchema(contract.Input), bindings);
            foreach (var (argument, requirement) in argumentRequirements)
            {
                PlannerParameterSchemaDescriptor? parameterContract = null;
                contract.Parameters?.TryGetValue(argument.Parameter.Name, out parameterContract);
                if (parameterContract?.Input is not null)
                    continue;
                var evaluation = argument.Parameter.Evaluation;
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
                foreach (var field in record.Fields)
                {
                    if (names is null || names.Contains(field.Key, StringComparer.Ordinal))
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
            var name = SelectedField(call, "selector");
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

        private Requirement RequireWith(LogicalCall call, LogicalSchema expected, string path)
        {
            var bodyIndex = call.Arguments
                .Select((argument, index) => (argument, index))
                .Single(item => item.argument.Parameter.Name == "body").index;
            var body = call.Arguments[bodyIndex].Value!;
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
                var projection = Argument(entry, "value")?.Value;
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
            var value = Argument(call, "value")?.Value;
            return value is null
                ? DynamicRequirement(path, "record spread")
                : Require(value, expected, $"{path}.arguments[0].value");
        }

        private Requirement RequireSortCriterion(LogicalCall call, string path)
        {
            var selector = Argument(call, "selector")?.Value;
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
                var evaluation = argument.Parameter.Evaluation;
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
                _ => Dynamic(path, value.GetType().Name),
            };
            Capture(value, path, input, output);
            return output;
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
            string intrinsic) => intrinsic switch
            {
                "field" => InferField(call, input, enclosing, path),
                "select-fields" => InferSelectFields(call, input, enclosing, path),
                "explode-field" => InferExplode(call, input, path, preserveParent: false),
                "explode-field-outer" => InferExplode(call, input, path, preserveParent: true),
                "with" => InferWith(call, input, enclosing, path),
                "array" => InferArray(call, input, enclosing, path),
                "tuple" => InferTuple(call, input, enclosing, path),
                "record" => InferRecord(call, input, enclosing, path),
                "dictionary" => InferAssociative(call, input, enclosing, path, grouping: false),
                "grouping" => InferAssociative(call, input, enclosing, path, grouping: true),
                "put" => InferPut(call, input, enclosing, path, RecordMutation.Always),
                "put-present" => InferPut(call, input, enclosing, path, RecordMutation.WhenPresent),
                "put-absent" => InferPut(call, input, enclosing, path, RecordMutation.WhenAbsent),
                "named-entry" => InferNamedEntry(call, input, enclosing, path),
                "spread-entry" => InferSpreadEntry(call, input, enclosing, path),
                "sort-criterion" => InferSortCriterion(call, input, enclosing, path),
                _ => throw new InvalidOperationException(
                    $"Unsupported schema intrinsic '{intrinsic}' for '{call.Function.Name}'."),
            };

        private LogicalSchema InferContract(
            LogicalCall call,
            LogicalSchema input,
            LogicalSchema enclosing,
            string path,
            PlannerSchemaDescriptor contract)
        {
            var bindings = new Dictionary<string, LogicalSchema>(StringComparer.Ordinal);
            if (contract.Input is not null)
                Bind(ParseSchema(contract.Input), input, bindings, $"{path}.input");
            var parameterOutputs = new Dictionary<
                string,
                (PlannerParameterSchemaDescriptor Contract, List<LogicalSchema> Outputs)>(StringComparer.Ordinal);
            var parameterResults = new Dictionary<string, List<LogicalSchema>>(StringComparer.Ordinal);
            for (var index = 0; index < call.Arguments.Count; index++)
            {
                var argument = call.Arguments[index];
                if (!argument.IsExplicit || argument.Value is null)
                    continue;
                PlannerParameterSchemaDescriptor? parameterContract = null;
                contract.Parameters?.TryGetValue(argument.Parameter.Name, out parameterContract);
                var context = parameterContract?.Input is null
                    ? ArgumentContext(argument, input, enclosing)
                    : Resolve(ParseSchema(parameterContract.Input), bindings);
                if (parameterContract?.Input is not null)
                    Bind(ParseSchema(parameterContract.Input), context, bindings, $"{path}.parameters.{argument.Parameter.Name}.input");
                var result = Infer(argument.Value, context, context, $"{path}.arguments[{index}].value");
                if (!parameterResults.TryGetValue(argument.Parameter.Name, out var results))
                {
                    results = [];
                    parameterResults.Add(argument.Parameter.Name, results);
                }
                results.Add(result);
                if (parameterContract?.Output is not null)
                {
                    if (!parameterOutputs.TryGetValue(argument.Parameter.Name, out var collected))
                    {
                        collected = (parameterContract, []);
                        parameterOutputs.Add(argument.Parameter.Name, collected);
                    }
                    collected.Outputs.Add(result);
                }
            }
            foreach (var parameter in parameterOutputs)
            {
                Bind(
                    ParseSchema(parameter.Value.Contract.Output!),
                    CombineParameterOutputs(parameter.Value.Outputs, parameter.Value.Contract.Combine),
                    bindings,
                    $"{path}.parameters.{parameter.Key}.output");
            }
            if (contract.Input is not null && contract.Output == contract.Input
                && ParseSchema(contract.Input).ContainsVariable)
                return input;
            var output = contract.Output is null
                ? FromType(call.Function.Output)
                : Resolve(ParseSchema(contract.Output), bindings);
            return IsConditionallyNullable(contract, input, parameterResults)
                ? WithNullability(output, true)
                : output;
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
            var expected = Resolve(ParseSchema(contract.Output), bindings);
            return contract.Combine == "tuple" && count > 1
                && expected is TupleLogicalSchema tuple && tuple.Items.Count == count
                    ? tuple.Items[ordinal]
                    : expected;
        }

        private LogicalSchema CombineParameterOutputs(
            IReadOnlyList<LogicalSchema> outputs,
            string? combination) => combination switch
            {
                "union" => outputs.Aggregate(Union),
                "tuple" when outputs.Count > 1 => new TupleLogicalSchema(outputs),
                "tuple" => outputs.Single(),
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
            var name = SelectedField(call, "selector");
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

            var selector = Argument(call, "selector");
            if (selector?.Value is not null)
            {
                var selectorIndex = call.Arguments
                    .Select((argument, index) => (argument, index))
                    .Single(item => item.argument.Parameter.Name == "selector")
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
                if (!argument.IsSpread)
                {
                    additionalItems = Union(additionalItems!, value);
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
                            .Aggregate(additionalItems!, Union);
                        break;
                    case ArrayLogicalSchema array:
                        additionalItems = additionalItems is null
                            ? array.Items
                            : Union(additionalItems, array.Items);
                        break;
                    default:
                        additionalItems = new AnyLogicalSchema();
                        diagnostics.Add(new SchemaAnalysisDiagnostic(
                            "schema.dynamic",
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
        {
            var fields = new SortedDictionary<string, LogicalSchemaField>(StringComparer.Ordinal);
            var dynamic = false;
            for (var index = 0; index < call.Arguments.Count; index++)
            {
                var argument = call.Arguments[index];
                if (!argument.IsExplicit || argument.Value is null)
                    continue;
                var valueSchema = InferArgument(call, index, input, enclosing, path);
                if (!argument.IsSpread
                    && argument.Value is LogicalCall entry
                    && entry.Function.Schema?.Intrinsic == "named-entry")
                {
                    var name = LiteralText(entry, "name");
                    if (name is not null && valueSchema is not null)
                        fields[name] = new LogicalSchemaField(valueSchema);
                }
                else if (!argument.IsSpread
                    && argument.Value is LogicalCall nested
                    && nested.Function.Schema?.Intrinsic == "record"
                    && valueSchema is RecordLogicalSchema nestedRecord)
                {
                    MergeRecordFields(fields, nestedRecord);
                    dynamic |= nestedRecord.AllowsAdditionalFields;
                }
                else if (argument.IsSpread && valueSchema is RecordLogicalSchema spreadRecord)
                {
                    MergeRecordFields(fields, spreadRecord);
                    dynamic |= spreadRecord.AllowsAdditionalFields;
                }
                else
                {
                    dynamic = true;
                }
            }
            if (dynamic)
                diagnostics.Add(new SchemaAnalysisDiagnostic("schema.dynamic", path, "Record spread has a dynamic shape."));
            return new RecordLogicalSchema(fields, AllowsAdditionalFields: dynamic);
        }

        private void MergeRecordFields(
            IDictionary<string, LogicalSchemaField> fields,
            RecordLogicalSchema source)
        {
            foreach (var field in source.Fields)
            {
                if (field.Value.Optional && fields.TryGetValue(field.Key, out var existing))
                {
                    fields[field.Key] = new LogicalSchemaField(
                        Union(existing.Schema, field.Value.Schema),
                        existing.Optional);
                }
                else
                {
                    fields[field.Key] = field.Value;
                }
            }
        }

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
            return Infer(
                body.argument.Value!,
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

        private void ApplyRecordMutation(
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
            return InferArgument(call, "value", input, enclosing, path) ?? new AnyLogicalSchema();
        }

        private LogicalSchema InferSpreadEntry(
            LogicalCall call,
            LogicalSchema input,
            LogicalSchema enclosing,
            string path)
            => InferArgument(call, "value", input, enclosing, path) ?? new AnyLogicalSchema();

        private LogicalSchema InferSortCriterion(
            LogicalCall call,
            LogicalSchema input,
            LogicalSchema enclosing,
            string path)
        {
            InferArgument(call, "selector", input, enclosing, path);
            InferArgument(call, "type", input, enclosing, path);
            InferArgument(call, "ascending", input, enclosing, path);
            InferArgument(call, "nulls-first", input, enclosing, path);
            var type = LiteralText(call, "type");
            return type is null
                ? Dynamic(path, "sort criterion coercion type")
                : WithNullability(FromType(type), true);
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
            diagnostics.Add(new SchemaAnalysisDiagnostic("schema.dynamic", path, reason));
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
        {
            var (kind, operation) = value switch
            {
                LogicalPipeline => ("pipeline", null),
                LogicalCall call => ("call", call.Function.Name),
                LogicalLiteral => ("literal", null),
                _ => (value.GetType().Name, null),
            };
            nodes[path] = new SchemaAnalysisNode(path, kind, operation, input, output);
        }

        private void Bind(
            SchemaExpression expression,
            LogicalSchema actual,
            IDictionary<string, LogicalSchema> bindings,
            string path)
        {
            if (actual is NoInputLogicalSchema or AnyLogicalSchema)
                return;
            if (expression.IsVariable)
            {
                bindings[expression.Name] = bindings.TryGetValue(expression.Name, out var existing)
                    ? Intersect(existing, actual, path)
                    : actual;
                return;
            }
            if (expression.Name == "nullable")
            {
                Bind(expression.Arguments.Single(), actual, bindings, path);
                return;
            }
            if (actual is UnionLogicalSchema union)
            {
                var alternatives = union.Alternatives
                    .Select((schema, index) => (schema, index))
                    .Where(item => AcceptsRoot(expression, item.schema))
                    .ToArray();
                if (alternatives.Length == 1)
                {
                    Bind(
                        expression,
                        alternatives[0].schema,
                        bindings,
                        $"{path}.alternatives[{alternatives[0].index}]");
                    return;
                }

                var alternativesBindings = alternatives.Select(item =>
                {
                    var alternativeBindings = new Dictionary<string, LogicalSchema>(StringComparer.Ordinal);
                    Bind(
                        expression,
                        item.schema,
                        alternativeBindings,
                        $"{path}.alternatives[{item.index}]");
                    return alternativeBindings;
                }).ToArray();
                foreach (var name in alternativesBindings.SelectMany(item => item.Keys).Distinct(StringComparer.Ordinal))
                {
                    var value = alternativesBindings
                        .Where(item => item.ContainsKey(name))
                        .Select(item => item[name])
                        .Aggregate(Union);
                    bindings[name] = bindings.TryGetValue(name, out var existing)
                        ? Intersect(existing, value, path)
                        : value;
                }
                return;
            }
            if (expression.Name == "array" && actual is ArrayLogicalSchema array)
            {
                Bind(expression.Arguments.Single(), array.Items, bindings, $"{path}.items");
                return;
            }
            if (expression.Name == "array" && actual is DictionaryLogicalSchema dictionaryCollection)
            {
                Bind(
                    expression.Arguments.Single(),
                    new PairLogicalSchema(dictionaryCollection.Keys, dictionaryCollection.Values),
                    bindings,
                    $"{path}.items");
                return;
            }
            if (expression.Name == "array" && actual is GroupingLogicalSchema groupingCollection)
            {
                Bind(
                    expression.Arguments.Single(),
                    new PairLogicalSchema(
                        groupingCollection.Keys,
                        new ArrayLogicalSchema(groupingCollection.Items)),
                    bindings,
                    $"{path}.items");
                return;
            }
            if (expression.Name == "tuple" && actual is TupleLogicalSchema tuple
                && expression.Arguments.Count == tuple.Items.Count)
            {
                for (var index = 0; index < tuple.Items.Count; index++)
                    Bind(expression.Arguments[index], tuple.Items[index], bindings, $"{path}.items[{index}]");
            }
            if (expression.Name == "variadic-tuple" && actual is TupleLogicalSchema variadicTuple)
            {
                var item = variadicTuple.Items
                    .Concat(variadicTuple.AdditionalItems is null ? [] : [variadicTuple.AdditionalItems])
                    .DefaultIfEmpty(new AnyLogicalSchema())
                    .Aggregate(Union);
                Bind(expression.Arguments.Single(), item, bindings, $"{path}.items");
            }
            if (expression.Name == "pair" && actual is PairLogicalSchema pair)
            {
                Bind(expression.Arguments[0], pair.Key, bindings, $"{path}.key");
                Bind(expression.Arguments[1], pair.Value, bindings, $"{path}.value");
            }
            if (expression.Name == "dictionary" && actual is DictionaryLogicalSchema dictionary)
            {
                Bind(expression.Arguments[0], dictionary.Keys, bindings, $"{path}.keys");
                Bind(expression.Arguments[1], dictionary.Values, bindings, $"{path}.values");
            }
            if (expression.Name == "grouping" && actual is GroupingLogicalSchema grouping)
            {
                Bind(expression.Arguments[0], grouping.Keys, bindings, $"{path}.keys");
                Bind(expression.Arguments[1], grouping.Items, bindings, $"{path}.items");
            }
        }

        private static bool AcceptsRoot(SchemaExpression expression, LogicalSchema actual)
        {
            if (expression.IsVariable)
                return true;
            if (expression.Name == "nullable")
                return AcceptsRoot(expression.Arguments.Single(), actual);
            return (expression.Name, actual) switch
            {
                ("array", ArrayLogicalSchema or DictionaryLogicalSchema or GroupingLogicalSchema) => true,
                ("tuple" or "variadic-tuple", TupleLogicalSchema) => true,
                ("pair", PairLogicalSchema) => true,
                ("dictionary", DictionaryLogicalSchema) => true,
                ("grouping", GroupingLogicalSchema) => true,
                _ => expression.Arguments.Count == 0 && actual is ScalarLogicalSchema scalar
                    && IntersectScalar(expression.Name, scalar.Type) is not null,
            };
        }

        private LogicalSchema Resolve(
            SchemaExpression expression,
            IReadOnlyDictionary<string, LogicalSchema> bindings)
        {
            if (expression.IsVariable)
                return bindings.TryGetValue(expression.Name, out var value) ? value : new AnyLogicalSchema();
            return expression.Name switch
            {
                "nullable" => WithNullability(Resolve(expression.Arguments.Single(), bindings), true),
                "array" => new ArrayLogicalSchema(Resolve(expression.Arguments.Single(), bindings)),
                "tuple" => new TupleLogicalSchema(expression.Arguments.Select(item => Resolve(item, bindings)).ToArray()),
                "variadic-tuple" => new TupleLogicalSchema(
                    [],
                    AdditionalItems: Resolve(expression.Arguments.Single(), bindings)),
                "pair" => new PairLogicalSchema(
                    Resolve(expression.Arguments[0], bindings),
                    Resolve(expression.Arguments[1], bindings)),
                "dictionary" => new DictionaryLogicalSchema(
                    Resolve(expression.Arguments[0], bindings),
                    Resolve(expression.Arguments[1], bindings)),
                "grouping" => new GroupingLogicalSchema(
                    Resolve(expression.Arguments[0], bindings),
                    Resolve(expression.Arguments[1], bindings)),
                _ when expression.Arguments.Count == 0 => FromType(expression.Name),
                _ => throw new InvalidOperationException($"Unsupported schema constructor '{expression.Name}'."),
            };
        }

        private static SchemaExpression ParseSchema(string text)
        {
            var index = 0;
            var expression = ParseSchema(text, ref index);
            SkipWhitespace(text, ref index);
            if (index != text.Length)
                throw new InvalidOperationException($"Invalid schema expression '{text}' at position {index}.");
            return expression;
        }

        private static SchemaExpression ParseSchema(string text, ref int index)
        {
            SkipWhitespace(text, ref index);
            var start = index;
            while (index < text.Length && (char.IsLetterOrDigit(text[index]) || text[index] is '-' or '_'))
                index++;
            if (start == index)
                throw new InvalidOperationException($"Invalid schema expression '{text}' at position {index}.");
            var name = text[start..index];
            SkipWhitespace(text, ref index);
            if (index >= text.Length || text[index] != '<')
                return new SchemaExpression(name, []);
            index++;
            var arguments = new List<SchemaExpression>();
            while (true)
            {
                arguments.Add(ParseSchema(text, ref index));
                SkipWhitespace(text, ref index);
                if (index < text.Length && text[index] == ',')
                {
                    index++;
                    continue;
                }
                if (index >= text.Length || text[index] != '>')
                    throw new InvalidOperationException($"Invalid schema expression '{text}' at position {index}.");
                index++;
                return new SchemaExpression(name, arguments);
            }
        }

        private static void SkipWhitespace(string text, ref int index)
        {
            while (index < text.Length && char.IsWhiteSpace(text[index]))
                index++;
        }

        private LogicalSchema FromType(string type)
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
                "array" => new ArrayLogicalSchema(new AnyLogicalSchema()),
                "record" => new RecordLogicalSchema(
                    new SortedDictionary<string, LogicalSchemaField>(StringComparer.Ordinal)),
                "tuple" => new TupleLogicalSchema([], AdditionalItems: new AnyLogicalSchema()),
                "pair" => new PairLogicalSchema(new AnyLogicalSchema(), new AnyLogicalSchema()),
                "dictionary" => new DictionaryLogicalSchema(new AnyLogicalSchema(), new AnyLogicalSchema()),
                "grouping" => new GroupingLogicalSchema(new AnyLogicalSchema(), new AnyLogicalSchema()),
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
            LogicalCall { Function.Schema.Intrinsic: "array" } array
                when array.Arguments.Where(argument => argument.IsExplicit)
                    .All(argument => argument.Value is LogicalLiteral { Value: string })
                => array.Arguments.Where(argument => argument.IsExplicit)
                    .Select(argument => (string)((LogicalLiteral)argument.Value!).Value!)
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
                "schema.dynamic",
                path,
                $"The schema of {description} cannot be determined statically."));
            return new AnyLogicalSchema();
        }

        private LogicalSchema Union(LogicalSchema left, LogicalSchema right)
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
                    return new ScalarLogicalSchema("numeric", IsNullable(left) || IsNullable(right));
                if (IsTemporal(leftScalar.Type) && IsTemporal(rightScalar.Type))
                    return new ScalarLogicalSchema("temporal", IsNullable(left) || IsNullable(right));
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

        private static IEnumerable<LogicalSchema> FlattenUnion(LogicalSchema schema)
            => schema is UnionLogicalSchema union ? union.Alternatives : [schema];

        private static string? IntersectScalar(string left, string right)
        {
            left = Normalize(left);
            right = Normalize(right);
            if (left == right)
                return left;
            if (left == "scalar")
                return right;
            if (right == "scalar")
                return left;
            if (left == "numeric" && IsNumeric(right))
                return right;
            if (right == "numeric" && IsNumeric(left))
                return left;
            if (left == "temporal" && IsTemporal(right))
                return right;
            if (right == "temporal" && IsTemporal(left))
                return left;
            return null;
        }

        private static bool IsNumeric(string type) => Normalize(type) is "integer" or "decimal" or "numeric";

        private static bool IsTemporal(string type) => Normalize(type) is "date" or "datetime" or "time" or "temporal";

        private static string Normalize(string type)
            => type.Trim().ToLowerInvariant() switch
            {
                "date-time" => "datetime",
                var value => value,
            };

        private static LogicalSchema WithNullability(LogicalSchema schema, bool nullable) => schema switch
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
            UnionLogicalSchema value => value with { IsNullable = nullable },
            ConflictingLogicalSchema value => value with { IsNullable = nullable },
            _ => schema,
        };

        private static bool IsNullable(LogicalSchema schema) => schema switch
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
            UnionLogicalSchema value => value.IsNullable,
            ConflictingLogicalSchema value => value.IsNullable,
            _ => false,
        };

        private static bool ContainsAny(LogicalSchema schema) => schema switch
        {
            AnyLogicalSchema => true,
            RecordLogicalSchema record => record.Fields.Values.Any(field => ContainsAny(field.Schema)),
            ArrayLogicalSchema array => ContainsAny(array.Items),
            TupleLogicalSchema tuple => tuple.Items.Any(ContainsAny)
                || (tuple.AdditionalItems is not null && ContainsAny(tuple.AdditionalItems)),
            PairLogicalSchema pair => ContainsAny(pair.Key) || ContainsAny(pair.Value),
            DictionaryLogicalSchema dictionary => ContainsAny(dictionary.Keys) || ContainsAny(dictionary.Values),
            GroupingLogicalSchema grouping => ContainsAny(grouping.Keys) || ContainsAny(grouping.Items),
            UnionLogicalSchema union => union.Alternatives.Any(ContainsAny),
            ConflictingLogicalSchema conflict => ContainsAny(conflict.Left) || ContainsAny(conflict.Right),
            _ => false,
        };

        private static bool ContainsConflict(LogicalSchema schema) => schema switch
        {
            ConflictingLogicalSchema => true,
            RecordLogicalSchema record => record.Fields.Values.Any(field => ContainsConflict(field.Schema)),
            ArrayLogicalSchema array => ContainsConflict(array.Items),
            TupleLogicalSchema tuple => tuple.Items.Any(ContainsConflict)
                || (tuple.AdditionalItems is not null && ContainsConflict(tuple.AdditionalItems)),
            PairLogicalSchema pair => ContainsConflict(pair.Key) || ContainsConflict(pair.Value),
            DictionaryLogicalSchema dictionary => ContainsConflict(dictionary.Keys) || ContainsConflict(dictionary.Values),
            GroupingLogicalSchema grouping => ContainsConflict(grouping.Keys) || ContainsConflict(grouping.Items),
            UnionLogicalSchema union => union.Alternatives.Any(ContainsConflict),
            _ => false,
        };

        private static string Describe(LogicalSchema schema) => schema switch
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
            UnionLogicalSchema => "union",
            ConflictingLogicalSchema => "conflict",
            _ => schema.GetType().Name,
        };

        private sealed record Requirement(LogicalSchema Input, LogicalSchema Enclosing);

        private enum RecordMutation
        {
            Always,
            WhenPresent,
            WhenAbsent,
        }

        private sealed record SchemaExpression(string Name, IReadOnlyList<SchemaExpression> Arguments)
        {
            public bool IsVariable => Arguments.Count == 0 && Name.Length > 0 && char.IsUpper(Name[0]);

            public bool ContainsVariable => IsVariable || Arguments.Any(argument => argument.ContainsVariable);
        }
    }
}
