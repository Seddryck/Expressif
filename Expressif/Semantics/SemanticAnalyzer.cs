using Expressif.Bindings;
using Expressif.Functions;
using Expressif.Functions.Array;
using Expressif.Predicates;
using Expressif.Syntax;
using BoundFunction = Expressif.Bindings.Function;

namespace Expressif.Semantics;

/// <summary>
/// Resolves field-reference value sources without constructing or evaluating
/// runtime functions. No context, variable provider, schema or input is required.
/// </summary>
public sealed class SemanticAnalyzer
{
    public SemanticAnalysis Analyze(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        try
        {
            return Analyze(ExpressifSyntax.Parse(text));
        }
        catch (ExpressifSyntaxException exception)
        {
            return new(null, System.Array.Empty<FieldReference>(), new[] { exception.Message });
        }
    }

    public SemanticAnalysis Analyze(RootExpressionSyntax syntax)
    {
        ArgumentNullException.ThrowIfNull(syntax);
        return new Analysis(syntax).Run();
    }

    private sealed class Analysis(RootExpressionSyntax syntax)
    {
        private static readonly SemanticSource External = new(SemanticSourceKind.ExternalInput);
        private static readonly SemanticSource Missing = Unknown("There is no enclosing expression scope.");
        private readonly ExpressifBinder binder = new(applyCoercion: false, trackSources: true);
        private readonly FunctionTypeMapper functions = new();
        private readonly PredicateTypeMapper predicates = new();
        private readonly List<FieldReference> references = [];
        private readonly List<string> diagnostics = [];
        private BindingSourceMap Sources => binder.Sources;
        private SemanticSource? contextInput;

        public SemanticAnalysis Run()
        {
            var frame = new ScopeFrame<SemanticSource>(External, External);
            try
            {
                switch (binder.Bind(syntax))
                {
                    case OpenRootExpression open:
                        Pipeline(open.Expression.Members, External, frame);
                        break;
                    case ClosedRootExpression closed:
                        Closed(closed.Expression, External, frame);
                        break;
                }
            }
            catch (Exception exception) when (exception is ExpressifException or ArgumentException)
            {
                references.Clear();
                diagnostics.Add(exception.Message);
            }

            foreach (var access in Descendants(syntax).OfType<RecordAccessSyntax>())
            {
                for (var index = 0; index < access.Fields.Count; index++)
                {
                    if (!references.Any(reference => ReferenceEquals(reference.Syntax, access) && reference.SelectionIndex == index))
                    {
                        var unknown = Unknown("This binding or scope transition is unsupported.");
                        AddReference(access, index, unknown, new(unknown, unknown, new(unknown, unknown)));
                    }
                }
            }

            return new(syntax, references.OrderBy(reference => reference.Span.Start).ToArray(), diagnostics.ToArray());
        }

        private SemanticSource Closed(ClosedExpression expression, SemanticSource input, ScopeFrame<SemanticSource> frame)
        {
            var source = Parameter(expression.Parameter, input, frame);
            return Pipeline(expression.Members, source, frame.Derive(source));
        }

        private SemanticSource Pipeline(IEnumerable<BoundFunction> members, SemanticSource input, ScopeFrame<SemanticSource> frame)
        {
            var current = input;
            foreach (var member in members)
            {
                if (Sources.GetField(member) is { } field)
                {
                    var kind = Kind(field.Access, field.Index);
                    var source = frame.Resolve(kind, current, Missing);
                    AddReference(field.Access, field.Index, source, frame);
                }
                else
                {
                    if (!functions.TryExecute(member.Name, out _) && !predicates.TryExecute(member.Name, out _)
                        && !FunctionFactory.IsImplicitFoldAccumulator(member))
                    {
                        current = Unknown($"The callable '{member.Name}' cannot be resolved by this analysis.");
                        continue;
                    }
                    Arguments(member, current, frame);
                }
                if (current.Kind != SemanticSourceKind.Unresolved)
                    current = Source(member);
            }
            return current;
        }

