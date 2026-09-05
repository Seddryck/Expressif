using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Expressif.Predicates.Introspection;

namespace Expressif.Testing.Predicates.Introspection;

[TestFixture]
[FixtureLifeCycle(LifeCycle.SingleInstance)]
[NonParallelizable]
public class PredicateIntrospectorTest
{
    private static readonly string[] BooleanCombinatorNames =
        ["and", "or", "xor", "not", "nand", "nor", "xnor", "implies", "majority"];

    private IEnumerable<PredicateInfo> Infos { get; set; }

    [SetUp]
    public void Setup()
        => Infos ??= new PredicateIntrospector().Describe();

    [Test]
    public void Locate_ExpressifAssembly_ElementsReturned()
    {
        Debug.WriteLine($"{Infos.Count()} predicates");
        Assert.That(Infos.Count(), Is.GreaterThan(1));
    }

    [Test]
    public void Locate_ExpressifAssembly_NameFollowsPredicateConvention()
    {
        foreach (var info in Infos)
        {
            Debug.WriteLine(info.Name);
            Assert.That(info.Name, Is.Not.Null.And.Not.Empty);
            Assert.That(
                info.Name.StartsWith("is-", StringComparison.Ordinal)
                    || info.Name.StartsWith("has-", StringComparison.Ordinal)
                    || BooleanCombinatorNames.Contains(info.Name, StringComparer.Ordinal)
                    || info.Name is "contains" or "starts-with" or "ends-with"
                    || info.Name.StartsWith("matches-", StringComparison.Ordinal),
                Is.True,
                $"Predicate '{info.Name}' does not follow the naming convention.");
        }
    }

    [Test]
    public void Locate_ExpressifAssembly_BooleanCombinatorsAreAvailable()
        => Assert.That(
            Infos.Where(x => x.Scope == "boolean").Select(x => x.Name),
            Is.SupersetOf(BooleanCombinatorNames));

    [Test]
    public void Locate_ExpressifAssembly_SomeAliases()
    {
        Assert.That(Infos.Count(x => x.Aliases.Length > 0), Is.GreaterThan(1));

        foreach (var info in Infos)
        {
            Debug.WriteLine($"{info.Name}: {(info.Aliases.Length != 0 ? info.Aliases.ElementAt(0) : string.Empty)}");
            foreach (var alias in info.Aliases)
                Assert.That(info.Aliases.ElementAt(0), Is.Not.Null.And.Not.Empty);
        }
    }

    [Test]
    public void Locate_ExpressifAssembly_NoDuplicateAlias()
    {
        var infos = Infos.Where(x => x.Aliases.Length != 0);

        foreach (var info in infos)
            Assert.That(infos.Count(x => x.Aliases.Contains(info.Aliases.ElementAt(0))), Is.EqualTo(1));
    }

    [Test]
    public void Locate_ExpressifAssembly_Namespace()
    {
        foreach (var info in Infos)
        {
            Debug.WriteLine($"{info.Name}: {info.Scope}");
            Assert.That(info.Scope, Is.Not.Null.And.Not.Empty);
            Assert.That(info.Scope, Does.Match("^[a-z][a-z-]*(/[a-z][a-z-]*)?$"));
        }
    }

    [Test]
    public void Locate_ExpressifAssembly_ArithmeticPredicatesExposeNumericSubcategory()
        => Assert.That(
            Infos.Where(x => x.Scope == "numeric/arithmetic").Select(x => x.Name),
            Is.EquivalentTo(new[] { "has-remainder", "is-divisible-by", "is-even", "is-odd" }));
}
