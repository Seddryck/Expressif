using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Expressif.Functions.Accumulation;
using Expressif.Discovery;
using Expressif.Introspection;

namespace Expressif.Testing.Introspection;

[TestFixture]
[FixtureLifeCycle(LifeCycle.SingleInstance)]
[NonParallelizable]
public class AccumulatorFunctionIntrospectionTest
{
    private IEnumerable<FunctionInfo> Infos { get; set; }

    [SetUp]
    public void Setup()
        => Infos ??= ExpressifIntrospection.Functions.Describe()
            .Where(info => typeof(IAccumulator).IsAssignableFrom(info.ImplementationType));

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
        Assert.That(Infos.Count(x => x.Aliases.Length > 0), Is.GreaterThan(0));

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
            Assert.That(info.Scope, Is.EqualTo(info.Name == "concat" ? "text" : "array"));
        }
    }

    [Test]
    public void Describe_Concat_ExposesDeprecatedAlias()
    {
        var info = Infos.Single(x => x.Name == "concat");
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Infos.Any(x => x.Name == "implode"), Is.False);
            Assert.That(info.Aliases, Does.Contain("implode"));
            Assert.That(info.DeprecatedAliases.Single().Name, Is.EqualTo("implode"));
            Assert.That(info.DeprecatedAliases.Single().Replacement, Is.EqualTo("concat"));
            Assert.That(info.DeprecatedAliases.Single().Message, Does.Contain("use concat"));
            Assert.That(info.DeprecatedAliases.Single().Sunset, Is.EqualTo("3.0"));
            Assert.That(info.DeprecatedAliases.Single().ReplacementIsEquivalent, Is.True);
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
            Assert.That(Infos.Any(x => x.Name == "every"), Is.True);
            Assert.That(Infos.Any(x => x.Name == "any"), Is.True);
            Assert.That(Infos.Any(x => x.Name == "concat"), Is.True);
            Assert.That(Infos.Any(x => x.Name == "reduce"), Is.True);
        }
    }
}