        private void Arguments(BoundFunction function, SemanticSource input, ScopeFrame<SemanticSource> frame)
        {
            if (FunctionFactory.IsImplicitFoldAccumulator(function))
                return;
            if (!functions.TryExecute(function.Name, out var type))
            {
                if (!predicates.TryExecute(function.Name, out type))
                    return;
                // Predicate factories have their own nested Boolean scopes. Simple
                // scalar arguments read the existing frame; combinators are deferred.
                if (function.Parameters.Length > 1 && function.Parameters.Any(parameter => parameter is OpenExpressionParameter))
                    return;
                foreach (var parameter in function.Parameters)
                {
                    if (parameter is OpenExpressionParameter open)
                        Pipeline(open.Expression.Members, frame.Current, frame.Derive(frame.Current));
                    else
                        Parameter(parameter, frame.Current, frame);
                }
                return;
            }

            if (function.Parameters is [WithDefinitionParameter definition])
            {
                foreach (var projection in definition.Projections)
                    ValueParameter(projection.Value, input, frame);
                var temporary = new SemanticSource(SemanticSourceKind.Expression, Sources.Get(function), input);
                ValueParameter(definition.Body, temporary, frame.Derive(temporary));
                return;
            }
            if (function.Parameters is [RecordDefinitionParameter record])
            {
                foreach (var entry in record.Entries)
                {
                    if (entry is RecordNamedEntry named)
                        RecordParameter(named.Value, input, frame);
                    else if (entry is RecordSpreadEntry spread)
                        RecordParameter(spread.Value, input, frame);
                }
                return;
            }
            if (type == typeof(Functions.Flow.Apply) && function.Parameters is [OpenExpressionParameter applied])
            {
                Pipeline(applied.Expression.Members, input, frame.Derive(input));
                return;
            }

            if (type == typeof(MapOver) || type == typeof(MapWith))
            {
                var parameters = ParameterArgumentBinder.Bind(type, function.Arguments).Parameters;
                if (parameters is not [OpenExpressionParameter operation, var values])
                    return;
                var collection = ValueParameter(values, frame.Current, frame);
                var item = new SemanticSource(SemanticSourceKind.Element, collection.Syntax, collection);
                var inputs = DirectionalScope<SemanticSource>.Create(type == typeof(MapOver), input, item);
                Pipeline(operation.Expression.Members, inputs.Input, frame.Derive(inputs.Arguments));
                return;
            }

            if (type == typeof(Map) || type == typeof(Filter))
            {
                var parameters = ParameterArgumentBinder.Bind(type, function.Arguments).Parameters;
                if (parameters is not [OpenExpressionParameter operation])
                    return;
                var item = new SemanticSource(SemanticSourceKind.Element, input.Syntax, input);
                var members = operation.Expression.Members.ToArray();
                var nested = type == typeof(Map) || !FunctionConstruction.IsPredicatePipeline(members, member => predicates.TryExecute(member.Name, out _));
                Pipeline(members, item, nested ? frame.Derive(item) : frame);
                return;
            }

            // Only ordinary scalar-provider constructors follow the default factory
            // path. Factory-integrated operators remain explicitly unresolved.
            if (FunctionConstruction.Classify(function.Name) != FunctionConstructionKind.Standard
                || typeof(IValueSpreadAware).IsAssignableFrom(type))
                return;
            if (type.GetConstructors().Any(constructor => constructor.GetParameters().Any(parameter =>
                parameter.ParameterType == typeof(Func<IFunction>) || parameter.ParameterType == typeof(Func<IPredicate>))))
                return;
            foreach (var parameter in ParameterArgumentBinder.Bind(type, function.Arguments).Parameters)
                Parameter(parameter, frame.Current, frame);
        }

        private SemanticSource RecordParameter(IParameter parameter, SemanticSource input, ScopeFrame<SemanticSource> frame)
            => parameter is OpenExpressionParameter open
                ? Pipeline(open.Expression.Members, input, frame.Derive(input))
                : Parameter(parameter, frame.Current, frame);

        private SemanticSource ValueParameter(IParameter parameter, SemanticSource input, ScopeFrame<SemanticSource> frame)
        {
            var previous = contextInput;
            contextInput = input;
            try
            {
                return ValueParameterCore(parameter, input, frame);
            }
            finally
            {
                contextInput = previous;
            }
        }

