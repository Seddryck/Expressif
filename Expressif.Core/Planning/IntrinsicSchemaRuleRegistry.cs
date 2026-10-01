namespace Expressif.Planning;

internal sealed partial class LogicalSchemaAnalysisSession
{
    private interface IIntrinsicSchemaRule
    {
        Requirement Require(LogicalCall call, LogicalSchema expected, string path);

        LogicalSchema Infer(
            LogicalCall call,
            LogicalSchema input,
            LogicalSchema enclosing,
            string path,
            string intrinsic);
    }

    private sealed class IntrinsicRuleRegistry(
        IReadOnlyDictionary<string, IIntrinsicSchemaRule> rules,
        IIntrinsicSchemaRule tuplePosition)
    {
        public static IntrinsicRuleRegistry Create(LogicalSchemaAnalysisSession session)
        {
            var rules = new Dictionary<string, IIntrinsicSchemaRule>(StringComparer.Ordinal);
            Add(rules, "field", session.RequireField, session.InferField);
            Add(rules, "select-fields", session.RequireSelectFields, session.InferSelectFields);
            Add(rules, "explode-field", session.RequireExplode,
                (call, input, _, path) => session.InferExplode(call, input, path, preserveParent: false));
            Add(rules, "explode-field-outer", session.RequireExplode,
                (call, input, _, path) => session.InferExplode(call, input, path, preserveParent: true));
            Add(rules, "implode-field", session.RequireImplode,
                (call, input, _, path) => session.InferImplode(call, input, path, preserveNull: true));
            Add(rules, "implode-field-inner", session.RequireImplode,
                (call, input, _, path) => session.InferImplode(call, input, path, preserveNull: false));
            Add(rules, "with", session.RequireWith, session.InferWith);
            Add(rules, "array", RequireGeneric(session), session.InferArray);
            Add(rules, "tuple", RequireGeneric(session), session.InferTuple);
            Add(rules, "record", RequireGeneric(session), session.InferRecord);
            Add(rules, "dictionary", RequireGeneric(session),
                (call, input, enclosing, path) => session.InferAssociative(call, input, enclosing, path, grouping: false));
            Add(rules, "grouping", RequireGeneric(session),
                (call, input, enclosing, path) => session.InferAssociative(call, input, enclosing, path, grouping: true));
            Add(rules, "put", RequireGeneric(session),
                (call, input, enclosing, path) => session.InferPut(call, input, enclosing, path, RecordMutation.Always));
            Add(rules, "put-present", RequireGeneric(session),
                (call, input, enclosing, path) => session.InferPut(call, input, enclosing, path, RecordMutation.WhenPresent));
            Add(rules, "put-absent", RequireGeneric(session),
                (call, input, enclosing, path) => session.InferPut(call, input, enclosing, path, RecordMutation.WhenAbsent));
            Add(rules, "named-entry", RequireGeneric(session), session.InferNamedEntry);
            Add(rules, "spread-entry", session.RequireSpreadEntry, session.InferSpreadEntry);
            Add(rules, "sort-criterion",
                (call, _, path) => session.RequireSortCriterion(call, path),
                session.InferSortCriterion);
            var tuple = new DelegateIntrinsicSchemaRule(
                (call, _, path) => session.RequireTuplePosition(call, path),
                (call, input, enclosing, path, intrinsic)
                    => session.InferTuplePosition(call, input, enclosing, path, intrinsic));
            return new IntrinsicRuleRegistry(rules, tuple);
        }

        public Requirement Require(
            string intrinsic,
            LogicalCall call,
            LogicalSchema expected,
            string path)
            => Resolve(intrinsic, call).Require(call, expected, path);

        public LogicalSchema Infer(
            string intrinsic,
            LogicalCall call,
            LogicalSchema input,
            LogicalSchema enclosing,
            string path)
            => Resolve(intrinsic, call).Infer(call, input, enclosing, path, intrinsic);

        private static Func<LogicalCall, LogicalSchema, string, Requirement> RequireGeneric(
            LogicalSchemaAnalysisSession session)
            => (call, _, path) => session.RequireGenericCall(call, path);

        private static void Add(
            IDictionary<string, IIntrinsicSchemaRule> rules,
            string name,
            Func<LogicalCall, LogicalSchema, string, Requirement> require,
            Func<LogicalCall, LogicalSchema, LogicalSchema, string, LogicalSchema> infer)
            => rules.Add(name, new DelegateIntrinsicSchemaRule(
                require,
                (call, input, enclosing, path, _) => infer(call, input, enclosing, path)));

        private IIntrinsicSchemaRule Resolve(string intrinsic, LogicalCall call)
        {
            if (rules.TryGetValue(intrinsic, out var rule))
                return rule;
            if (intrinsic.StartsWith("tuple-position", StringComparison.Ordinal))
                return tuplePosition;
            throw new InvalidOperationException(
                $"Unsupported schema intrinsic '{intrinsic}' for '{call.Function.Name}'.");
        }
    }

    private sealed class DelegateIntrinsicSchemaRule(
        Func<LogicalCall, LogicalSchema, string, Requirement> require,
        Func<LogicalCall, LogicalSchema, LogicalSchema, string, string, LogicalSchema> infer)
        : IIntrinsicSchemaRule
    {
        public Requirement Require(LogicalCall call, LogicalSchema expected, string path)
            => require(call, expected, path);

        public LogicalSchema Infer(
            LogicalCall call,
            LogicalSchema input,
            LogicalSchema enclosing,
            string path,
            string intrinsic)
            => infer(call, input, enclosing, path, intrinsic);
    }
}
