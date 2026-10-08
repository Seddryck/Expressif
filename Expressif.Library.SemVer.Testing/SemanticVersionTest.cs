using Expressif.Bindings;
using Expressif.Hosting;
using Expressif.Library.Composition;
using Expressif.Planning;
using Expressif.Syntax;
using Expressif.Types;
using NUnit.Framework;
using System.Reflection;
using System.Reflection.Emit;

namespace Expressif.Library.SemVer.Testing;

public sealed class SemanticVersionTest
{
    private static ExpressifEnvironment Environment { get; } =
        ExpressifEnvironment.Default.RegisterLibrary<SemVerLibrary>();

    private static ExpressionFactory Factory { get; } = new(new ExpressionBinder(Environment));

    [TestCase("1.0.0")]
    [TestCase("1.25.0-rc.1+abc")]
    [TestCase("0.0.0+001")]
    public void Parse_RoundTripsValidVersions(string text)
    {
        Assert.That(SemanticVersion.TryParse(text, out var version), Is.True);
        Assert.That(version?.ToString(), Is.EqualTo(text));
    }

    [TestCase("2.5")]
    [TestCase("01.0.0")]
    [TestCase("1.0.0-01")]
    [TestCase("1.0.0+")]
    public void Parse_RejectsInvalidVersions(string text)
        => Assert.That(SemanticVersion.TryParse(text, out _), Is.False);

    [Test]
    public void Environment_DiscoversLibraryCapabilities()
    {
        Assert.Multiple(() =>
        {
            Assert.That(typeof(SemVerLibrary).Assembly.GetName().Name,
                Is.EqualTo("Expressif.Library.SemVer"));
            Assert.That(Environment.Libraries.Select(library => library.Name),
                Does.Contain("Expressif.Library.SemVer"));
            Assert.That(Environment.Types.Resolve("semver").RuntimeType, Is.EqualTo(typeof(SemanticVersion)));
            Assert.That(Environment.Catalog.Find("semver::bump-patch")?.Output, Is.EqualTo("semver"));
            Assert.That(Environment.QuotedLiterals.Parsers.Select(parser => parser.TypeName), Does.Contain("semver"));
        });
    }

    [Test]
    public void TypedLiteral_CanBeBoundAndEvaluatedOnlyInRegisteredEnvironment()
    {
        var expression = Factory.CreateClosed("#\"1.2.3-rc.1+build.7\":semver | bump-patch");

        Assert.That(expression.Evaluate(null), Is.EqualTo(new SemanticVersion(1, 2, 4)));
        Assert.That(
            () => new ExpressionFactory(new ExpressionBinder()).CreateClosed("#\"1.2.3\":semver | bump-patch"),
            Throws.Exception);
    }

    [Test]
    public void UnsuffixedLiteral_UsesSemVerWhenItIsTheOnlyAcceptingParser()
    {
        var expression = Factory.CreateClosed("#\"1.2.3-rc.1+build.7\" | bump-patch");

        Assert.That(expression.Evaluate(null), Is.EqualTo(new SemanticVersion(1, 2, 4)));
    }

    [Test]
    public void RegisteringSemVer_DoesNotChangeBuiltInTemporalLiteralInference()
    {
        var expression = Factory.CreateClosed("#\"2026-10-01\" | year");

        Assert.That(expression.Evaluate(null), Is.EqualTo("2026"));
    }

