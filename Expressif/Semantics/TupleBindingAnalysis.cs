using Expressif.Bindings;
using Expressif.Functions;
using Expressif.Predicates;
using Expressif.Syntax;
using Expressif.Values;
using Expressif.Values.Casters;
using BoundFunction = Expressif.Bindings.Function;

namespace Expressif.Semantics;

/// <summary>A resolved tuple-binding use; unknown tuple shape does not prevent target eligibility.</summary>
public sealed record TupleBindingUse(string? Name, SourceSpan? Span, Type? ImplementationType,
    IReadOnlyList<TupleBindingSignature> Signatures, TupleBindingFailure? Failure, string? Message);

/// <summary>Inspects binding targets and statically known invocations without evaluating user code.</summary>
public sealed class TupleBindingAnalyzer
{
    private readonly BaseTypeMapper functions;
    private readonly PredicateTypeMapper predicates = new();

    public TupleBindingAnalyzer()
        : this(new FunctionTypeMapper()) { }
    public TupleBindingAnalyzer(BaseTypeMapper functions) => this.functions = functions;

    public IReadOnlyList<TupleBindingUse> Analyze(RootExpressionSyntax syntax)
    {
        var uses = new List<TupleBindingUse>();
        try { Visit(new ExpressifBinder(applyCoercion: false).Bind(syntax), uses); }
        catch (BindingException) { return []; }
        return uses;
    }

    private void Visit(IRootExpression root, List<TupleBindingUse> uses)
    {
        if (root is ClosedRootExpression closed)
        {
            VisitPipeline(closed.Expression.Members, closed.Expression.Parameter, uses);
        }
        else if (root is OpenRootExpression open)
        {
            if (open.Expression is InputBoundExpression bound) Visit(bound.Body, uses);
            else VisitPipeline(open.Expression.Members, null, uses);
        }
    }

    private void VisitPipeline(IEnumerable<BoundFunction> members, IParameter? input, List<TupleBindingUse> uses)
    {
        foreach (var member in members)
        {
            VisitParameters(member.Parameters, uses);
            if (member.Name.Equals("bind", StringComparison.OrdinalIgnoreCase))
                uses.Add(Inspect(member, input));
            if (member.Name.Equals("rotate", StringComparison.OrdinalIgnoreCase) && member.Parameters.Length == 0
                && input is TupleParameter { Elements.Length: > 0 } tuple)
                input = new TupleParameter([tuple.Elements[^1], .. tuple.Elements[..^1]]);
            else if (member.Name.Equals("tuple", StringComparison.OrdinalIgnoreCase)
                && member.Arguments.All(argument => argument.Name is null && !argument.IsSpread))
                input = new TupleParameter(member.Parameters);
            else input = null;
        }
    }

    private void VisitParameters(IEnumerable<IParameter> parameters, List<TupleBindingUse> uses)
    {
        foreach (var parameter in parameters)
        {
            if (parameter is OpenExpressionParameter open) Visit(new OpenRootExpression(open.Expression), uses);
            if (parameter is InputExpressionParameter closed) Visit(new ClosedRootExpression(closed.Expression), uses);
        }
    }

    private TupleBindingUse Inspect(BoundFunction member, IParameter? input)
    {
        var name = member.Parameters switch
        {
            [QuotedLiteralParameter literal] => literal.Value,
            [LiteralParameter { Value: string text }] => text,
            _ => null,
        };
        if (name is null) return new(null, member.SourceSpan, null, [], null, null);
        if (!functions.TryExecute(name, out var type) && !predicates.TryExecute(name, out type))
            return new(name, member.SourceSpan, null, [], TupleBindingFailure.UnknownTarget, $"Unknown tuple-binding target '{name}'.");
        var signatures = TupleBindingCapabilities.Describe(type);
        if (!signatures.Any(signature => signature.SupportsTupleBinding))
            return new(name, member.SourceSpan, type, signatures, TupleBindingFailure.IneligibleTarget, $"Callable '{name}' does not support tuple binding.");
        try
        {
            ValidateInvocation(input, type, signatures);
        }
        catch (TupleBindingException exception)
        {
            return new(name, member.SourceSpan, type, signatures, exception.Failure, exception.Message);
        }
        return new(name, member.SourceSpan, type, signatures, null, null);
    }

    private static void ValidateInvocation(IParameter? input, Type type, IReadOnlyList<TupleBindingSignature> signatures)
    {
        if (input is TupleParameter tuple && tuple.Elements.All(element => !element.IsSpread))
        {
            if (tuple.Elements.Length == 0) throw new TupleBindingException(TupleBindingFailure.InvalidInput, "bind requires a tuple input position.");
            var binding = TupleBindingCapabilities.Resolve(type, tuple.Values.Skip(1).Select(value => new FunctionArgument(null, value)).ToArray());
            var signature = signatures.Single(candidate => candidate.Constructor == binding.Constructor);
            if (!signature.Variadic)
            {
                foreach (var pair in binding.Constructor.GetParameters().Zip(binding.Parameters))
                    ValidateValue(pair.Second, pair.First.ParameterType.GetGenericArguments()[0]);
            }
            var inputs = type.GetInterfaces().Where(contract => contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(IFunction<,>))
                .Select(contract => contract.GetGenericArguments()[0]).ToArray();
            if (inputs.Length == 1) ValidateValue(tuple.Values[0], inputs[0]);
        }
        else if (input is LiteralParameter or QuotedLiteralParameter or ArrayParameter)
        {
            throw new TupleBindingException(TupleBindingFailure.InvalidInput, "bind requires tuple input.");
        }
    }

    private static void ValidateValue(IParameter parameter, Type target)
    {
        var value = parameter switch { LiteralParameter literal => literal.Value, QuotedLiteralParameter quoted => quoted.Value, _ => null };
        target = Nullable.GetUnderlyingType(target) ?? target;
        if (value is not null && target != typeof(object) && !new Caster().TryCast(value, target, out _))
            throw new TupleBindingException(TupleBindingFailure.IncompatibleValue, $"Value is incompatible with '{target.Name}'.");
    }
}
