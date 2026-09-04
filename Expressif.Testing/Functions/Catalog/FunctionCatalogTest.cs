using Expressif.Functions.Catalog;
using Expressif.Functions.Introspection;
using Expressif.Functions;

namespace Expressif.Testing.Functions.Catalog;

[TestFixture]
public class FunctionCatalogTest
{
    [Test]
    [Category("MetadataConsistency")]
    public void Default_ExpressifAssembly_ContainsEmbeddedCatalog()
    {
        var assembly = typeof(FunctionCatalog).Assembly;

        Assert.That(assembly.GetManifestResourceNames(), Does.Contain(FunctionCatalog.ResourceName));
        Assert.That(FunctionCatalog.Default.Functions, Is.Not.Empty);
    }

    [Test]
    [Category("MetadataConsistency")]
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
                Assert.That(documentation.Deprecated, Is.EqualTo(implementation.Deprecated), $"Deprecation for {name}");
                Assert.That(documentation.Replacement, Is.EqualTo(implementation.Replacement), $"Replacement for {name}");
                Assert.That(documentation.Sunset, Is.EqualTo(implementation.Sunset), $"Sunset for {name}");
                Assert.That(documentation.Parameters.Select(x => (x.Name, Type: x.TypeOrKind, x.Optional, x.Variadic)),
                    Is.EqualTo(implementation.Parameters.Select(x => (x.Name, x.Type, x.Optional, x.Variadic))),
                    $"Parameters for {name}");
                Assert.That(documentation.Parameters.Select(x => x.Summary), Is.All.Not.Empty, $"Parameter summaries for {name}");
            }
        }
    }

    [Test]
    [Category("MetadataConsistency")]
    public void Default_LifecycleMetadata_IsConsistent()
    {
        foreach (var function in FunctionCatalog.Default.Functions)
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(function.Replacement is null || function.Deprecated, Is.True,
                    $"Replacement for {function.Name} requires deprecation.");
                Assert.That(function.Sunset is null || function.Deprecated, Is.True,
                    $"Sunset for {function.Name} requires deprecation.");
                Assert.That(function.Replacement is null || FunctionCatalog.Default.Find(function.Replacement)?.IsPublic == true,
                    Is.True, $"Replacement for {function.Name} must resolve to a public callable.");
            }
        }
    }

    [TestCase("append", "suffix", "3.0")]
    [TestCase("prepend", "prefix", "3.0")]
    public void Default_DeprecatedTextFunction_ExposesLifecycle(
        string name,
        string replacement,
        string sunset)
    {
        var function = FunctionCatalog.Default.Find(name);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(function?.Deprecated, Is.True);
            Assert.That(function?.Replacement, Is.EqualTo(replacement));
            Assert.That(function?.Sunset, Is.EqualTo(sunset));
        }
    }

    [TestCase("append-space")]
    [TestCase("append-new-line")]
    [TestCase("prepend-space")]
    [TestCase("prepend-new-line")]
    public void Default_RelatedTextFunction_DoesNotInheritLifecycle(string name)
        => Assert.That(FunctionCatalog.Default.Find(name)?.Deprecated, Is.False);

    [Test]
    public void LifecycleAttribute_Values_ExposesDeprecationMetadata()
    {
        var lifecycle = new FunctionLifecycleAttribute("replacement", "3.0");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(lifecycle.Deprecated, Is.True);
            Assert.That(lifecycle.Replacement, Is.EqualTo("replacement"));
            Assert.That(lifecycle.Sunset, Is.EqualTo("3.0"));
        }
    }

    [Test]
    public void LifecycleRecords_Values_ExposeDeprecationMetadata()
    {
        var documentation = new FunctionDocumentation(
            "sample", true, [], "special", "any", "any", "Summary.", [],
            Deprecated: true, Replacement: "replacement", Sunset: "3.0");
        var implementation = new FunctionInfo(
            "sample", true, [], "special", "any", "any", false, "Reason.",
            typeof(object), "Summary.", [], true, "replacement", "3.0");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(documentation.Deprecated, Is.True);
            Assert.That(documentation.Replacement, Is.EqualTo("replacement"));
            Assert.That(documentation.Sunset, Is.EqualTo("3.0"));
            Assert.That(implementation.Deprecated, Is.True);
            Assert.That(implementation.Replacement, Is.EqualTo("replacement"));
            Assert.That(implementation.Sunset, Is.EqualTo("3.0"));
        }
    }

    [Test]
    public void Find_Alias_ReturnsCanonicalFunction()
        => Assert.That(FunctionCatalog.Default.Find("array-to-broadcast")?.Name, Is.EqualTo("broadcast"));

    [TestCase("array", "values", 0)]
    [TestCase("record", "entries", 0)]
    [TestCase("coalesce", "expressions", 2)]
    [TestCase("transform-with", "expressions", 1)]
    [TestCase("transform-as", "expressions", 1)]
    public void Default_VariadicParameter_DeserializesMinimumCardinality(
        string function,
        string parameter,
        int minimumCardinality)
        => Assert.That(
            FunctionCatalog.Default.Find(function)?.Parameters.Single(x => x.Name == parameter).MinimumCardinality,
            Is.EqualTo(minimumCardinality));

    [Test]
    public void Find_CaseVariantAliasForSameFunction_ReturnsCanonicalFunction()
        => Assert.That(FunctionCatalog.Default.Find("FILE-TO-CREATION-DATETIME")?.Name, Is.EqualTo("creation-datetime"));

    [Test]
    public void ForScope_CaseInsensitive_ReturnsOnlyRequestedScope()
        => Assert.That(FunctionCatalog.Default.ForScope("record").Select(x => x.Scope), Is.All.EqualTo("record"));

    [Test]
    public void Suggest_CloseName_ReturnsExpectedFunctionFirst()
        => Assert.That(FunctionCatalog.Default.Suggest("revers").First().Name, Is.EqualTo("reverse"));

    [Test]
    public void Default_FunctionWithExamples_DeserializesExamples()
        => Assert.That(
            FunctionCatalog.Default.Find("add")?.Examples,
            Is.EqualTo(new[] { "10 | add(5)      → 15", "10 | add(5, 2)   → 20" }));

    [Test]
    public void Default_FunctionWithBehavior_DeserializesBehavior()
        => Assert.That(
            FunctionCatalog.Default.Find("adjacent")?.Behavior,
            Does.StartWith("`adjacent` evaluates the supplied operation"));
}
