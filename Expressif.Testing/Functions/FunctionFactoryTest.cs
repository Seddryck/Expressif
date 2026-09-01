using Expressif.Bindings;
using Expressif.Accumulators;
using Expressif.Functions;
using Expressif.Functions.Array;
using Expressif.Functions.Numeric;
using Expressif.Functions.Text;
using Expressif.Predicates.Numeric;
using System.Reflection;

namespace Expressif.Testing.Functions;

public class FunctionFactoryTest
{
    [Test]
    public void Instantiate_ValueSpreadAwareFunction_DispatchesByResolvedType()
    {
        var mapper = new TestTypeMapper("unrelated-name", typeof(SpreadAwareProbe));
        var function = Expressif.Bindings.Function.FromArguments("unrelated-name", [
            new FunctionArgument(null, new LiteralParameter(1)),
            new FunctionArgument(null, new ArrayParameter([
                new ArrayElementParameter(new LiteralParameter(2)),
                new ArrayElementParameter(new LiteralParameter(3)),
            ]), true),
        ]);
        var root = new OpenRootExpression(new OpenExpression([function]));

        var runtime = new FunctionFactory(mapper).Instantiate(root, new Context());

        Assert.That(runtime.Evaluate(null), Is.EqualTo(new object?[] { 1, 2, 3 }));
    }

    [TestCase("even", 4, true)]
    [TestCase("even", 5, false)]
    [TestCase("is-even", 4, true)]
    public void Instantiate_PredicateOnlyExpression_EvaluatesBoolean(string source, object value, bool expected)
        => Assert.That(Instantiate(source, new Context()).Evaluate(value), Is.EqualTo(expected));

    [Test]
    public void Instantiate_CoercingTypedPipeline_EvaluatesInsertedConversions()
    {
        var function = Instantiate("trim | multiply(1.21) | round(2) | prepend(\"€\")", new Context());

        Assert.Multiple(() =>
        {
            Assert.That(function.GetType().IsGenericType, Is.True);
            Assert.That(function.GetType().GetGenericTypeDefinition(), Is.EqualTo(typeof(ChainFunction<,>)));
            Assert.That(((IFunction<string?, string?>)function).Evaluate(" 12.345 "), Is.EqualTo("€14.94"));
        });
    }

    [Test]
    public void Instantiate_NonCoercingPipeline_UsesDynamicChainFallback()
    {
        var root = new ExpressifBinder(applyCoercion: false).Bind(
            ExpressifSyntax.Parse("trim | multiply(1.21) | round(2) | prepend(\"€\")"));
        var function = new FunctionFactory().Instantiate(root, new Context());

        Assert.Multiple(() =>
        {
            Assert.That(function, Is.TypeOf<ChainFunction>());
            Assert.That(function.Evaluate(" 12.345 "), Is.EqualTo("€14.94"));
        });
    }

    [Test]
    public void Instantiate_NumericFunctionWithInvalidDirectInput_ReturnsNull()
        => Assert.That(Instantiate("nth-root(2)", new Context()).Evaluate("A"), Is.Null);

    [Test]
    public void Instantiate_NumericFunctionWithInvalidDynamicInput_ReturnsNull()
        => Assert.That(
            Instantiate("coalesce(coerce(:integer), \"?\") | nth-root(2)", new Context()).Evaluate("A"),
            Is.Null);

    [Test]
    public void Instantiate_TemporalFunctionWithInvalidDynamicInput_ReturnsNull()
        => Assert.That(
            Instantiate("coalesce(coerce(:dateTime), \"?\") | dateTime-to-date", new Context()).Evaluate("A"),
            Is.Null);

