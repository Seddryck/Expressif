namespace Expressif.Planning;

internal sealed partial class LogicalSchemaAnalysisSession
{
    internal interface IIntrinsicSchemaRule
    {
        IReadOnlyCollection<string> Intrinsics { get; }

        bool Matches(string intrinsic) => Intrinsics.Contains(intrinsic, StringComparer.Ordinal);

        Requirement Require(LogicalCall call, LogicalSchema expected, string path, string intrinsic);

        LogicalSchema Infer(
            LogicalCall call,
            LogicalSchema input,
            LogicalSchema enclosing,
            string path,
            string intrinsic);
    }

    private sealed class IntrinsicRuleRegistry
    {
        private readonly Dictionary<string, IIntrinsicSchemaRule> rules = new(StringComparer.Ordinal);
        private readonly List<IIntrinsicSchemaRule> patternRules = [];

        public static IntrinsicRuleRegistry Create(LogicalSchemaAnalysisSession session)
        {
            var registry = new IntrinsicRuleRegistry();
            registry.Register(new FieldIntrinsicSchemaRule(session));
            registry.Register(new ExplodeIntrinsicSchemaRule(session));
            registry.Register(new ImplodeIntrinsicSchemaRule(session));
            registry.Register(new WithIntrinsicSchemaRule(session));
            registry.Register(new CollectionIntrinsicSchemaRule(session));
            registry.Register(new RecordMutationIntrinsicSchemaRule(session));
            registry.Register(new EntryIntrinsicSchemaRule(session));
            registry.Register(new SortCriterionIntrinsicSchemaRule(session));
            registry.Register(new TuplePositionIntrinsicSchemaRule(session));
            return registry;
        }

        public void Register(IIntrinsicSchemaRule rule)
        {
            if (rule.Intrinsics.Count == 0)
            {
                patternRules.Add(rule);
                return;
            }

            var duplicate = rule.Intrinsics.FirstOrDefault(rules.ContainsKey);
            if (duplicate is not null)
                throw new InvalidOperationException($"A schema rule is already registered for '{duplicate}'.");

            foreach (var intrinsic in rule.Intrinsics)
                rules.Add(intrinsic, rule);
        }

        public Requirement Require(
            string intrinsic,
            LogicalCall call,
            LogicalSchema expected,
            string path)
            => Resolve(intrinsic, call).Require(call, expected, path, intrinsic);

        public LogicalSchema Infer(
            string intrinsic,
            LogicalCall call,
            LogicalSchema input,
            LogicalSchema enclosing,
            string path)
            => Resolve(intrinsic, call).Infer(call, input, enclosing, path, intrinsic);

        private IIntrinsicSchemaRule Resolve(string intrinsic, LogicalCall call)
        {
            if (rules.TryGetValue(intrinsic, out var rule))
                return rule;
            var matching = patternRules.Where(candidate => candidate.Matches(intrinsic)).ToArray();
            return matching.Length switch
            {
                1 => matching[0],
                > 1 => throw new InvalidOperationException(
                    $"Multiple schema rules are registered for '{intrinsic}'."),
                _ => throw new InvalidOperationException(
                    $"Unsupported schema intrinsic '{intrinsic}' for '{call.Function.Name}'."),
            };
        }
    }

    private abstract class IntrinsicSchemaRule(LogicalSchemaAnalysisSession session)
        : IIntrinsicSchemaRule
    {
        protected LogicalSchemaAnalysisSession Session { get; } = session;

        public abstract IReadOnlyCollection<string> Intrinsics { get; }

        public virtual bool Matches(string intrinsic)
            => Intrinsics.Contains(intrinsic, StringComparer.Ordinal);

        public virtual Requirement Require(
            LogicalCall call,
            LogicalSchema expected,
            string path,
            string intrinsic)
            => Session.RequireGenericCall(call, path);

        public abstract LogicalSchema Infer(
            LogicalCall call,
            LogicalSchema input,
            LogicalSchema enclosing,
            string path,
            string intrinsic);
    }

    private sealed class FieldIntrinsicSchemaRule(LogicalSchemaAnalysisSession session)
        : IntrinsicSchemaRule(session)
    {
        public override IReadOnlyCollection<string> Intrinsics { get; } = ["field", "select-fields"];

        public override Requirement Require(LogicalCall call, LogicalSchema expected, string path, string intrinsic)
            => intrinsic == "field"
                ? Session.RequireField(call, expected, path)
                : Session.RequireSelectFields(call, expected, path);

        public override LogicalSchema Infer(
            LogicalCall call, LogicalSchema input, LogicalSchema enclosing, string path, string intrinsic)
            => intrinsic == "field"
                ? Session.InferField(call, input, enclosing, path)
                : Session.InferSelectFields(call, input, enclosing, path);
    }

    private sealed class ExplodeIntrinsicSchemaRule(LogicalSchemaAnalysisSession session)
        : IntrinsicSchemaRule(session)
    {
        public override IReadOnlyCollection<string> Intrinsics { get; } =
            ["explode-field", "explode-field-outer"];

        public override Requirement Require(LogicalCall call, LogicalSchema expected, string path, string intrinsic)
            => Session.RequireExplode(call, expected, path);

        public override LogicalSchema Infer(
            LogicalCall call, LogicalSchema input, LogicalSchema enclosing, string path, string intrinsic)
            => Session.InferExplode(call, input, path, intrinsic == "explode-field-outer");
    }

