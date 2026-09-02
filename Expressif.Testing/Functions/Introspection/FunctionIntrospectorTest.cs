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
            Assert.That(info.Scope, Does.Match("^[a-z][a-z-]*(/[a-z][a-z-]*)?$"));
        }
    }

    [Test]
    public void Locate_ExpressifAssembly_ArithmeticFunctionsExposeNumericSubcategory()
        => Assert.That(
            Infos.Where(x => x.Scope == "numeric/arithmetic").Select(x => x.Name),
            Is.EquivalentTo(new[] { "absolute", "add", "cube-power", "cube-root", "decrement", "divide", "greatest-common-divisor", "increment", "invert", "lowest-common-multiple", "multiply", "nth-root", "oppose", "percent-change", "power", "sign", "square-power", "square-root", "subtract" }));

    [TestCase("array/set", new[] { "complement", "difference", "distinct", "intersection", "symmetric-difference", "union" })]
    [TestCase("array/aggregation", new[] { "broadcast", "fold", "scan" })]
    [TestCase("array/combination", new[] { "zip", "zip-padded", "zip-strict" })]
    [TestCase("array/partitioning", new[] { "chunk", "chunk-around", "chunk-on", "chunk-while" })]
    [TestCase("array/selection", new[] { "first-elements", "last-elements", "single", "skip-first-elements", "skip-last-elements", "slice-elements", "value-at" })]
    [TestCase("array/sequencing", new[] { "adjacent", "lag", "lead", "pairwise", "position-of", "reverse", "with-position" })]
    [TestCase("numeric/rounding", new[] { "ceiling", "clip", "floor", "integer", "round" })]
    [TestCase("numeric/conversion", new[] { "null-to-zero" })]
    [TestCase("numeric/formatting", new[] { "human-readable-format-binary-bytes", "human-readable-format-decimal", "human-readable-format-decimal-bytes" })]
    [TestCase("temporal/calendar", new[] { "catholic-calendar", "first-in-month", "first-of-month", "first-of-year", "last-in-month", "last-of-month", "last-of-year", "length-of-month", "length-of-year" })]
    [TestCase("temporal/conversion", new[] { "datetime-to-date", "invalid-to-date", "null-to-date" })]
    [TestCase("text/casing", new[] { "allcaps-case", "camel-case", "camel-snake-case", "cobol-case", "dot-case", "flat-case", "kebab-case", "lower", "namespace-case", "pascal-case", "pascal-snake-case", "path-case", "screaming-snake-case", "sentence-case", "snake-case", "swap-case", "title-case", "train-case", "upper" })]
    [TestCase("text/character", new[] { "remove-chars", "replace-chars" })]
    [TestCase("text/concatenation", new[] { "append", "append-new-line", "append-space", "prefix", "prefix-new-line", "prefix-space", "prepend", "prepend-new-line", "prepend-space", "replace-slice", "suffix", "suffix-new-line", "suffix-space", "text" })]
    [TestCase("text/conversion", new[] { "text-to-datetime" })]
    [TestCase("text/counting", new[] { "count-distinct-chars", "count-substring", "length" })]
    [TestCase("text/encoding", new[] { "html-to-text", "text-to-html" })]
    [TestCase("text/filtering", new[] { "filter-chars", "retain-alpha", "retain-alpha-numeric", "retain-numeric", "retain-numeric-symbol" })]
    [TestCase("text/masking", new[] { "mask-to-text", "text-to-mask" })]
    [TestCase("text/normalization", new[] { "clean-whitespace", "collapse-whitespace", "empty-to-null", "null-to-empty", "slug", "trim", "whitespaces-to-empty", "whitespaces-to-null", "without-diacritics", "without-whitespaces" })]
    [TestCase("text/padding", new[] { "pad-center", "pad-left", "pad-right" })]
    [TestCase("text/selection", new[] { "after-substring", "before-substring", "first-chars", "last-chars", "skip-first-chars", "skip-last-chars" })]
    [TestCase("text/tokenization", new[] { "token", "token-count", "token-count-lexical", "tokenize", "tokenize-camel", "tokenize-kebab", "tokenize-lexical", "tokenize-pascal", "tokenize-snake", "tokenize-words" })]
    public void Locate_ExpressifAssembly_FunctionsExposeBehavioralSubcategory(string scope, string[] names)
        => Assert.That(
            Infos.Where(x => x.Scope == scope).Select(x => x.Name),
            Is.EquivalentTo(names));

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

    [Test]
    public void Describe_Apply_ExposesIntentionalDynamicContract()
    {
        var info = Infos.Single(x => x.Name == "apply");
        using (Assert.EnterMultipleScope())
        {
            Assert.That(info.Input, Is.EqualTo("any"));
            Assert.That(info.Output, Is.EqualTo("any"));
        }
    }

    [TestCase("round", "numeric", "numeric")]
    [TestCase("length", "text", "integer")]
    [TestCase("datetime-to-date", "date-time", "date")]
    [TestCase("next-weekday", "date-time", "date")]
    [TestCase("reverse", "array", "array")]
    [TestCase("to-tuple", "array", "tuple")]
    [TestCase("single", "array", "any")]
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
            Is.EquivalentTo(new[] { "apply", "coalesce", "field", "guard", "neutral", "walk", "with" }));

    [TestCase("after-substring", "substring", "text")]
    [TestCase("first-chars", "length", "integer")]
    [TestCase("clamp", "min", "date-time")]
    [TestCase("filter", "predicate", "predicate")]
    [TestCase("adjacent", "operation", "expression")]
    [TestCase("fold", "accumulator", "accumulator")]
    [TestCase("next-weekday", "weekday", "weekday")]
    [TestCase("duration-between", "previous", "date | date-time | year-month")]
    [TestCase("with", "projections", "entry")]
    [TestCase("with", "body", "expression")]
    public void Describe_Parameter_ExposesExpressifType(string function, string parameter, string type)
        => Assert.That(
            Infos.Single(x => x.Name == function).Parameters.Single(x => x.Name == parameter).Type,
            Is.EqualTo(type));

    [Test]
    public void Describe_AllParametersExposeExpressifTypes()
        => Assert.That(
            Infos.SelectMany(x => x.Parameters).Select(x => x.Type),
            Is.All.Not.Null.And.Not.Empty);

    [TestCase("array", "values", "any", true, 0)]
    [TestCase("record", "entries", "entry", true, 0)]
    [TestCase("coalesce", "expressions", "expression", false, 2)]
    public void Describe_VariadicParameter_ExposesElementTypeAndVariadicity(
        string function,
        string parameter,
        string type,
        bool optional,
        int minimumCardinality)
    {
        var info = Infos.Single(x => x.Name == function).Parameters.Single(x => x.Name == parameter);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(info.Type, Is.EqualTo(type));
            Assert.That(info.Optional, Is.EqualTo(optional));
            Assert.That(info.Variadic, Is.True);
            Assert.That(info.MinimumCardinality, Is.EqualTo(minimumCardinality));
        }
    }

    [Test]
    public void Describe_NonVariadicParameter_IsNotVariadic()
    {
        var info = Infos.Single(x => x.Name == "difference").Parameters.Single(x => x.Name == "array");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(info.Variadic, Is.False);
            Assert.That(info.MinimumCardinality, Is.EqualTo(1));
        }
    }

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
