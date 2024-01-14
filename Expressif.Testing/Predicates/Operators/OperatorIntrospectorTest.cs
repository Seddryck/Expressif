using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Expressif.Predicates.Operators;

namespace Expressif.Testing.Predicates.Operators;

[TestFixture]
[FixtureLifeCycle(LifeCycle.SingleInstance)]
[Parallelizable(scope: ParallelScope.Fixtures)]
internal class OperatorIntrospectorTest
{
    private OperatorInfo[] Infos { get; set; }

    [OneTimeSetUp]
    public void Setup()
        => Infos ??= new OperatorIntrospector().Describe().ToArray();

    [Test]
    public void Locate_ExpressifAssembly_ElementsReturned()
    {
        Debug.WriteLine($"{Infos.Length} operators");
        Assert.That(Infos, Has.Length.EqualTo(4));
    }

    [Test]
    public void Locate_ExpressifAssembly_NameEqualClass()
    {
        foreach (var info in Infos)
        {
            Debug.WriteLine(info.Name);
            Assert.That(info.Name, Is.Not.Null.Or.Empty);
            Assert.That(info.Name.ToPascalCase(), Is.EqualTo(info.ImplementationType.Name.TrimAt('`')));
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
    public void Locate_ExpressifAssembly_TypeIsIOperator()
    {
        foreach (var info in Infos)
        {
            Debug.WriteLine($"{info.Name}: {info.ImplementationType.Name}");
            Assert.That(info.ImplementationType.Namespace, Is.EqualTo("Expressif.Predicates.Operators"));
        }
    }
}
