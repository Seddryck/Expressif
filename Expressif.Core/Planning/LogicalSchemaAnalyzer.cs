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
        private readonly IReadOnlyDictionary<string, Func<LogicalCall, LogicalSchema, string, Requirement>> requirementRules;
        private readonly IReadOnlyDictionary<string, Func<LogicalCall, LogicalSchema, LogicalSchema, string, LogicalSchema>> inferenceRules;

        public Analyzer()
        {
            requirementRules = new Dictionary<string, Func<LogicalCall, LogicalSchema, string, Requirement>>(
                StringComparer.Ordinal)
            {
                ["field"] = RequireField,
                ["filter"] = RequireFilter,
                ["map"] = RequireMap,
                ["flat-map"] = RequireFlatMap,
            };
            inferenceRules = new Dictionary<string, Func<LogicalCall, LogicalSchema, LogicalSchema, string, LogicalSchema>>(
                StringComparer.Ordinal)
            {
                ["field"] = InferField,
                ["filter"] = InferFilter,
                ["map"] = InferMap,
                ["flat-map"] = InferFlatMap,
                ["array"] = InferArray,
                ["tuple"] = InferTuple,
                ["record"] = InferRecord,
                ["put"] = InferPut,
                ["put-present"] = InferPut,
                ["put-absent"] = InferPut,
                ["named-entry"] = InferNamedEntry,
            };
        }

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
            if (left is TupleLogicalSchema leftTuple
                && right is TupleLogicalSchema rightTuple
                && leftTuple.Items.Count == rightTuple.Items.Count)
            {
                return new TupleLogicalSchema(
                    leftTuple.Items.Select((item, index) =>
                        Intersect(item, rightTuple.Items[index], $"{path}.items[{index}]")).ToArray(),
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

        private Requirement Require(LogicalValue value, LogicalSchema expected, string path) => value switch
        {
            LogicalLiteral => new Requirement(new NoInputLogicalSchema(), new NoInputLogicalSchema()),
            LogicalPipeline pipeline => new Requirement(RequirePipeline(pipeline, expected, path), new AnyLogicalSchema()),
            LogicalCall call => RequireCall(call, expected, path),
            _ => DynamicRequirement(path, value.GetType().Name),
        };

        private Requirement RequireCall(LogicalCall call, LogicalSchema expected, string path)
        {
            return requirementRules.TryGetValue(call.Function.Name, out var rule)
                ? rule(call, expected, path)
                : RequireGenericCall(call, path);
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

        private Requirement RequireFilter(LogicalCall call, LogicalSchema expected, string path)
        {
            var downstreamItem = expected is ArrayLogicalSchema array ? array.Items : new AnyLogicalSchema();
            var predicate = Argument(call, "predicate")?.Value;
            var predicateRequirement = predicate is null
                ? new AnyLogicalSchema()
                : Require(predicate, new ScalarLogicalSchema("boolean"), $"{path}.predicate").Input;
            return new Requirement(
                new ArrayLogicalSchema(Intersect(downstreamItem, predicateRequirement, $"{path}.items")),
                new AnyLogicalSchema());
        }

        private Requirement RequireMap(LogicalCall call, LogicalSchema expected, string path)
        {
            var expectedItem = expected is ArrayLogicalSchema array ? array.Items : new AnyLogicalSchema();
            var transformation = Argument(call, "transformation")?.Value;
            var itemRequirement = transformation is null
                ? new AnyLogicalSchema()
                : Require(transformation, expectedItem, $"{path}.transformation").Input;
            return new Requirement(new ArrayLogicalSchema(itemRequirement), new AnyLogicalSchema());
        }

        private Requirement RequireFlatMap(LogicalCall call, LogicalSchema expected, string path)
        {
            var expectedItems = expected is ArrayLogicalSchema array ? array.Items : new AnyLogicalSchema();
            var transformation = Argument(call, "expression")?.Value;
            var itemRequirement = transformation is null
                ? new AnyLogicalSchema()
                : Require(transformation, new ArrayLogicalSchema(expectedItems), $"{path}.expression").Input;
            return new Requirement(new ArrayLogicalSchema(itemRequirement), new AnyLogicalSchema());
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
            return inferenceRules.TryGetValue(call.Function.Name, out var rule)
                ? rule(call, input, enclosing, path)
                : InferGenericCall(call, input, enclosing, path);
        }

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

        private LogicalSchema InferFilter(
            LogicalCall call,
            LogicalSchema input,
            LogicalSchema enclosing,
            string path)
        {
            InferArgument(call, "predicate", input, enclosing, path);
            return input;
        }

        private LogicalSchema InferMap(
            LogicalCall call,
            LogicalSchema input,
            LogicalSchema enclosing,
            string path)
        {
            var transformed = InferArgument(call, "transformation", input, enclosing, path);
            return new ArrayLogicalSchema(transformed ?? new AnyLogicalSchema());
        }

        private LogicalSchema InferFlatMap(
            LogicalCall call,
            LogicalSchema input,
            LogicalSchema enclosing,
            string path)
        {
            var transformed = InferArgument(call, "expression", input, enclosing, path)
                ?? new AnyLogicalSchema();
            return transformed is ArrayLogicalSchema result
                ? result
                : new ArrayLogicalSchema(Dynamic(path, "flat-map result item"));
        }

        private LogicalSchema InferArray(
            LogicalCall call,
            LogicalSchema input,
            LogicalSchema enclosing,
            string path)
        {
            var item = (LogicalSchema)new AnyLogicalSchema();
            for (var index = 0; index < call.Arguments.Count; index++)
            {
                var value = InferArgument(call, index, input, enclosing, path);
                if (value is not null)
                    item = Union(item, value);
            }
            return new ArrayLogicalSchema(item);
        }

        private LogicalSchema InferTuple(
            LogicalCall call,
            LogicalSchema input,
            LogicalSchema enclosing,
            string path)
            => new TupleLogicalSchema(Enumerable.Range(0, call.Arguments.Count)
                .Select(index => InferArgument(call, index, input, enclosing, path))
                .Where(schema => schema is not null)
                .Cast<LogicalSchema>()
                .ToArray());

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
                if (argument.Value is LogicalCall entry && entry.Function.Name == "named-entry")
                {
                    var name = LiteralText(entry, "name");
                    if (name is not null && valueSchema is not null)
                        fields[name] = new LogicalSchemaField(valueSchema);
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

        private LogicalSchema InferPut(
            LogicalCall call,
            LogicalSchema input,
            LogicalSchema enclosing,
            string path)
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
                if (entry.Function.Name != "named-entry")
                    continue;
                var name = LiteralText(entry, "name");
                if (name is not null && valueSchema is not null)
                    fields[name] = new LogicalSchemaField(valueSchema);
            }
            return new RecordLogicalSchema(fields, source.AllowsAdditionalFields, source.IsNullable);
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
                "tuple" => new TupleLogicalSchema([]),
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
            if (left is AnyLogicalSchema)
                return right;
            if (right is AnyLogicalSchema)
                return left;
            if (left == right)
                return left;
            if (left is ScalarLogicalSchema leftScalar && right is ScalarLogicalSchema rightScalar)
            {
                if (IsNumeric(leftScalar.Type) && IsNumeric(rightScalar.Type))
                    return new ScalarLogicalSchema("numeric", IsNullable(left) || IsNullable(right));
                if (IsTemporal(leftScalar.Type) && IsTemporal(rightScalar.Type))
                    return new ScalarLogicalSchema("temporal", IsNullable(left) || IsNullable(right));
            }
            return new AnyLogicalSchema(IsNullable(left) || IsNullable(right));
        }

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
            ConflictingLogicalSchema value => value.IsNullable,
            _ => false,
        };

        private static bool ContainsAny(LogicalSchema schema) => schema switch
        {
            AnyLogicalSchema => true,
            RecordLogicalSchema record => record.Fields.Values.Any(field => ContainsAny(field.Schema)),
            ArrayLogicalSchema array => ContainsAny(array.Items),
            TupleLogicalSchema tuple => tuple.Items.Any(ContainsAny),
            ConflictingLogicalSchema conflict => ContainsAny(conflict.Left) || ContainsAny(conflict.Right),
            _ => false,
        };

        private static bool ContainsConflict(LogicalSchema schema) => schema switch
        {
            ConflictingLogicalSchema => true,
            RecordLogicalSchema record => record.Fields.Values.Any(field => ContainsConflict(field.Schema)),
            ArrayLogicalSchema array => ContainsConflict(array.Items),
            TupleLogicalSchema tuple => tuple.Items.Any(ContainsConflict),
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
            ConflictingLogicalSchema => "conflict",
            _ => schema.GetType().Name,
        };

        private sealed record Requirement(LogicalSchema Input, LogicalSchema Enclosing);
    }
}
