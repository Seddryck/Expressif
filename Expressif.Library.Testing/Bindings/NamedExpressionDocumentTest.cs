using Expressif.Bindings;
using Expressif.Library.Composition;
using Expressif.Planning;
using Expressif.Syntax;
namespace Expressif.Testing.Bindings;

public sealed class NamedExpressionDocumentTest
{
    [Test]
    public void Bind_DefinitionAndEntry_InvokesAgainstCurrentPipelineValue()
    {
        var definition = Definition("short-name", "upper | first-chars(3)");
        var plan = Document(
            [definition],
            [new LogicalLiteral("text", "alphabet"), Invoke("short-name")]);

        var expression = ExpressionBinder.Bind(RoundTrip(plan));

        Assert.That(expression.Evaluate(null), Is.EqualTo("ALP"));
    }

    [Test]
    public void Bind_DefinitionOnlyDocument_ExposesReusableLibraryWithoutImplicitExecution()
    {
        var plan = Document([Definition("short-name", "upper | first-chars(3)")], []);

        var document = (NamedExpressionDocument)ExpressionBinder.Bind(RoundTrip(plan));

        Assert.Multiple(() =>
        {
            Assert.That(document.HasEntry, Is.False);
            Assert.That(document.Evaluate("unchanged"), Is.EqualTo("unchanged"));
            Assert.That(document.Invoke("short-name", "alphabet"), Is.EqualTo("ALP"));
        });
    }

    [Test]
    public void Bind_ComposedForwardReference_UsesLexicalDefinitionGraph()
    {
        var shortName = new LogicalNamedExpressionDefinition(
            "short-name",
            new LogicalPipeline([Invoke("normalize"), .. Pipeline("first-chars(3)").Items]));
        var normalize = Definition("normalize", "trim | upper");
        var plan = Document(
            [shortName, normalize],
            [new LogicalLiteral("text", " alphabet "), Invoke("short-name")]);

        var expression = ExpressionBinder.Bind(RoundTrip(plan));

        Assert.That(expression.Evaluate(null), Is.EqualTo("ALP"));
    }

    [Test]
    public void Bind_Definition_DoesNotCaptureCallerEvaluationFrame()
    {
        var definition = Definition("outer-secret", "^^.secret");
        var plan = Document(
            [definition],
            [.. Pipeline("record(secret := \"caller\")").Items, Invoke("outer-secret")]);

        var expression = ExpressionBinder.Bind(RoundTrip(plan));

        var exception = Assert.Catch<Exception>(() => expression.Evaluate(null));

        Assert.That(exception!.ToString(), Does.Contain("object of type 'null'").And.Contain("secret"));
    }

    [Test]
    public void Serialize_RecursiveDefinitions_ReportsDependencyChain()
    {
        var plan = Document(
            [
                new LogicalNamedExpressionDefinition("first", new LogicalPipeline([Invoke("second")])),
                new LogicalNamedExpressionDefinition("second", new LogicalPipeline([Invoke("first")])),
            ],
            []);

        var exception = Assert.Throws<LogicalPlanFormatException>(() => LogicalPlanJson.Serialize(plan));

        Assert.That(exception!.Message, Does.Contain("first -> second -> first"));
    }

    [Test]
    public void Bind_RequiredParameter_IsEvaluatedInCallerScope()
    {
        var definition = Definition(
            "short-name",
            "upper | first-chars(@length)",
            [new LogicalNamedExpressionParameter("length")]);
        var plan = Document(
            [definition],
            [new LogicalLiteral("text", "alphabet"), Invoke("short-name", new LogicalLiteral("integer", 5))]);

        var expression = ExpressionBinder.Bind(RoundTrip(plan));

        Assert.That(expression.Evaluate(null), Is.EqualTo("ALPHA"));
    }

    [Test]
    public void Bind_OmittedParameter_UsesLiteralDefault()
    {
        var definition = Definition(
            "short-name",
            "upper | first-chars(@length)",
            [new LogicalNamedExpressionParameter("length", new LogicalLiteral("integer", 3))]);
        var plan = Document(
            [definition],
            [new LogicalLiteral("text", "alphabet"), Invoke("short-name")]);

        Assert.That(ExpressionBinder.Bind(RoundTrip(plan)).Evaluate(null), Is.EqualTo("ALP"));
    }

    [Test]
    public void Bind_ExplicitNullArgument_DoesNotUseDefault()
    {
        var definition = Definition(
            "choose",
            "coalesce(@value, \"fallback\")",
            [new LogicalNamedExpressionParameter("value", new LogicalLiteral("text", "default"))]);
        var omitted = Document([definition], [Invoke("choose")]);
        var explicitNull = Document(
            [definition],
            [Invoke("choose", new LogicalLiteral("null", null))]);

        Assert.Multiple(() =>
        {
            Assert.That(ExpressionBinder.Bind(RoundTrip(omitted)).Evaluate(null), Is.EqualTo("default"));
            Assert.That(ExpressionBinder.Bind(RoundTrip(explicitNull)).Evaluate(null), Is.EqualTo("fallback"));
        });
    }

