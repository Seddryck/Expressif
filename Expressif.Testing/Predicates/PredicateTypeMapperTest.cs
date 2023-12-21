using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    [TestCase("equivalent-to", typeof(EquivalentTo))]
    [TestCase("Equivalent-To", typeof(EquivalentTo))]
    [TestCase("text-is-equivalent-to", typeof(EquivalentTo))]
    public void Execute_PredicateNameVariations_Valid(string value, Type expected)
        => Assert.That(new PredicateTypeMapper().Execute(value), Is.EqualTo(expected));

    [Test]
    [TestCase("foo")]
    [TestCase("foo-to-bar")]
    [TestCase("foo - to - bar")]
    public void Execute_PredicateName_Invalid(string value)
        => Assert.That(() => new PredicateTypeMapper().Execute(value), Throws.TypeOf<NotImplementedFunctionException>());
}
