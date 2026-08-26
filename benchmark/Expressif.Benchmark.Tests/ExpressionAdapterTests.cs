namespace Expressif.Benchmark.Tests;

[TestFixture]
public class ExpressionAdapterTests
{
    private const string ComplexTextExpression =
        "trim | lower | replace(\" \", \"-\") | substring(0, 10) | upper";
    private const string CoercionExpression =
        "trim | multiply(1.21) | round(2) | prepend(\"€\")";

    [Test]
    public void V1ComplexPipeline_ParsingSucceedsButBindingRejectsReplace()
    {
        var version = GetVersion(AdapterKind.V1);
        var adapter = ExpressionAdapter.Load(version.Directory);

        Assert.That(() => adapter.Parse(ComplexTextExpression), Throws.Nothing);
        var exception = Assert.Catch<Exception>(() => adapter.Create(ComplexTextExpression));
        Assert.That(exception!.ToString(), Does.Contain("function named 'replace'"));
    }

    [TestCase("V1")]
    [TestCase("V2")]
    public void CoercionPipeline_NumericStringReturnsFormattedText(string adapterName)
    {
        var kind = Enum.Parse<AdapterKind>(adapterName);
        var version = GetVersion(kind);
        var evaluate = ExpressionAdapter.Load(version.Directory).CreateEvaluator(CoercionExpression);

        var result = evaluate(" 1234.56 ");

        Assert.That(result, Is.TypeOf<string>());
        Assert.That(result, Is.EqualTo("€1493.82"));
    }

    private static VersionUnderTest GetVersion(AdapterKind kind)
    {
        IReadOnlyList<VersionUnderTest> versions;
        try
        {
            versions = VersionDiscovery.Discover();
        }
        catch (DirectoryNotFoundException exception)
        {
            Assert.Ignore(exception.Message);
            throw;
        }

        var version = versions.FirstOrDefault(candidate => candidate.AdapterKind == kind);
        if (version is null)
            Assert.Ignore($"No {kind.ToString().ToLowerInvariant()} version folder is available.");

        return version!;
    }
}