    [Test]
    public void Serialize_InvalidArgumentCount_ReportsDefinitionAndExpectedRange()
    {
        var definition = Definition(
            "short-name",
            "identity",
            [new LogicalNamedExpressionParameter("required")]);
        var plan = Document([definition], [Invoke("short-name")]);

        var exception = Assert.Throws<LogicalPlanFormatException>(() => LogicalPlanJson.Serialize(plan));

        Assert.That(exception!.Message,
            Does.Contain("short-name").And.Contain("between 1 and 1").And.Contain("received 0"));
    }

    [Test]
    public void Bind_CoercingParameterContract_ConvertsBeforeBodyEvaluation()
    {
        var definition = Definition(
            "short-name",
            "first-chars(@length)",
            [new LogicalNamedExpressionParameter(
                "length",
                Contract: new LogicalTypeContract("integer"))]);
        var plan = Document(
            [definition],
            [new LogicalLiteral("text", "alphabet"), Invoke("short-name", new LogicalLiteral("text", "5"))]);

        Assert.That(ExpressionBinder.Bind(RoundTrip(plan)).Evaluate(null), Is.EqualTo("alpha"));
    }

    [Test]
    public void Bind_StrictParameterContract_RejectsConvertibleValue()
    {
        var definition = Definition(
            "short-name",
            "first-chars(@length)",
            [new LogicalNamedExpressionParameter(
                "length",
                Contract: new LogicalTypeContract("integer", Strict: true))]);
        var plan = Document(
            [definition],
            [new LogicalLiteral("text", "alphabet"), Invoke("short-name", new LogicalLiteral("text", "5"))]);
        var expression = ExpressionBinder.Bind(RoundTrip(plan));

        var exception = Assert.Throws<InvalidOperationException>(() => expression.Evaluate(null));

        Assert.That(exception!.Message, Does.Contain("short-name").And.Contain("length").And.Contain(":integer"));
    }

    [Test]
    public void Bind_InputAndOutputContracts_CoerceAtDefinitionBoundaries()
    {
        var definition = new LogicalNamedExpressionDefinition(
            "convert",
            Pipeline("identity"),
            InputContract: new LogicalTypeContract("integer"),
            OutputContract: new LogicalTypeContract("text"));
        var plan = Document(
            [definition],
            [new LogicalLiteral("text", "42"), Invoke("convert")]);

        Assert.That(ExpressionBinder.Bind(RoundTrip(plan)).Evaluate(null), Is.EqualTo("42"));
    }

    [Test]
    public void Bind_TupleReceiver_DecomposesInputIndependentlyFromParameters()
    {
        var definition = new LogicalNamedExpressionDefinition(
            "short-name",
            Pipeline("@text | first-chars(@length)"),
            Receivers:
            [
                new LogicalNamedExpressionReceiver("text", new LogicalTypeContract("text", Strict: true)),
                new LogicalNamedExpressionReceiver("length", new LogicalTypeContract("decimal", Strict: true)),
            ]);
        var plan = Document([definition], [.. Pipeline("T(\"alphabet\", 3)").Items, Invoke("short-name")]);

        Assert.That(ExpressionBinder.Bind(RoundTrip(plan)).Evaluate(null), Is.EqualTo("alp"));
    }

    [Test]
    public void Serialize_ReceiverAndParameterNameCollision_IsRejected()
    {
        var definition = new LogicalNamedExpressionDefinition(
            "invalid",
            Pipeline("identity"),
            [new LogicalNamedExpressionParameter("value")],
            [new LogicalNamedExpressionReceiver("value")]);

        var exception = Assert.Throws<LogicalPlanFormatException>(() => LogicalPlanJson.Serialize(
            Document([definition], [])));

        Assert.That(exception!.Message, Does.Contain("value").And.Contain("more than once"));
    }

    private static LogicalNamedExpressionDefinition Definition(
        string name,
        string body,
        IReadOnlyList<LogicalNamedExpressionParameter>? parameters = null)
        => new(name, Pipeline(body), parameters);

    private static LogicalNamedExpressionInvocation Invoke(string name, params LogicalValue[] arguments)
        => new(name, arguments);

    private static LogicalPipeline Pipeline(string source)
        => LogicalPlannerFactory.Create().Build(ExpressionParser.Parse(source)).Pipeline;

    private static LogicalPlan Document(
        IReadOnlyList<LogicalNamedExpressionDefinition> definitions,
        IReadOnlyList<LogicalValue> entry)
        => new(new LogicalPipeline(entry)) { Definitions = definitions };

    private static LogicalPlan RoundTrip(LogicalPlan plan)
        => LogicalPlanJson.Deserialize(LogicalPlanJson.Serialize(plan));
}
