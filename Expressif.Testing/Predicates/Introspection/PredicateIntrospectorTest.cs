using Expressif.Predicates.Introspection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Predicates.Introspection;

[TestFixture]
[FixtureLifeCycle(LifeCycle.SingleInstance)]
[Parallelizable(scope: ParallelScope.Fixtures)]
public class PredicateIntrospectorTest
{
    private IEnumerable<PredicateInfo> Infos { get; set; }

    [SetUp]
    public void Setup()
        => Infos ??= new PredicateIntrospector().Locate();

    [Test]
    public void Locate_ExpressifAssembly_ElementsReturned()
    {
        Debug.WriteLine($"{Infos.Count()} predicates");
        Assert.That(Infos.Count(), Is.GreaterThan(1));
    }

    [Test]
    public void Locate_ExpressifAssembly_NameEqualClass()
    {
        foreach(var info in Infos)
        {
            Debug.WriteLine(info.Name);
            Assert.That(info.Name, Is.Not.Null.Or.Empty);
            Assert.That(info.Name.ToPascalCase(), Is.EqualTo(info.ImplementationType.Name));
        }
    }

    [Test]
    public void Locate_ExpressifAssembly_SomeAliases()
    {
        Assert.That(Infos.Count(x => x.Aliases.Length > 0), Is.GreaterThan(1));

        foreach (var info in Infos)
        {
            Debug.WriteLine($"{info.Name}: {(info.Aliases.Length!=0 ? info.Aliases.ElementAt(0) : string.Empty)}");
            foreach (var alias in info.Aliases)
                Assert.That(info.Aliases.ElementAt(0), Is.Not.Null.Or.Empty);
        }
    }

    [Test]
    public void Locate_ExpressifAssembly_NoDuplicateAlias()
    {
        var infos = Infos.Where(x => x.Aliases.Length!=0);

        foreach (var info in infos)
            Assert.That(infos.Count(x => x.Aliases.Contains(info.Aliases.ElementAt(0))), Is.EqualTo(1));
    }

    [Test]
    public void Locate_ExpressifAssembly_Namespace()
    {
        foreach (var info in Infos)
        {
            Debug.WriteLine($"{info.Name}: {info.Scope}");
            Assert.That(info.Scope, Is.Not.Null.Or.Empty);
            Assert.That(info.Scope, Is.EqualTo("Text").Or.EqualTo("Numeric").Or.EqualTo("Boolean").Or.EqualTo("Temporal").Or.EqualTo("Special"));
        }
    }
}