    [Test]
    public void CoerceAndTypeInspection_UseRegisteredTypeAndCoercions()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Factory.CreateClosed("\"1.25.0+abc\" | coerce(:semver) | is-type(:semver)").Evaluate(null), Is.True);
            Assert.That(Factory.CreateClosed("\"invalid\" | coerce(:semver)").Evaluate(null), Is.Null);
            Assert.That(Factory.CreateClosed("#\"1.25.0+abc\":semver | coerce(:text)").Evaluate(null), Is.EqualTo("1.25.0+abc"));
            Assert.That(Factory.CreateClosed("\"1.25.0\" | is-type(:semver)").Evaluate(null), Is.False);
        });
    }

    [Test]
    public void FunctionsAndPredicates_FollowSemVerRules()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Factory.CreateClosed("#\"1.2.3-rc.1+build.7\":semver | major").Evaluate(null), Is.EqualTo(1));
            Assert.That(Factory.CreateClosed("#\"1.2.3-rc.1+build.7\":semver | pre-release").Evaluate(null), Is.EqualTo("rc.1"));
            Assert.That(Factory.CreateClosed("#\"1.2.3-rc.1+build.7\":semver | build-metadata").Evaluate(null), Is.EqualTo("build.7"));
            Assert.That(Factory.CreateClosed("#\"1.0.0+abc\":semver | same-version(#\"1.0.0+def\":semver)").Evaluate(null), Is.False);
            Assert.That(Factory.CreateClosed("#\"1.0.0+abc\":semver | same-precedence(#\"1.0.0+def\":semver)").Evaluate(null), Is.True);
            Assert.That(Factory.CreateClosed("#\"1.0.0-alpha\":semver | lower-precedence(#\"1.0.0\":semver)").Evaluate(null), Is.True);
        });
    }

    [Test]
    public void EqualityHashingAndOrdering_AreConsistent()
    {
        var first = new SemanticVersion(1, 0, 0, buildMetadata: "abc");
        var same = new SemanticVersion(1, 0, 0, buildMetadata: "abc");
        var differentBuild = new SemanticVersion(1, 0, 0, buildMetadata: "def");

        Assert.Multiple(() =>
        {
            Assert.That(new[] { first, same, differentBuild }.Distinct().ToArray(), Has.Length.EqualTo(2));
            Assert.That(first.GetHashCode(), Is.EqualTo(same.GetHashCode()));
            Assert.That(first.CompareTo(differentBuild), Is.Zero);
            Assert.That(new SemanticVersion(1, 0, 0, "rc.1").CompareTo(first), Is.LessThan(0));
        });
    }

    [Test]
    public void LiteralSerialization_RoundTripsThroughRegisteredParser()
    {
        var value = new SemanticVersion(1, 25, 0, "rc.1", "abc");
        var serialized = Environment.QuotedLiterals.Serialize(value);
        var parsed = Environment.QuotedLiterals.Parse("1.25.0-rc.1+abc", "semver");

        Assert.Multiple(() =>
        {
            Assert.That(serialized, Is.EqualTo("#\"1.25.0-rc.1+abc\":semver"));
            Assert.That(parsed.Value, Is.EqualTo(value));
        });
    }

    [Test]
    public void LogicalPlanSerialization_RoundTripsRegisteredLiteral()
    {
        var syntax = ExpressionParser.Parse("#\"1.2.3+abc\":semver | bump-patch");
        var plan = LogicalPlannerFactory.Create(Environment).Build(syntax);
        var restored = LogicalPlanJson.Deserialize(LogicalPlanJson.Serialize(plan));
        IExpressionBinder binder = new ExpressionBinder(Environment);

        Assert.That(binder.BindClosed(restored).Evaluate(null), Is.EqualTo(new SemanticVersion(1, 2, 4)));
    }

    [Test]
    public void Registration_RejectsIncompatibleCoreApiWithoutChangingExistingEnvironment()
    {
        var incompatible = CreateLibraryAssembly("Incompatible", "4.0.0", "5.0.0");

        Assert.That(
            () => Environment.RegisterLibrary(incompatible),
            Throws.TypeOf<LibraryRegistrationException>().With.Message.Contains("requires Expressif core API"));
        Assert.That(Environment.Libraries.Select(library => library.Name), Does.Not.Contain("Incompatible"));
    }

    [Test]
    public void Registration_RejectsMissingDependencyWithoutChangingExistingEnvironment()
    {
        var missing = CreateLibraryAssembly("MissingDependency", "3.0.0", "4.0.0", "Absent.Library");

        Assert.That(
            () => Environment.RegisterLibrary(missing),
            Throws.TypeOf<LibraryRegistrationException>().With.Message.Contains("requires missing library"));
        Assert.That(Environment.Libraries.Select(library => library.Name), Does.Not.Contain("MissingDependency"));
    }

    [Test]
    public void Registration_RejectsCatalogCollisionWithoutChangingExistingEnvironment()
    {
        Assert.That(
            () => Environment.RegisterLibrary(typeof(CollisionFunction).Assembly),
            Throws.TypeOf<LibraryRegistrationException>().With.Message.Contains("ambiguous"));
        Assert.That(Environment.Catalog.Find("semver::bump-patch"), Is.Not.Null);
        Assert.That(
            Environment.Libraries.Select(library => library.Name),
            Does.Not.Contain("Expressif.Library.SemVer.CollisionFixture"));
    }

    private static Assembly CreateLibraryAssembly(
        string name,
        string minimumCoreApi,
        string maximumCoreApiExclusive,
        string? dependency = null)
    {
        var assembly = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName($"Expressif.Testing.{name}"),
            AssemblyBuilderAccess.Run);
        var manifestConstructor = typeof(ExpressifLibraryAttribute).GetConstructor(
            [typeof(string), typeof(string), typeof(string), typeof(string)])!;
        assembly.SetCustomAttribute(new CustomAttributeBuilder(
            manifestConstructor,
            new object[] { name, "1.0.0", minimumCoreApi, maximumCoreApiExclusive }));
        if (dependency is not null)
        {
            var dependencyConstructor = typeof(ExpressifLibraryDependencyAttribute).GetConstructor(
                [typeof(string), typeof(string), typeof(string)])!;
            assembly.SetCustomAttribute(new CustomAttributeBuilder(
                dependencyConstructor,
                new object[] { dependency, "1.0.0", "2.0.0" }));
        }
        return assembly;
    }
}
