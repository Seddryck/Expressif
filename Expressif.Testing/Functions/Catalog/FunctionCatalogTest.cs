using Expressif.Functions.Catalog;
using Expressif.Functions.Introspection;

namespace Expressif.Testing.Functions.Catalog;

[TestFixture]
public class FunctionCatalogTest
{
    [Test]
    public void Default_ExpressifAssembly_ContainsEmbeddedCatalog()
    {
        var assembly = typeof(FunctionCatalog).Assembly;

        Assert.That(assembly.GetManifestResourceNames(), Does.Contain(FunctionCatalog.ResourceName));
        Assert.That(FunctionCatalog.Default.Functions, Is.Not.Empty);
    }

    [Test]
    public void Default_PublicFunctions_MatchIntrospectionMetadata()
    {
        var documented = FunctionCatalog.Default.Functions.ToDictionary(x => x.Name);
        var introspected = new FunctionIntrospector().Describe().Where(x => x.IsPublic).ToDictionary(x => x.Name);

        Assert.That(documented.Keys, Is.EquivalentTo(introspected.Keys));
        foreach (var (name, implementation) in introspected)
        {
            var documentation = documented[name];
            using (Assert.EnterMultipleScope())
            {
                Assert.That(documentation.Aliases, Is.EquivalentTo(implementation.Aliases), $"Aliases for {name}");
                Assert.That(documentation.Scope, Is.EqualTo(implementation.Scope), $"Scope for {name}");
                Assert.That(documentation.Input, Is.EqualTo(implementation.Input), $"Input for {name}");
                Assert.That(documentation.Output, Is.EqualTo(implementation.Output), $"Output for {name}");
                Assert.That(documentation.Summary, Is.Not.Empty, $"Summary for {name}");
                Assert.That(documentation.Parameters.Select(x => (x.Name, x.Type, x.Optional)),
                    Is.EqualTo(implementation.Parameters.Select(x => (x.Name, x.Type, x.Optional))),
                    $"Parameters for {name}");
                Assert.That(documentation.Parameters.Select(x => x.Summary), Is.All.Not.Empty, $"Parameter summaries for {name}");
            }
        }
    }

    [Test]
    public void Find_Alias_ReturnsCanonicalFunction()
        => Assert.That(FunctionCatalog.Default.Find("array-to-broadcast")?.Name, Is.EqualTo("broadcast"));

    [Test]
    public void Find_CaseVariantAliasForSameFunction_ReturnsCanonicalFunction()
        => Assert.That(FunctionCatalog.Default.Find("FILE-TO-CREATION-DATETIME")?.Name, Is.EqualTo("creation-datetime"));

    [Test]
    public void ForScope_CaseInsensitive_ReturnsOnlyRequestedScope()
        => Assert.That(FunctionCatalog.Default.ForScope("record").Select(x => x.Scope), Is.All.EqualTo("Record"));

    [Test]
    public void Suggest_CloseName_ReturnsExpectedFunctionFirst()
        => Assert.That(FunctionCatalog.Default.Suggest("revers").First().Name, Is.EqualTo("reverse"));
}
