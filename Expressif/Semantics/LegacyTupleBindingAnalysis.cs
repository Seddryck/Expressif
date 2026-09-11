using Expressif.Bindings;
using Expressif.Functions;
using Expressif.Predicates;
using Expressif.Syntax;
using BoundFunction = Expressif.Bindings.Function;

namespace Expressif.Semantics;

/// <summary>Resolved deprecated argument injection, including its signature and directional mapping.</summary>
public sealed record LegacyTupleBindingUse(string Operator, string Callable, SourceSpan? Span,
    Type ImplementationType, IReadOnlyList<TupleBindingSignature> Signatures,
    string PipelineInput, string Arguments, string Replacement, bool CanRewrite)
{
    public string Code { get; } = "implicit-tuple-binding";
    public string Message => $"Implicit argument injection into '{Callable}' is deprecated; use an explicit binding expression.";
}

/// <summary>Analyzes usage rather than deprecating the surrounding operator or target callable.</summary>
public sealed class LegacyTupleBindingAnalyzer
{
    private const string MapOver = "map-over";
    private const string MapWith = "map-with";
    private readonly BaseTypeMapper functions;
    private readonly PredicateTypeMapper predicates = new();

    public LegacyTupleBindingAnalyzer()
        : this(new FunctionTypeMapper()) { }
    public LegacyTupleBindingAnalyzer(BaseTypeMapper functions) => this.functions = functions;

    public IReadOnlyList<LegacyTupleBindingUse> Analyze(RootExpressionSyntax syntax)
    {
        var uses = new List<LegacyTupleBindingUse>();
        try { Visit(new ExpressifBinder(applyCoercion: false).Bind(syntax), uses); }
        catch (BindingException) { return []; }
        return uses;
    }

    private void Visit(IRootExpression root, List<LegacyTupleBindingUse> uses)
    {
        if (root is ClosedRootExpression closed) Pipeline(closed.Expression.Members, closed.Expression.Parameter, uses);
        if (root is OpenRootExpression open)
        {
            if (open.Expression is InputBoundExpression binding) Visit(binding.Body, uses);
            else Pipeline(open.Expression.Members, null, uses);
        }
    }

    private void Pipeline(IEnumerable<BoundFunction> members, IParameter? input, List<LegacyTupleBindingUse> uses)
    {
        foreach (var member in members)
        {
            var consumer = member.Name.ToKebabCase();
            if (consumer is "adjacent" or "chunk-while" or MapOver or MapWith)
                Inspect(member, consumer, input, uses);
            foreach (var parameter in member.Parameters)
            {
                if (parameter is OpenExpressionParameter open) Visit(new OpenRootExpression(open.Expression), uses);
                if (parameter is InputExpressionParameter closed) Visit(new ClosedRootExpression(closed.Expression), uses);
            }
            input = null;
        }
    }

    private void Inspect(BoundFunction member, string consumer, IParameter? input, List<LegacyTupleBindingUse> uses)
    {
        if (!functions.TryExecute(consumer, out var consumerType)) return;
        var parameters = ParameterArgumentBinder.Bind(consumerType, member.Arguments).Parameters;
        if (parameters.FirstOrDefault() is not OpenExpressionParameter operation
            || !LegacyTupleBindingRules.IsCandidate(consumer, operation.Expression)) return;
        var callable = operation.Expression.Members.First();
        if (!functions.TryExecute(callable.Name, out var type) && !predicates.TryExecute(callable.Name, out type)) return;
        var directional = consumer is MapOver or MapWith;
        if (!directional && !LegacyTupleBindingRules.HasBinarySignature(type)) return;
        var candidates = TupleBindingCapabilities.Describe(type).Where(signature => directional
            || signature.Constructor.GetParameters().Length == 1).ToArray();
        if (candidates.Length == 0) return;
        var invocations = KnownInvocations(consumer, input, parameters.ElementAtOrDefault(1));
        var analyzer = new TupleBindingAnalyzer(functions);
        var bind = new BoundFunction("bind", [new QuotedLiteralParameter(callable.Name)]);
        var valid = invocations is { Count: > 0 }
            && invocations.All(tuple => IsKnown(tuple) && analyzer.Inspect(bind, tuple).Failure is null);
        var selected = SelectSignatures(type, candidates, invocations, ref valid);
        uses.Add(CreateUse(consumer, callable, type, candidates, selected, valid));
    }

    private static LegacyTupleBindingUse CreateUse(string consumer, BoundFunction callable, Type type,
        TupleBindingSignature[] candidates, List<TupleBindingSignature> selected, bool valid)
    {
        var prefix = consumer != MapOver;
        return new(consumer, callable.Name, callable.SourceSpan, type,
            selected.Count > 0 ? selected.Distinct().ToArray() : candidates,
            consumer switch { MapOver => "outer input", MapWith => "supplied item", _ => "current item" },
            consumer switch { MapOver => "supplied item, expanding its tuple positions once", MapWith => "outer input as one value", _ => "previous item" },
            prefix ? "~" + callable.Name : callable.Name + "~", valid && candidates.Any(signature => signature.SupportsTupleBinding));
    }

    private static List<TupleBindingSignature> SelectSignatures(Type type, TupleBindingSignature[] candidates,
        List<TupleParameter>? invocations, ref bool valid)
    {
        var selected = new List<TupleBindingSignature>();
        if (invocations is not null)
        {
            foreach (var invocation in invocations)
            {
                try
                {
                    var signature = TupleBindingCapabilities.Resolve(type, invocation.Values.Skip(1).Select(value => new FunctionArgument(null, value)).ToArray());
                    selected.Add(candidates.Single(candidate => candidate.Constructor == signature.Constructor));
                }
                catch (BindingException) { valid = false; }
            }
        }
        return selected;
    }

    private static List<TupleParameter>? KnownInvocations(string consumer, IParameter? input, IParameter? values)
    {
        if (consumer is "adjacent" or "chunk-while")
        {
            if (input is not ArrayParameter array || array.Elements.Any(element => element.IsSpread)) return null;
            return array.Values.Zip(array.Values.Skip(1), (previous, current) => new TupleParameter([current, previous])).ToList();
        }
        if (input is null || values is not ArrayParameter items || items.Elements.Any(element => element.IsSpread)) return null;
        return items.Values.Select(item => PrepareInvocation(consumer, input, item)).ToList();
    }

    private static TupleParameter PrepareInvocation(string consumer, IParameter input, IParameter item)
    {
        if (consumer == MapWith) return new TupleParameter([item, input]);
        return item is TupleParameter tuple
            ? new TupleParameter([input, .. tuple.Values]) : new TupleParameter([input, item]);
    }

    private static bool IsKnown(IParameter parameter) => parameter switch
    {
        LiteralParameter or QuotedLiteralParameter => true,
        TupleParameter tuple => tuple.Elements.All(element => !element.IsSpread && IsKnown(element.Value)),
        ArrayParameter array => array.Elements.All(element => !element.IsSpread && IsKnown(element.Value)),
        _ => false,
    };
}
