using System.Reflection;
using Expressif.Bindings;
using Expressif.Discovery;
using Expressif.Functions;
using Expressif.Functions.Accumulation;
using Expressif.Predicates;
using ReflectionParameterInfo = System.Reflection.ParameterInfo;

namespace Expressif.Testing.Functions;

public class FunctionInfrastructureApiTest
{
    private static readonly string[] InternalInfrastructureTypes =
    [
        "Expressif.Functions.FunctionRegistry",
        "Expressif.Predicates.PredicateRegistry",
        "Expressif.Functions.Accumulation.AccumulatorRegistry",
        "Expressif.Functions.FunctionConstructorRegistry",
        "Expressif.Functions.BaseExpressionFactory",
        "Expressif.Functions.IFunctionConstructor",
        "Expressif.Functions.IFunctionConstructor`1",
        "Expressif.Functions.IFunctionConstructionContext",
        "Expressif.Functions.IValueConverter",
        "Expressif.Functions.IPredicationFactory",
        "Expressif.Functions.ITupleFunctionInvoker",
        "Expressif.Functions.DelegatedFunction",
        "Expressif.Functions.ChainFunction",
        "Expressif.Functions.ChainFunction`2",
        "Expressif.Functions.LexicallyBoundContextFunction",
        "Expressif.Functions.IInputBoundFunction",
        "Expressif.Functions.ValueArgumentEvaluator",
        "Expressif.Functions.ValueArguments",
    ];

    [TestCaseSource(nameof(InternalInfrastructureTypes))]
    public void InfrastructureType_IsNotPublic(string typeName)
    {
        var type = typeof(FunctionFactory).Assembly.GetType(typeName);

        Assert.That(type, Is.Not.Null);
        Assert.That(type!.IsPublic, Is.False);
    }

    [Test]
    public void ValueSpreadMarker_IsRemoved()
        => Assert.That(
            typeof(FunctionFactory).Assembly.GetType("Expressif.Functions.IValueSpreadAware"),
            Is.Null);

    [Test]
    public void FunctionFactory_PublicSurfaceIsNarrowFacade()
    {
        var constructors = typeof(FunctionFactory).GetConstructors();
        var methods = typeof(FunctionFactory)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

        Assert.Multiple(() =>
        {
            Assert.That(typeof(FunctionFactory).IsSealed, Is.True);
            Assert.That(typeof(FunctionFactory).BaseType, Is.EqualTo(typeof(object)));
            Assert.That(constructors, Has.Length.EqualTo(1));
            Assert.That(constructors[0].GetParameters().Select(parameter => parameter.ParameterType),
                Is.EqualTo(new[] { typeof(ITypeSource) }));
            Assert.That(methods.Select(method => method.Name),
                Is.EquivalentTo(new[] { nameof(FunctionFactory.Instantiate), nameof(FunctionFactory.InstantiateClosed) }));
            Assert.That(methods, Has.All.Matches<MethodInfo>(method =>
                method.GetParameters().Select(parameter => parameter.ParameterType)
                    .SequenceEqual(new[] { typeof(IRootExpression), typeof(IContext) })));
        });
    }

    [Test]
    public void AuthoringContracts_RemainPublic()
    {
        var contracts = new[]
        {
            typeof(IFunction),
            typeof(IFunction<,>),
            typeof(Function<,>),
            typeof(IPredicate),
            typeof(IPredicate<>),
            typeof(BasePredicate),
            typeof(IAccumulator),
            typeof(FunctionAttribute),
            typeof(PredicateAttribute),
            typeof(ScopeAttribute),
            typeof(FunctionLifecycleAttribute),
            typeof(FunctionAliasLifecycleAttribute),
        };

        Assert.That(contracts, Has.All.Matches<Type>(type => type.IsPublic));
    }

    [Test]
    public void PredicateAttribute_IsSealedWithOneOptionalConstructor()
    {
        var constructors = typeof(PredicateAttribute).GetConstructors();

        Assert.Multiple(() =>
        {
            Assert.That(typeof(PredicateAttribute).IsSealed, Is.True);
            Assert.That(constructors, Has.Length.EqualTo(1));
            Assert.That(constructors[0].GetParameters(), Has.All.Matches<ReflectionParameterInfo>(parameter => parameter.IsOptional));
        });
    }

    [Test]
    public void CustomTypeSource_DiscoversAuthoredFunctionAndPredicate()
    {
        var source = new CompositeTypeSource(
            TestExpression.LibraryTypeSource,
            typeof(SpreadFunction),
            typeof(PositivePredicate));
        var factory = new FunctionFactory(source);
        var spread = Expressif.Bindings.Function.FromArguments("spread-function", [
            new FunctionArgument(null, new LiteralParameter(1)),
            new FunctionArgument(null, new ArrayParameter([
                new ArrayElementParameter(new LiteralParameter(2)),
                new ArrayElementParameter(new LiteralParameter(3)),
            ]), true),
        ]);

        var spreadRuntime = factory.Instantiate(
            new OpenRootExpression(new OpenExpression([spread])),
            new Context());
        var predicateRuntime = factory.Instantiate(
            new OpenRootExpression(new OpenExpression([new Expressif.Bindings.Function("positive-predicate", [])])),
            new Context());

        Assert.Multiple(() =>
        {
            Assert.That(spreadRuntime.Evaluate(null), Is.EqualTo(new object?[] { 1, 2, 3 }));
            Assert.That(predicateRuntime.Evaluate(1), Is.True);
            Assert.That(predicateRuntime.Evaluate(-1), Is.False);
        });
    }

    private sealed class CompositeTypeSource(ITypeSource builtIns, params Type[] extensions) : ITypeSource
    {
        public IEnumerable<Type> GetTypes() => builtIns.GetTypes().Concat(extensions);
    }

    [Function(Name = "spread-function", SupportsValueSpread = true)]
    private sealed class SpreadFunction(Func<object?, object?[]> arguments) : IFunction
    {
        public object? Evaluate(object? value) => arguments.Invoke(value);
    }

    [Predicate(appendIs: false, prefix: "", name: "positive-predicate")]
    private sealed class PositivePredicate : BasePredicate
    {
        public override bool Evaluate(object? value) => value is int number && number > 0;
    }
}