    private sealed class ImplodeIntrinsicSchemaRule(LogicalSchemaAnalysisSession session)
        : IntrinsicSchemaRule(session)
    {
        public override IReadOnlyCollection<string> Intrinsics { get; } =
            ["implode-field", "implode-field-inner"];

        public override Requirement Require(LogicalCall call, LogicalSchema expected, string path, string intrinsic)
            => Session.RequireImplode(call, expected, path);

        public override LogicalSchema Infer(
            LogicalCall call, LogicalSchema input, LogicalSchema enclosing, string path, string intrinsic)
            => Session.InferImplode(call, input, path, intrinsic == "implode-field");
    }

    private sealed class WithIntrinsicSchemaRule(LogicalSchemaAnalysisSession session)
        : IntrinsicSchemaRule(session)
    {
        public override IReadOnlyCollection<string> Intrinsics { get; } = ["with"];

        public override Requirement Require(LogicalCall call, LogicalSchema expected, string path, string intrinsic)
            => Session.RequireWith(call, expected, path);

        public override LogicalSchema Infer(
            LogicalCall call, LogicalSchema input, LogicalSchema enclosing, string path, string intrinsic)
            => Session.InferWith(call, input, enclosing, path);
    }

    private sealed class CollectionIntrinsicSchemaRule(LogicalSchemaAnalysisSession session)
        : IntrinsicSchemaRule(session)
    {
        public override IReadOnlyCollection<string> Intrinsics { get; } =
            ["array", "tuple", "record", "dictionary", "grouping"];

        public override LogicalSchema Infer(
            LogicalCall call, LogicalSchema input, LogicalSchema enclosing, string path, string intrinsic)
            => intrinsic switch
            {
                "array" => Session.InferArray(call, input, enclosing, path),
                "tuple" => Session.InferTuple(call, input, enclosing, path),
                "record" => Session.InferRecord(call, input, enclosing, path),
                "dictionary" => Session.InferAssociative(call, input, enclosing, path, grouping: false),
                "grouping" => Session.InferAssociative(call, input, enclosing, path, grouping: true),
                _ => throw new InvalidOperationException($"Unsupported collection intrinsic '{intrinsic}'."),
            };
    }

    private sealed class RecordMutationIntrinsicSchemaRule(LogicalSchemaAnalysisSession session)
        : IntrinsicSchemaRule(session)
    {
        public override IReadOnlyCollection<string> Intrinsics { get; } =
            ["put", "put-present", "put-absent"];

        public override LogicalSchema Infer(
            LogicalCall call, LogicalSchema input, LogicalSchema enclosing, string path, string intrinsic)
            => Session.InferPut(call, input, enclosing, path, intrinsic switch
            {
                "put" => RecordMutation.Always,
                "put-present" => RecordMutation.WhenPresent,
                "put-absent" => RecordMutation.WhenAbsent,
                _ => throw new InvalidOperationException($"Unsupported record mutation intrinsic '{intrinsic}'."),
            });
    }

    private sealed class EntryIntrinsicSchemaRule(LogicalSchemaAnalysisSession session)
        : IntrinsicSchemaRule(session)
    {
        public override IReadOnlyCollection<string> Intrinsics { get; } = ["named-entry", "spread-entry"];

        public override Requirement Require(LogicalCall call, LogicalSchema expected, string path, string intrinsic)
            => intrinsic == "spread-entry"
                ? Session.RequireSpreadEntry(call, expected, path)
                : base.Require(call, expected, path, intrinsic);

        public override LogicalSchema Infer(
            LogicalCall call, LogicalSchema input, LogicalSchema enclosing, string path, string intrinsic)
            => intrinsic == "named-entry"
                ? Session.InferNamedEntry(call, input, enclosing, path)
                : Session.InferSpreadEntry(call, input, enclosing, path);
    }

    private sealed class SortCriterionIntrinsicSchemaRule(LogicalSchemaAnalysisSession session)
        : IntrinsicSchemaRule(session)
    {
        public override IReadOnlyCollection<string> Intrinsics { get; } = ["sort-criterion"];

        public override Requirement Require(LogicalCall call, LogicalSchema expected, string path, string intrinsic)
            => Session.RequireSortCriterion(call, path);

        public override LogicalSchema Infer(
            LogicalCall call, LogicalSchema input, LogicalSchema enclosing, string path, string intrinsic)
            => Session.InferSortCriterion(call, input, enclosing, path);
    }

    private sealed class TuplePositionIntrinsicSchemaRule(LogicalSchemaAnalysisSession session)
        : IntrinsicSchemaRule(session)
    {
        public override IReadOnlyCollection<string> Intrinsics { get; } = [];

        public override bool Matches(string intrinsic)
            => intrinsic.StartsWith("tuple-position", StringComparison.Ordinal);

        public override Requirement Require(LogicalCall call, LogicalSchema expected, string path, string intrinsic)
            => Session.RequireTuplePosition(call, path);

        public override LogicalSchema Infer(
            LogicalCall call, LogicalSchema input, LogicalSchema enclosing, string path, string intrinsic)
            => Session.InferTuplePosition(call, input, enclosing, path, intrinsic);
    }
}