        private SemanticSource ValueParameterCore(IParameter parameter, SemanticSource input, ScopeFrame<SemanticSource> frame)
        {
            IEnumerable<IParameter>? children = parameter switch
            {
                ArrayParameter array => array.Elements.Select(element => element.Value),
                PairParameter pair => [pair.Key, pair.Value],
                GroupingParameter grouping => grouping.Entries,
                DictionaryParameter dictionary => dictionary.Entries,
                RecordLiteralParameter record => record.Fields.Select(field => field.Value),
                _ => null,
            };
            if (children is not null)
            {
                foreach (var child in children)
                    ValueParameter(child, input, frame);
                return Source(parameter);
            }
            // BuildValueEvaluator supplies the function input as the legacy context
            // object. Open expressions additionally enter a nested evaluation frame.
            if (parameter is InputExpressionParameter closed)
            {
                var source = ValueParameter(closed.Expression.Parameter, input, frame);
                return Pipeline(closed.Expression.Members, source, frame);
            }
            if (parameter is ObjectPropertyParameter && Sources.GetField(parameter) is { } field)
            {
                AddReference(field.Access, field.Index, input, frame);
                return Source(parameter);
            }
            return Parameter(parameter, input, frame);
        }

        private SemanticSource Parameter(IParameter parameter, SemanticSource input, ScopeFrame<SemanticSource> frame)
        {
            if (Sources.GetField(parameter) is { } field)
            {
                var source = parameter is ObjectPropertyParameter or ObjectIndexParameter
                    ? ArgumentScope.Root(contextInput, frame.Root)
                    : frame.Resolve(Kind(field.Access, field.Index), input, Missing);
                AddReference(field.Access, field.Index, source, frame);
                return Source(parameter);
            }

            switch (parameter)
            {
                case IncomingValueParameter:
                    return input;
                case VariableParameter:
                    return new(SemanticSourceKind.ExternalInput, Reason: "The value is supplied by a variable provider.");
                case OpenExpressionParameter open:
                    var argument = ArgumentScope.Root(contextInput, input);
                    return Pipeline(open.Expression.Members, argument, frame.Derive(argument));
                case InputExpressionParameter closed:
                    return Closed(closed.Expression, input, frame);
                case ArrayParameter array:
                    foreach (var element in array.Elements)
                        Parameter(element.Value, input, frame);
                    break;
                case TupleParameter tuple:
                    foreach (var element in tuple.Elements)
                        Parameter(element.Value, input, frame);
                    break;
                case VectorParameter vector:
                    foreach (var element in vector.Elements)
                        Parameter(element.Value, input, frame);
                    break;
                case RecordLiteralParameter record:
                    foreach (var recordField in record.Fields)
                        Parameter(recordField.Value, input, frame);
                    break;
                case PairParameter pair:
                    Parameter(pair.Key, input, frame);
                    Parameter(pair.Value, input, frame);
                    break;
                case GroupingParameter grouping:
                    foreach (var entry in grouping.Entries)
                        Parameter(entry, input, frame);
                    break;
                case DictionaryParameter dictionary:
                    foreach (var entry in dictionary.Entries)
                        Parameter(entry, input, frame);
                    break;
            }
            return Source(parameter);
        }

        private SemanticSource Source(object bound)
        {
            if (Sources.GetField(bound) is { } field)
            {
                var selection = FieldSpans.Get(field.Access, field.Index);
                return new(SemanticSourceKind.Expression, field.Access,
                    Region: new SourceSpan(field.Access.Span.Start, selection.End - field.Access.Span.Start));
            }
            return Sources.Get(bound) is { } node
                ? new(SemanticSourceKind.Expression, node)
                : Unknown("The supplying operation has no source association.");
        }

        private void AddReference(RecordAccessSyntax access, int index, SemanticSource source, ScopeFrame<SemanticSource> frame)
        {
            var span = FieldSpans.Get(access, index);
            references.Add(new(access, index, span, Kind(access, index), source, frame.Root, frame.EnclosingRoot ?? Missing));
        }

        private static FieldReferenceKind Kind(RecordAccessSyntax access, int index)
            => (index == 0 ? access.RootDepth : 0) switch
            {
                1 => FieldReferenceKind.ExpressionRoot,
                2 => FieldReferenceKind.EnclosingExpressionRoot,
                _ => FieldReferenceKind.CurrentInput,
            };

        private static SemanticSource Unknown(string reason) => new(SemanticSourceKind.Unresolved, Reason: reason);

        private static IEnumerable<SyntaxNode> Descendants(SyntaxNode node)
        {
            yield return node;
            foreach (var child in node.Children)
            {
                foreach (var descendant in Descendants(child))
                    yield return descendant;
            }
        }
    }
}