    [Test]
    public void BuildTypedChain_InvokesTypedContractsInsteadOfObjectFallbacks()
    {
        var first = new TypedCallProbe("first");
        var second = new TypedCallProbe("second");
        var functions = new List<IFunction> { first, second };
        var members = new[]
        {
            new Expressif.Bindings.Function("first", []),
            new Expressif.Bindings.Function("second", []),
        };

        var success = FunctionFactory.TryBuildTypedChain(members, functions, out var chain);
        var result = ((IFunction<string, string>)chain!).Evaluate("value");

        Assert.Multiple(() =>
        {
            Assert.That(success, Is.True);
            Assert.That(result, Is.EqualTo("value-first-second"));
            Assert.That(first.TypedCalls, Is.EqualTo(1));
            Assert.That(first.ObjectCalls, Is.Zero);
            Assert.That(second.TypedCalls, Is.EqualTo(1));
            Assert.That(second.ObjectCalls, Is.Zero);
        });
    }

    [TestCase(4, false)]
    [TestCase(6, true)]
    [TestCase(7, false)]
    public void Instantiate_ComposedPredicateOnlyExpression_EvaluatesAgainstOriginalInput(object value, bool expected)
        => Assert.That(Instantiate("even |AND greater-than(5)", new Context()).Evaluate(value), Is.EqualTo(expected));

    [TestCase("replace-slice(2, 4, \"abc\")")]
    [TestCase("replace-slice(start := 2, length := 4, append := \"abc\")")]
    [TestCase("replace-slice(2, append := \"abc\", length := 4)")]
    [TestCase("replace-slice(append := \"abc\", start := 2, length := 4)")]
    public void Instantiate_ReplaceSliceArgumentForms_Equivalent(string source)
    {
        var function = new ExpressifBinder().BindFunction(ExpressifSyntax.Parse(source));
        var root = new OpenRootExpression(new OpenExpression([function]));
        var runtime = new FunctionFactory().Instantiate(root, new Context());

        Assert.That(runtime.Evaluate("01234567"), Is.EqualTo("01abc67"));
    }

    [TestCase("replace-slice(value := 2, length := 4, append := \"abc\")", typeof(UnknownParameterNameException))]
    [TestCase("replace-slice(2, start := 4, append := \"abc\")", typeof(PositionallySuppliedParameterException))]
    [TestCase("replace-slice(start := 2, append := \"abc\")", typeof(MissingRequiredParameterException))]
    [TestCase("replace-slice(1, 2, \"x\", 4)", typeof(TooManyPositionalArgumentsException))]
    public void Instantiate_InvalidNamedArguments_ThrowsSpecificException(string source, Type exceptionType)
    {
        var function = new ExpressifBinder().BindFunction(ExpressifSyntax.Parse(source));
        var root = new OpenRootExpression(new OpenExpression([function]));

        Assert.That(() => new FunctionFactory().Instantiate(root, new Context()), Throws.TypeOf(exceptionType));
    }
    [SetUp]
    public void Setup()
    { }

    [Test]
    [TestCase(typeof(NullToZero), 0)]
    [TestCase(typeof(Round), 1)]
    [TestCase(typeof(PadRight), 2)]
    [TestCase(typeof(Token), 1)]
    [TestCase(typeof(Token), 2)]
    public void GetMatchingConstructor_TypeAndParams_Valid(Type type, int paramCount)
    {
        var ctor = new FunctionFactory().GetMatchingConstructor(type, paramCount);
        Assert.That(ctor, Is.Not.Null);
        Assert.That(ctor.GetParameters(), Has.Length.EqualTo(paramCount));
    }

    [Test]
    [TestCase(typeof(NullToZero), 1)]
    [TestCase(typeof(Round), 2)]
    [TestCase(typeof(PadRight), 3)]
    [TestCase(typeof(Token), 0)]
    [TestCase(typeof(Token), 3)]
    public void GetMatchingConstructor_TypeAndParams_Invalid(Type type, int paramCount)
        => Assert.That(() => new FunctionFactory().GetMatchingConstructor(type, paramCount), Throws.TypeOf<MissingOrUnexpectedParametersFunctionException>());

    [Test]
    public void Instantiate_RoundLiteralParameter_Valid()
    {
        var function = new FunctionFactory().Instantiate(typeof(Round), new[] { new LiteralParameter("1") }, new Context());
        Assert.That(function, Is.Not.Null);
        Assert.That(function, Is.TypeOf<Round>());
        Assert.That((function as Round)!.Digits.Invoke(), Is.EqualTo(1));
    }

