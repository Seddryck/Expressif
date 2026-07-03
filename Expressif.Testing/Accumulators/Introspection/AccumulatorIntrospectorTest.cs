using Expressif.Accumulators.Introspection;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Expressif.Testing.Accumulators.Introspection;

[TestFixture]
[FixtureLifeCycle(LifeCycle.SingleInstance)]
[NonParallelizable]
public class AccumulatorIntrospectorTest
{
    private IEnumerable<AccumulatorInfo> Infos { get; set; }

    [SetUp]
    public void Setup()
        => Infos ??= new AccumulatorIntrospector().Describe();

    [Test]
    public void Locate_ExpressifAssembly_ElementsReturned()
    {
        Debug.WriteLine($"{Infos.Count()} accumulators");
        Assert.That(Infos.Count(), Is.GreaterThan(1));
    }

    [Test]
    public void Locate_ExpressifAssembly_NameEqualClass()
    {
        foreach (var info in Infos)
        {
            Debug.WriteLine(info.Name);
            Assert.That(info.Name, Is.Not.Null.And.Not.Empty);
            Assert.That(info.ImplementationType.Name, Does.StartWith(info.Name.ToPascalCase()));
            Assert.That(info.Name.ToPascalCase(), Does.Not.Contain("accumulator"));
        }
    }

    [Test]
    public void Locate_ExpressifAssembly_SomeAliases()
    {
        Assert.That(Infos.Count(x => x.Aliases.Length > 0), Is.GreaterThan(1));

        foreach (var info in Infos)
        {
            Debug.WriteLine($"{info.Name}: {(info.Aliases.Any() ? info.Aliases.ElementAt(0) : string.Empty)}");
            foreach (var alias in info.Aliases)
                Assert.That(alias, Is.Not.Null.And.Not.Empty);
        }
    }

    [Test]
    public void Locate_ExpressifAssembly_NoDuplicateAlias()
    {
        var infos = Infos.Where(x => x.Aliases.Any());
        foreach (var info in infos)
            Assert.That(infos.Count(x => x.Aliases.Contains(info.Aliases.ElementAt(0))), Is.EqualTo(1));
    }

    [Test]
    public void Locate_ExpressifAssembly_Namespace()
    {
        foreach (var info in Infos)
        {
            Debug.WriteLine($"{info.Name}: {info.Scope}");
            Assert.That(info.Scope, Is.EqualTo("Array"));
        }
    }

    [Test]
    public void Locate_ExpressifAssembly_ArrayAccumulatorsExposed()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Infos.Any(x => x.Name == "count"), Is.True);
            Assert.That(Infos.Any(x => x.Name == "sum"), Is.True);
            Assert.That(Infos.Any(x => x.Name == "min"), Is.True);
            Assert.That(Infos.Any(x => x.Name == "max"), Is.True);
            Assert.That(Infos.Any(x => x.Name == "first"), Is.True);
            Assert.That(Infos.Any(x => x.Name == "last"), Is.True);
        }
    }
}
