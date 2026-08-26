using BenchmarkDotNet.Attributes;

namespace Expressif.Benchmark;

public class ExpressionBenchmarks
{
    private const string ComplexTextExpression =
        "trim | lower | replace-chars(\" \", \"-\") | first-chars(10) | upper";
    private const string CoercionExpression =
        "trim | multiply(1.21) | round(2) | prepend(\"€\")";

    private ExpressionAdapter adapter = null!;
    private Func<object?, object?> simpleEvaluator = null!;
    private Func<object?, object?> textEvaluator = null!;
    private Func<object?, object?> coercionEvaluator = null!;

    [GlobalSetup(Target = nameof(ParseComplexPipeline))]
    public void SetupComplexParsing() => SetupAdapter();

    [GlobalSetup(Target = nameof(ParseCoercionPipeline))]
    public void SetupCoercionParsing() => SetupAdapter();

    [GlobalSetup(Target = nameof(ParseAndBindComplexPipeline))]
    public void SetupConstruction() => SetupAdapter();

    private void SetupAdapter()
    {
        var versionDirectory = Environment.GetEnvironmentVariable(
            VersionDiscovery.VersionEnvironmentVariable)
            ?? throw new InvalidOperationException("The benchmark version environment variable was not set.");
        adapter = ExpressionAdapter.Load(versionDirectory);
    }

    [GlobalSetup(Target = nameof(EvaluateSimple))]
    public void SetupSimple()
    {
        SetupAdapter();
        simpleEvaluator = adapter.CreateEvaluator("add(5)");
    }

    [GlobalSetup(Target = nameof(EvaluateTextPipeline))]
    public void SetupTextPipeline()
    {
        SetupAdapter();
        textEvaluator = adapter.CreateEvaluator(ComplexTextExpression);
    }

    [GlobalSetup(Target = nameof(EvaluateCoercionPipeline))]
    public void SetupCoercionPipeline()
    {
        SetupAdapter();
        coercionEvaluator = adapter.CreateEvaluator(CoercionExpression);
    }

    [Benchmark(Description = "Parse complex text pipeline")]
    public object ParseComplexPipeline() => adapter.Parse(ComplexTextExpression);

    [Benchmark(Description = "Parse implicit-coercion pipeline")]
    public object ParseCoercionPipeline() => adapter.Parse(CoercionExpression);

    [Benchmark(Description = "Parse and bind complex pipeline")]
    public object ParseAndBindComplexPipeline() => adapter.Create(ComplexTextExpression);

    [Benchmark(Description = "Evaluate add(5)")]
    public object? EvaluateSimple() => simpleEvaluator(1234.56m);

    [Benchmark(Description = "Evaluate text-only pipeline")]
    public object? EvaluateTextPipeline() => textEvaluator("  Benchmark Input Value  ");

    [Benchmark(Description = "Evaluate pipeline with implicit coercion")]
    public object? EvaluateCoercionPipeline() => coercionEvaluator(" 1234.56 ");
}