    [Test]
    public void Instantiate_RoundVariableParameter_Valid()
    {
        var context = new Context();
        context.Variables.Add<int>("myVar", 2);
        var function = new FunctionFactory().Instantiate(typeof(Round), new[] { new VariableParameter("myVar") }, context);
        Assert.That(function, Is.Not.Null);
        Assert.That(function, Is.TypeOf<Round>());
        Assert.That((function as Round)!.Digits.Invoke(), Is.EqualTo(2));
    }

    [Test]
    public void Instantiate_RoundObjectPropertyParameter_Valid()
    {
        var context = new Context();
        context.CurrentObject.Set(new { Digits = 3 });
        var function = new FunctionFactory().Instantiate(typeof(Round), new[] { new ObjectPropertyParameter("Digits") }, context);
        Assert.That(function, Is.Not.Null);
        Assert.That(function, Is.TypeOf<Round>());
        Assert.That((function as Round)!.Digits.Invoke(), Is.EqualTo(3));
    }

    [Test]
    public void Instantiate_RoundObjectIndexParameter_Valid()
    {
        var context = new Context();
        var function = new FunctionFactory().Instantiate(typeof(Round), new[] { new ObjectIndexParameter(1) }, context);
        context.CurrentObject.Set(new List<int> { 0, 4 });
        Assert.That(function, Is.Not.Null);
        Assert.That(function, Is.TypeOf<Round>());
        Assert.That(((Round)function).Digits.Invoke(), Is.EqualTo(4));
    }

    [Test]
    public void Instantiate_RoundExpressionParameter_Valid()
    {
        var context = new Context();
        var subFunction = new InputExpressionParameter(new Expressif.Bindings.ClosedExpression(new VariableParameter("myVar"), new[] { new Function("numeric-to-increment", []) }));
        var function = new FunctionFactory().Instantiate(typeof(Round), new[] { subFunction }, context);
        context.Variables.Add<int>("myVar", 4);
        Assert.That(function, Is.Not.Null);
        Assert.That(function, Is.TypeOf<Round>());
        Assert.That((function as Round)!.Digits.Invoke(), Is.EqualTo(5));
    }

    [Test]
    public void Instantiate_RoundMultipleExpressionParameter_Valid()
    {
        var context = new Context();
        var subFunction1 = new InputExpressionParameter(new Expressif.Bindings.ClosedExpression(new VariableParameter("myVar1"), new[] { new Function("numeric-to-decrement", []) }));
        var subFunction2 = new InputExpressionParameter(new Expressif.Bindings.ClosedExpression(new VariableParameter("myVar2"), new[] { new Function("numeric-to-increment", []) }));
        var subFunction3 = new InputExpressionParameter(new Expressif.Bindings.ClosedExpression(new VariableParameter("myVar1"), new[] { new Function("numeric-to-add", [subFunction1]), new Function("numeric-to-multiply", [subFunction2]) }));
        var function = new FunctionFactory().Instantiate(typeof(Round), new[] { subFunction3 }, context);
        context.Variables.Add<int>("myVar1", 4);
        context.Variables.Add<int>("myVar2", 5);
        Assert.That(function, Is.Not.Null);
        Assert.That(function, Is.TypeOf<Round>());
        Assert.That((function as Round)!.Digits.Invoke(), Is.EqualTo(42)); // (4+3)*6
    }

    [Test]
    public void Instantiate_FoldWithAccumulatorName_Valid()
    {
        var function = Instantiate("fold(sum)", new Context());
        var fold = GetSingleFunction<Fold>(function);
        var accumulator = fold.Accumulator.Invoke();

        Assert.That(fold, Is.Not.Null);
        Assert.That(accumulator, Is.TypeOf<SumAccumulator>());
    }

