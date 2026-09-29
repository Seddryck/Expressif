using Expressif.Functions;
using Expressif.Library.Record;
using Expressif.Values;
using RecordFunction = Expressif.Library.Record.Record;

namespace Expressif.Testing.Composition;

public sealed class RecordFunctionConstructorTest
{
    [Test]
    public void ConstructRejectsNamedFunctionArguments()
    {
        var function = Function.FromArguments("record", [new FunctionArgument("unexpected", new LiteralParameter(1))]);

        Assert.That(
            () => Construct(function),
            Throws.TypeOf<UnknownParameterNameException>());
    }

    [Test]
    public void ConstructReturnsEmptyRecordForNoParameters()
        => Assert.That(Construct(new Function("record", [])), Is.TypeOf<RecordFunction>());

    [Test]
    public void ConstructRejectsInvalidParameterShape()
        => Assert.That(
            () => Construct(new Function("record", [new LiteralParameter(1)])),
            Throws.TypeOf<MissingOrUnexpectedParametersFunctionException>());

    [Test]
    public void ConstructRejectsDuplicateExplicitFields()
    {
        var definition = new RecordDefinitionParameter([
            new RecordNamedEntry("name", new LiteralParameter("first")),
            new RecordNamedEntry("name", new LiteralParameter("second")),
        ]);

        Assert.That(
            () => Construct(new Function("record", [definition])),
            Throws.TypeOf<BindingException>().With.Message.Contains("Duplicate explicit field 'name'"));
    }

    [Test]
    public void ConstructBuildsEvaluatorsForCoreParameterKinds()
    {
        var definition = new RecordDefinitionParameter([
            new RecordNamedEntry("incoming", new IncomingValueParameter()),
            new RecordNamedEntry("quoted", new QuotedLiteralParameter("quoted-value")),
            new RecordNamedEntry("literal", new LiteralParameter(42)),
            new RecordNamedEntry("provided", new ContextParameter(_ => "provided-value")),
            new RecordSpreadEntry(new IncomingValueParameter()),
        ]);
        var construction = new Mock<IFunctionConstructionContext>();
        construction.Setup(candidate => candidate.CreateParameter(
                It.IsAny<IParameter>(), typeof(object), It.IsAny<IContext>()))
            .Returns(new Func<object?>(() => "provided-value"));

        var runtime = (RecordFunction)Construct(new Function("record", [definition]), construction);
        var input = new Dictionary<string, object?> { ["spread"] = "spread-value" };
        var result = (RecordValue)runtime.Evaluate(input)!;

        Assert.Multiple(() =>
        {
            Assert.That(result["incoming"], Is.SameAs(input));
            Assert.That(result["quoted"], Is.EqualTo("quoted-value"));
            Assert.That(result["literal"], Is.EqualTo(42));
            Assert.That(result["provided"], Is.EqualTo("provided-value"));
            Assert.That(result["spread"], Is.EqualTo("spread-value"));
        });
    }

    [TestCase("42", 42)]
    [TestCase("true", true)]
    public void ConstructParsesSingleTypedToken(string token, object expected)
    {
        var definition = Definition("value", new OpenExpressionParameter(new OpenExpression([
            new Function(token, []),
        ])));

        var result = (RecordValue)((RecordFunction)Construct(new Function("record", [definition]))).Evaluate(null)!;

        Assert.That(result["value"], Is.EqualTo(expected));
    }

    [Test]
    public void ConstructTreatsUnknownSingleTokenAsLiteralWhenRuntimeResolutionFails()
    {
        var definition = Definition("value", new OpenExpressionParameter(new OpenExpression([
            new Function("unregistered-token", []),
        ])));
        var construction = new Mock<IFunctionConstructionContext>();
        construction.Setup(candidate => candidate.CreateOpenExpression(It.IsAny<OpenExpression>(), It.IsAny<IContext>()))
            .Throws(new NotImplementedFunctionException("unregistered-token"));

        var result = (RecordValue)((RecordFunction)Construct(new Function("record", [definition]), construction)).Evaluate(null)!;

        Assert.That(result["value"], Is.EqualTo("unregistered-token"));
    }

    [Test]
    public void ConstructUsesInputBoundExpressionWithoutReplacingItsContext()
    {
        var inputBound = new InputBoundExpression([], false, new OpenRootExpression(new OpenExpression([])));
        var definition = Definition("value", new OpenExpressionParameter(inputBound));
        var construction = new Mock<IFunctionConstructionContext>();
        construction.Setup(candidate => candidate.CreateOpenExpression(inputBound, It.IsAny<IContext>()))
            .Returns(new EchoFunction());
        var input = new object();

        var result = (RecordValue)((RecordFunction)Construct(new Function("record", [definition]), construction)).Evaluate(input)!;

        Assert.That(result["value"], Is.SameAs(input));
    }

    [Test]
    public void ConstructRejectsUnsupportedDefinitionEntry()
    {
        var definition = new RecordDefinitionParameter([new UnsupportedEntry()]);

        Assert.That(
            () => Construct(new Function("record", [definition])),
            Throws.TypeOf<BindingException>().With.Message.Contains("Unsupported entry type"));
    }

    private static RecordDefinitionParameter Definition(string name, IParameter value)
        => new([new RecordNamedEntry(name, value)]);

    private static IFunction Construct(
        Function function,
        Mock<IFunctionConstructionContext>? construction = null)
        => new RecordFunctionConstructor().Construct(
            function,
            new Context(),
            construction?.Object ?? Mock.Of<IFunctionConstructionContext>());

    private sealed class EchoFunction : IFunction
    {
        public object? Evaluate(object? value) => value;
    }

    private sealed class UnsupportedEntry : IRecordDefinitionEntry;
}
