using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Expressif.Functions.Introspection;

namespace Expressif.Testing.Functions.Introspection;

[TestFixture]
[FixtureLifeCycle(LifeCycle.SingleInstance)]
[NonParallelizable]
public class FunctionIntrospectorTest
{
    private IEnumerable<FunctionInfo> Infos { get; set; }

    [SetUp]
    public void Setup()
        => Infos ??= new FunctionIntrospector().Describe();

    [Test]
    public void Locate_ExpressifAssembly_ElementsReturned()
    {
        Debug.WriteLine($"{Infos.Count()} functions");
        Assert.That(Infos.Count(), Is.GreaterThan(1));
    }

    [Test]
    public void Locate_ExpressifAssembly_NameEqualClass()
    {
        foreach (var info in Infos)
        {
            Debug.WriteLine(info.Name);
            Assert.That(info.Name, Is.Not.Null.And.Not.Empty);
            Assert.That(info.Name.ToPascalCase(), Is.EqualTo(info.ImplementationType.Name));
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
                Assert.That(info.Aliases.ElementAt(0), Is.Not.Null.And.Not.Empty);
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
    public void Locate_ExpressifAssembly_DateToAgeIsSynonymOfAge()
    {
        var info = Infos.Single(x => x.Name == "age");
        Assert.That(info.Aliases, Has.Length.EqualTo(1));
        Assert.That(info.Aliases, Does.Contain("date-to-age"));
    }

    [Test]
    public void Locate_ExpressifAssembly_DateTimeToDate()
    {
        var info = Infos.Single(x => x.Name == "datetime-to-date");
        Assert.That(info.Aliases, Has.Length.EqualTo(1));
        Assert.That(info.Aliases, Does.Contain("dateTime-to-date"));
    }

    [Test]
    public void Locate_ExpressifAssembly_TextToDateTime()
    {
        var info = Infos.Single(x => x.Name == "text-to-datetime");
        Assert.That(info.Aliases, Does.Contain("text-to-dateTime"));
    }

    [Test]
    public void Locate_ExpressifAssembly_CreationDateTime()
    {
        var info = Infos.Single(x => x.Name == "creation-datetime");
        Assert.That(info.Aliases, Has.Length.EqualTo(2));
        Assert.That(info.Aliases, Does.Contain("file-to-creation-dateTime"));
        Assert.That(info.Aliases, Does.Contain("file-to-creation-datetime"));
    }

    [Test]
    public void Locate_ExpressifAssembly_Namespace()
    {
        foreach (var info in Infos)
        {
            Debug.WriteLine($"{info.Name}: {info.Scope}");
            Assert.That(info.Scope, Is.Not.Null.And.Not.Empty);
            Assert.That(info.Scope, Does.Match("^(Text|Numeric|IO|Temporal|Special|Array|Record)(/[A-Z][A-Za-z]*)?$"));
        }
    }

    [Test]
    public void Locate_ExpressifAssembly_ArithmeticFunctionsExposeNumericSubcategory()
        => Assert.That(
            Infos.Where(x => x.Scope == "Numeric/Arithmetic").Select(x => x.Name),
            Is.EquivalentTo(new[] { "add", "subtract", "increment", "decrement", "multiply", "divide" }));

    [Test]
    public void Describe_AllFunctionsExposeExpressifInputAndOutputTypes()
    {
        foreach (var info in Infos)
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(info.Input, Is.Not.Null.And.Not.Empty, info.Name);
                Assert.That(info.Output, Is.Not.Null.And.Not.Empty, info.Name);
            }
        }
    }

    [TestCase("round", "numeric", "numeric")]
    [TestCase("length", "text", "integer")]
    [TestCase("datetime-to-date", "date-time", "date")]
    [TestCase("next-weekday", "date-time", "date")]
    [TestCase("reverse", "array", "array")]
    [TestCase("record", "any", "record")]
    public void Describe_TypedFunction_ExposesExpressifContract(string name, string input, string output)
    {
        var info = Infos.Single(x => x.Name == name);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(info.Converted, Is.True);
            Assert.That(info.Input, Is.EqualTo(input));
            Assert.That(info.Output, Is.EqualTo(output));
        }
    }

    [Test]
    public void Describe_UnconvertedFunctions_AreExplicitlyReported()
        => Assert.That(
            Infos.Where(x => !x.Converted).Select(x => x.Name),
            Is.EquivalentTo(new[] { "coalesce", "field", "neutral" }));

    [TestCase("after-substring", "substring", "text")]
    [TestCase("first-chars", "length", "integer")]
    [TestCase("clamp", "min", "date-time")]
    [TestCase("filter", "predicate", "predicate")]
    [TestCase("adjacent", "operation", "expression")]
    [TestCase("fold", "accumulator", "accumulator")]
    [TestCase("next-weekday", "weekday", "weekday")]
    [TestCase("duration-between", "previous", "date | date-time | year-month")]
    public void Describe_Parameter_ExposesExpressifType(string function, string parameter, string type)
        => Assert.That(
            Infos.Single(x => x.Name == function).Parameters.Single(x => x.Name == parameter).Type,
            Is.EqualTo(type));

    [Test]
    public void Describe_AllParametersExposeExpressifTypes()
        => Assert.That(
            Infos.SelectMany(x => x.Parameters).Select(x => x.Type),
            Is.All.Not.Null.And.Not.Empty);

    [Test]
    public void Locate_ExpressifAssembly_ArrayFunctionsExposeWrappersAndShiftsOnly()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Infos.Any(x => x.Name == "fold"), Is.True);
            Assert.That(Infos.Any(x => x.Name == "broadcast"), Is.True);
            Assert.That(Infos.Any(x => x.Name == "scan"), Is.True);
            Assert.That(Infos.Any(x => x.Name == "lag"), Is.True);
            Assert.That(Infos.Any(x => x.Name == "lead"), Is.True);

            Assert.That(Infos.Any(x => x.Name == "count"), Is.False);
            Assert.That(Infos.Any(x => x.Name == "sum"), Is.False);
            Assert.That(Infos.Any(x => x.Name == "min"), Is.False);
            Assert.That(Infos.Any(x => x.Name == "max"), Is.False);
            Assert.That(Infos.Any(x => x.Name == "first"), Is.False);
            Assert.That(Infos.Any(x => x.Name == "last"), Is.False);
        }
    }
}