    [Test]
    public void Instantiate_BroadcastWithAccumulatorName_Valid()
    {
        var function = Instantiate("broadcast(sum)", new Context());
        var broadcast = GetSingleFunction<Broadcast>(function);
        var accumulator = broadcast.Accumulator.Invoke();

        Assert.That(broadcast, Is.Not.Null);
        Assert.That(accumulator, Is.TypeOf<SumAccumulator>());
    }

    [Test]
    public void Instantiate_ScanWithAccumulatorName_Valid()
    {
        var function = Instantiate("scan(sum)", new Context());
        var scan = GetSingleFunction<Scan>(function);
        var accumulator = scan.Accumulator.Invoke();

        Assert.That(scan, Is.Not.Null);
        Assert.That(accumulator, Is.TypeOf<SumAccumulator>());
    }

    [Test]
    public void Instantiate_Lag_Valid()
    {
        var function = Instantiate("lag", new Context());
        var lag = GetSingleFunction<Lag>(function);

        Assert.That(lag, Is.Not.Null);
    }

    [Test]
    public void Instantiate_Lead_Valid()
    {
        var function = Instantiate("lead", new Context());
        var lead = GetSingleFunction<Lead>(function);

        Assert.That(lead, Is.Not.Null);
    }

    [Test]
    public void Instantiate_PositionFunctions_Valid()
    {
        var withPosition = Instantiate("with-position", new Context());
        var positionOf = Instantiate("position-of(`b`)", new Context());
        var valueAt = Instantiate("value-at(1)", new Context());

        Assert.Multiple(() =>
        {
            Assert.That(GetSingleFunction<WithPosition>(withPosition), Is.Not.Null);
            Assert.That(GetSingleFunction<PositionOf>(positionOf).Value.Invoke(), Is.EqualTo("b"));
            Assert.That(GetSingleFunction<ValueAt>(valueAt).Position.Invoke(), Is.EqualTo(1));
        });
    }

    [Test]
    [TestCase("first-elements(2)", typeof(FirstElements))]
    [TestCase("first(2)", typeof(FirstElements))]
    [TestCase("last(2)", typeof(LastElements))]
    [TestCase("skip-first(2)", typeof(SkipFirstElements))]
    [TestCase("skip-last(2)", typeof(SkipLastElements))]
    public void Instantiate_ArraySelectionAliases_Valid(string expression, Type expectedType)
    {
        var function = Instantiate(expression, new Context());

        var selectionFunction = GetSingleFunction(function, expectedType);
        Assert.That(selectionFunction, Is.Not.Null);

        var countFactory = selectionFunction.GetType().GetProperty("Count")?.GetValue(selectionFunction) as Func<int>
                                ?? throw new InvalidOperationException($"Could not resolve 'Count' parameter for function type '{expectedType.Name}'.");
        var count = countFactory.Invoke();

        Assert.That(count, Is.EqualTo(2));
    }

    [Test]
    public void Instantiate_SliceElements_Valid()
    {
        var function = Instantiate("slice-elements(1,4)", new Context());
        var sliceElements = GetSingleFunction<SliceElements>(function);

        Assert.That(sliceElements, Is.Not.Null);
        Assert.That(sliceElements.Start.Invoke(), Is.EqualTo(1));
        Assert.That(sliceElements.End.Invoke(), Is.EqualTo(4));
    }

    [Test]
    public void Instantiate_MapWithOpenExpressionParameter_Valid()
    {
        var function = Instantiate("map(lower | trim)", new Context());
        var map = GetSingleFunction<Map>(function);
        var transformation = map.Transformation.Invoke();
        var transformationFunctions = GetFunctions(transformation).ToArray();

        Assert.That(map, Is.Not.Null);
        Assert.That(transformation, Is.InstanceOf<ChainFunction>());
        Assert.That(transformationFunctions, Has.Length.EqualTo(2));
        Assert.That(transformationFunctions[0], Is.TypeOf<Lower>());
        Assert.That(transformationFunctions[1], Is.TypeOf<Trim>());
    }

