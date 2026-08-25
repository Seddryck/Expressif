using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Expressif.Predicates;
using Expressif.Predicates.Numeric;
using Expressif.Predicates.Text;

namespace Expressif.Testing.Predicates;

public class PredicateTypeMapperTest
{
    [Test]
    [TestCase("equal-to", typeof(EqualTo))]
    [TestCase("equivalent-to", typeof(EquivalentTo))]
    [TestCase("greater-than", typeof(GreaterThan))]
    public void Execute_PredicateName_Valid(string value, Type expected)
            => Assert.That(new PredicateTypeMapper().Execute(value), Is.EqualTo(expected));

    [Test]
    [TestCase("even", typeof(Even))]
    [TestCase("Even", typeof(Even))]
    [TestCase("numeric-is-even", typeof(Even))]
    [TestCase("is-divisible-by", typeof(DivisibleBy))]
    [TestCase("divisible-by", typeof(DivisibleBy))]
    [TestCase("numeric-is-divisible-by", typeof(DivisibleBy))]
    [TestCase("equivalent-to", typeof(EquivalentTo))]
    [TestCase("Equivalent-To", typeof(EquivalentTo))]
    [TestCase("text-is-equivalent-to", typeof(EquivalentTo))]
    public void Execute_PredicateNameVariations_Valid(string value, Type expected)
        => Assert.That(new PredicateTypeMapper().Execute(value), Is.EqualTo(expected));

    [Test]
    [TestCase("foo")]
    [TestCase("foo-to-bar")]
    [TestCase("foo - to - bar")]
    [TestCase("boolean-is-and")]
    [TestCase("boolean-is-not")]
    [TestCase("boolean-is-or")]
    [TestCase("boolean-is-xor")]
    public void Execute_PredicateName_Invalid(string value)
        => Assert.That(() => new PredicateTypeMapper().Execute(value), Throws.TypeOf<NotImplementedFunctionException>());

    [Test]
    public void Execute_RenamedPredicateAndLegacyAlias_ResolveToSameImplementation()
    {
        var manifestPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "predicates-rename.json");
        var renames = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(manifestPath))!;
        var mapper = new PredicateTypeMapper();

        Assert.Multiple(() =>
        {
            foreach (var (legacyName, canonicalName) in renames)
                Assert.That(
                    mapper.Execute(legacyName),
                    Is.EqualTo(mapper.Execute(canonicalName)),
                    $"Legacy predicate '{legacyName}' should alias '{canonicalName}'.");
        });
    }
}
