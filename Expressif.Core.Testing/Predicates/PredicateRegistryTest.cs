using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Expressif.Functions;
using Expressif.Predicates;
using Expressif.Library.Numeric;
using Expressif.Library.Text;

namespace Expressif.Testing.Predicates;

public class PredicateRegistryTest
{
    private static IImplementationRegistry CreateRegistry()
        => new PredicateRegistry(TestExpression.LibraryProbe);

    [Test]
    [TestCase("equal-to", typeof(EqualTo))]
    [TestCase("equivalent-to", typeof(EquivalentTo))]
    [TestCase("greater-than", typeof(GreaterThan))]
    public void Execute_PredicateName_Valid(string value, Type expected)
            => Assert.That(CreateRegistry().Resolve(value), Is.EqualTo(expected));

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
        => Assert.That(CreateRegistry().Resolve(value), Is.EqualTo(expected));

    [Test]
    [TestCase("foo")]
    [TestCase("foo-to-bar")]
    [TestCase("foo - to - bar")]
    [TestCase("boolean-is-and")]
    [TestCase("boolean-is-not")]
    [TestCase("boolean-is-or")]
    [TestCase("boolean-is-xor")]
    public void Execute_PredicateName_Invalid(string value)
        => Assert.That(() => CreateRegistry().Resolve(value), Throws.TypeOf<NotImplementedFunctionException>());

    [Test]
    public void Execute_RenamedPredicateAndLegacyAlias_ResolveToSameImplementation()
    {
        var manifestPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "predicates-rename.json");
        var renames = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(manifestPath))!;
        var registry = CreateRegistry();

        Assert.Multiple(() =>
        {
            foreach (var (legacyName, canonicalName) in renames)
                Assert.That(
                    registry.Resolve(legacyName),
                    Is.EqualTo(registry.Resolve(canonicalName)),
                    $"Legacy predicate '{legacyName}' should alias '{canonicalName}'.");
        });
    }
}
