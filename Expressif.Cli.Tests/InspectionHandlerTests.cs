using Expressif.Bindings;
using Expressif.Cli.Application;
using Expressif.Cli.Expressions;
using Expressif.Functions.Catalog;
using Expressif.Syntax;

namespace Expressif.Cli.Tests;

public class InspectionHandlerTests
{
    [Test]
    public void ParseHandler_DelegatesToSyntaxService()
    {
        var syntax = new FakeSyntaxService();
        var request = new ParseRequest("add(2)", TreeOutputFormat.Json);

        var result = new ParseHandler(syntax).Execute(request);

        Assert.Multiple(() =>
        {
            Assert.That(syntax.ParsedCode, Is.EqualTo(request.Expression));
            Assert.That(result, Is.SameAs(syntax.ParsedSyntax));
        });
    }

    [Test]
    public void BindHandler_ParsesBindsAndValidatesInOrder()
    {
        var syntax = new FakeSyntaxService();

        var result = new BindHandler(syntax).Execute(new BindRequest("add(2)", TreeOutputFormat.Tree));

        Assert.Multiple(() =>
        {
            Assert.That(syntax.Calls, Is.EqualTo(new[] { "parse", "bind", "validate" }));
            Assert.That(result, Is.SameAs(syntax.BoundExpression));
        });
    }

    [TestCase(null, false, null)]
    [TestCase("add", true, null)]
    [TestCase("add", false, "Numeric")]
    [TestCase(null, true, "Numeric")]
    public void HelpRequest_InvalidModeCombinations_AreRejected(string? function, bool list, string? scope)
        => Assert.That(HelpRequest.TryCreate(function, list, scope, out _), Is.False);

    [TestCase("add", false, null, "Function")]
    [TestCase(null, true, null, "List")]
    [TestCase(null, false, "Numeric", "Scope")]
    public void HelpRequest_ValidMode_IsNormalized(
        string? function,
        bool list,
        string? scope,
        string expected)
    {
        var success = HelpRequest.TryCreate(function, list, scope, out var request);

        Assert.Multiple(() =>
        {
            Assert.That(success, Is.True);
            Assert.That(request!.Mode.ToString(), Is.EqualTo(expected));
        });
    }

    [Test]
    public void HelpHandler_UnknownFunction_ReturnsSuggestions()
    {
        var catalog = new FakeFunctionCatalog();

        var result = new HelpHandler(catalog).Execute(new HelpRequest(HelpMode.Function, "ad"));

        Assert.That(result, Is.TypeOf<UnknownFunctionHelpResult>());
        var unknown = (UnknownFunctionHelpResult)result;
        Assert.Multiple(() =>
        {
            Assert.That(unknown.Name, Is.EqualTo("ad"));
            Assert.That(unknown.Suggestions, Is.EqualTo(new[] { "add" }));
        });
    }

    [Test]
    public void HelpHandler_Scope_ReturnsMatchingFunctions()
    {
        var catalog = new FakeFunctionCatalog();

        var result = new HelpHandler(catalog).Execute(new HelpRequest(HelpMode.Scope, "Numeric"));

        Assert.That(result, Is.TypeOf<FunctionListHelpResult>());
        Assert.That(((FunctionListHelpResult)result).Functions.Select(x => x.Name), Is.EqualTo(new[] { "add" }));
    }

    [Test]
    public void HelpHandler_UnknownScope_ReturnsTypedFailure()
    {
        var result = new HelpHandler(new FakeFunctionCatalog())
            .Execute(new HelpRequest(HelpMode.Scope, "Missing"));

        Assert.That(result, Is.EqualTo(new UnknownScopeHelpResult("Missing")));
    }

    [Test]
    public void HelpHandler_List_ReturnsCatalogFunctions()
    {
        var catalog = new FakeFunctionCatalog();

        var result = new HelpHandler(catalog).Execute(new HelpRequest(HelpMode.List, null));

        Assert.That(result, Is.TypeOf<FunctionListHelpResult>());
        Assert.That(((FunctionListHelpResult)result).Functions.Select(x => x.Name), Is.EqualTo(new[] { "add" }));
    }

    private sealed class FakeSyntaxService : ISyntaxService
    {
        private readonly SyntaxService inner = new();

        public FakeSyntaxService()
        {
            ParsedSyntax = inner.Parse("add(2)");
            BoundExpression = inner.Bind(ParsedSyntax);
        }

        public List<string> Calls { get; } = [];

        public string? ParsedCode { get; private set; }

        public RootExpressionSyntax ParsedSyntax { get; }

        public IRootExpression BoundExpression { get; }

        public RootExpressionSyntax Parse(string code)
        {
            Calls.Add("parse");
            ParsedCode = code;
            return ParsedSyntax;
        }

        public IRootExpression Bind(RootExpressionSyntax syntax)
        {
            Calls.Add("bind");
            return BoundExpression;
        }

        public void Validate(IRootExpression expression, Context context) => Calls.Add("validate");
    }

    private sealed class FakeFunctionCatalog : IFunctionCatalogService
    {
        private static readonly FunctionDocumentation Add = new(
            "add", true, ["add"], "Numeric", "numeric", "numeric", "Adds a value.", []);

        public IReadOnlyList<FunctionDocumentation> Functions => [Add];

        public FunctionDocumentation? Find(string name) => name == Add.Name ? Add : null;

        public IEnumerable<FunctionDocumentation> ForScope(string scope)
            => string.Equals(scope, Add.Scope, StringComparison.OrdinalIgnoreCase) ? [Add] : [];

        public IEnumerable<FunctionDocumentation> Suggest(string name) => [Add];
    }
}
