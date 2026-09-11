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

            return new(syntax, references.OrderBy(reference => reference.Span.Start).ToArray(), diagnostics.ToArray())
            {
                TupleBindings = new TupleBindingAnalyzer().Analyze(syntax),
                LegacyTupleBindings = new LegacyTupleBindingAnalyzer().Analyze(syntax),
            };
        }

        private SemanticSource Closed(ClosedExpression expression, SemanticSource input, ScopeFrame<SemanticSource> frame)
        {
            var source = FunctionConstruction.UsesInputValueEvaluator(expression.Parameter)
                ? ValueParameter(expression.Parameter, input, frame)
                : Parameter(expression.Parameter, input, frame);
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
                PredicateArguments(function, frame);
                return;
            }
            if (StructuredArguments(function, input, frame))
                return;
            if (type == typeof(MapOver) || type == typeof(MapWith))
            {
                DirectionalArguments(function, type, input, frame);
                return;
            }
            if (type == typeof(Map) || type == typeof(Filter))
            {
                MappingArguments(function, type, input, frame);
                return;
            }
            if (!UsesScalarProviders(function, type))
                return;
            foreach (var parameter in ParameterArgumentBinder.Bind(type, function.Arguments).Parameters)
                Parameter(parameter, frame.Current, frame);
        }

        private void PredicateArguments(BoundFunction function, ScopeFrame<SemanticSource> frame)
        {
            if (!predicates.TryExecute(function.Name, out _))
                return;
            // Multi-argument combinators have additional deferred Boolean scopes.
            if (function.Parameters.Length > 1 && function.Parameters.Any(parameter => parameter is OpenExpressionParameter))
                return;
            foreach (var parameter in function.Parameters)
            {
                if (parameter is OpenExpressionParameter open)
                    Pipeline(open.Expression.Members, frame.Current, frame.Derive(frame.Current));
                else
                    Parameter(parameter, frame.Current, frame);
            }
        }

        private bool StructuredArguments(BoundFunction function, SemanticSource input, ScopeFrame<SemanticSource> frame)
        {
            var construction = FunctionConstruction.Classify(function.Name);
            if (construction is FunctionConstructionKind.Catch or FunctionConstructionKind.Throw)
            {
                var type = construction == FunctionConstructionKind.Catch
                    ? typeof(Functions.Flow.Catch) : typeof(Functions.Flow.Throw);
                var source = construction == FunctionConstructionKind.Catch ? frame.Current : input;
                foreach (var parameter in ParameterArgumentBinder.Bind(type, function.Arguments).Parameters)
                    ValueParameter(parameter, source, frame);
                return true;
            }
            if (function.Parameters is [WithDefinitionParameter definition])
            {
                WithArguments(function, definition, input, frame);
                return true;
            }
            if (function.Parameters is [RecordDefinitionParameter record])
            {
                RecordArguments(record, input, frame);
                return true;
            }
            if (FunctionConstruction.Classify(function.Name) == FunctionConstructionKind.Apply
                && function.Parameters is [OpenExpressionParameter applied])
            {
                Pipeline(applied.Expression.Members, input, frame.Derive(input));
                return true;
            }
            return false;
        }

        private void WithArguments(BoundFunction function, WithDefinitionParameter definition, SemanticSource input, ScopeFrame<SemanticSource> frame)
        {
            foreach (var projection in definition.Projections)
                ValueParameter(projection.Value, input, frame);
            var temporary = new SemanticSource(SemanticSourceKind.Expression, Sources.Get(function), input);
            ValueParameter(definition.Body, temporary, frame.Derive(temporary));
        }

        private void RecordArguments(RecordDefinitionParameter record, SemanticSource input, ScopeFrame<SemanticSource> frame)
        {
            foreach (var entry in record.Entries)
            {
                if (entry is RecordNamedEntry named)
                    RecordParameter(named.Value, input, frame);
                else if (entry is RecordSpreadEntry spread)
                    RecordParameter(spread.Value, input, frame);
            }
        }

        private void DirectionalArguments(BoundFunction function, Type type, SemanticSource input, ScopeFrame<SemanticSource> frame)
        {
            var parameters = ParameterArgumentBinder.Bind(type, function.Arguments).Parameters;
            if (parameters is not [OpenExpressionParameter operation, var values])
                return;
            var collection = ValueParameter(values, frame.Current, frame);
            var item = new SemanticSource(SemanticSourceKind.Element, collection.Syntax, collection);
            var inputs = DirectionalScope<SemanticSource>.Create(type == typeof(MapOver), input, item);
            Pipeline(operation.Expression.Members, inputs.Input, frame.Derive(inputs.Arguments));
        }

        private void MappingArguments(BoundFunction function, Type type, SemanticSource input, ScopeFrame<SemanticSource> frame)
        {
            var parameters = ParameterArgumentBinder.Bind(type, function.Arguments).Parameters;
            if (parameters is not [OpenExpressionParameter operation])
                return;
            var item = new SemanticSource(SemanticSourceKind.Element, input.Syntax, input);
            var members = operation.Expression.Members.ToArray();
            var nested = type == typeof(Map) || !FunctionConstruction.IsPredicatePipeline(members, member => predicates.TryExecute(member.Name, out _));
            Pipeline(members, item, nested ? frame.Derive(item) : frame);
        }

        private static bool UsesScalarProviders(BoundFunction function, Type type)
            => FunctionConstruction.Classify(function.Name) == FunctionConstructionKind.Standard
                && !typeof(IValueSpreadAware).IsAssignableFrom(type)
                && !type.GetConstructors().Any(constructor => constructor.GetParameters().Any(parameter =>
                    parameter.ParameterType == typeof(Func<IFunction>) || parameter.ParameterType == typeof(Func<IPredicate>)));

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
            }
            foreach (var child in ParameterChildren(parameter))
                Parameter(child, input, frame);
            return Source(parameter);
        }

        private static IEnumerable<IParameter> ParameterChildren(IParameter parameter)
            => parameter switch
            {
                ArrayParameter array => array.Elements.Select(element => element.Value),
                TupleParameter tuple => tuple.Elements.Select(element => element.Value),
                VectorParameter vector => vector.Elements.Select(element => element.Value),
                RecordLiteralParameter record => record.Fields.Select(field => field.Value),
                PairParameter pair => [pair.Key, pair.Value],
                GroupingParameter grouping => grouping.Entries,
                DictionaryParameter dictionary => dictionary.Entries,
                _ => [],
            };

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