    [Test]
    public void Instantiate_FilterWithPredicateExpression_Valid()
    {
        var function = Instantiate("filter(greater-than(2))", new Context());
        var filter = GetSingleFunction<Filter>(function);
        var predicate = filter.Predicate.Invoke();

        Assert.That(filter, Is.Not.Null);
        Assert.That(predicate, Is.TypeOf<GreaterThan>());
    }

    [Test]
    public void Instantiate_FilterWithClosedPredicateParameter_ResolvesNestedFunction()
    {
        var reference = new InputExpressionParameter(
            new Expressif.Bindings.ClosedExpression(
                new LiteralParameter(17m),
                [new Function("add", [new LiteralParameter(17m)])]));
        var predicate = new Function("greater-than", [reference]);
        var filter = new Function(
            "filter",
            [new OpenExpressionParameter(new OpenExpression([predicate]))]);
        var root = new OpenRootExpression(new OpenExpression([filter]));

        var function = new FunctionFactory().Instantiate(root, new Context());

        Assert.That(function.Evaluate(new object[] { 10, 12, 13 }), Is.Empty);
    }

    [Test]
    public void Instantiate_SliceAlias_Valid()
    {
        var function = Instantiate("slice(1,4)", new Context());
        var sliceElements = GetSingleFunction<SliceElements>(function);

        Assert.That(sliceElements, Is.Not.Null);
        Assert.That(sliceElements.Start.Invoke(), Is.EqualTo(1));
        Assert.That(sliceElements.End.Invoke(), Is.EqualTo(4));
    }

    [Test]
    public void Instantiate_FunctionWithFuncStringConstructor_NotTreatedAsAggregation()
    {
        var function = Instantiate("prefix(`abc`)", new Context());
        var prefix = GetSingleFunction<Prefix>(function);

        Assert.That(prefix, Is.Not.Null);
        Assert.That(prefix.Append.Invoke(), Is.EqualTo("abc"));
    }

    private static T GetSingleFunction<T>(IFunction function)
        where T : class, IFunction
    {
        var property = typeof(ChainFunction).GetProperty("Functions", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Could not locate ChainFunction.Functions property.");
        var functions = property.GetValue(function) as IEnumerable<IFunction>
            ?? throw new InvalidOperationException("Could not read ChainFunction functions.");

        return functions.Single() as T
            ?? throw new InvalidOperationException($"Could not find function of type '{typeof(T).Name}'.");
    }

    private static IFunction Instantiate(string source, IContext context)
        => new FunctionFactory().Instantiate(
            new ExpressifBinder().Bind(ExpressifSyntax.Parse(source)),
            context);

    private static IFunction GetSingleFunction(IFunction function, Type expectedType)
    {
        return GetFunctions(function).SingleOrDefault(expectedType.IsInstanceOfType)
            ?? throw new InvalidOperationException($"Could not find function of type '{expectedType.Name}'.");
    }

    private static IEnumerable<IFunction> GetFunctions(IFunction function)
    {
        var property = typeof(ChainFunction).GetProperty("Functions", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Could not locate ChainFunction.Functions property.");
        return property.GetValue(function) as IEnumerable<IFunction>
            ?? throw new InvalidOperationException("Could not read ChainFunction functions.");
    }

    private sealed class TypedCallProbe(string suffix) : IFunction<string, string>
    {
        public int TypedCalls { get; private set; }
        public int ObjectCalls { get; private set; }

        public string Evaluate(string value)
        {
            TypedCalls++;
            return $"{value}-{suffix}";
        }

        object? IFunction.Evaluate(object? value)
        {
            ObjectCalls++;
            return $"{value}-object";
        }
    }

    private sealed class SpreadAwareProbe(Func<ValueArgumentEvaluator[]> arguments)
        : IFunction, IValueSpreadAware
    {
        public object? Evaluate(object? value)
            => ValueArguments.Evaluate(arguments.Invoke(), value).ToArray();
    }

    private sealed class TestTypeMapper(string name, Type type) : BaseTypeMapper
    {
        protected override IDictionary<string, Type> Initialize()
            => new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase) { [name] = type };
    }
}
