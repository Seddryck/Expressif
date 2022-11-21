using Expressif;
using Expressif.Values;
using Expressif.Values.Special;
using System.Data;
using System.Diagnostics;

namespace Expressif
{
    public class PredicationTest
    {
        [SetUp]
        public void Setup()
        { }

        [Test]
        public void Evaluate_SinglePredicateWithoutParameter_Valid()
        {
            var predication = new Predication("lower-case");
            var result = predication.Evaluate("Nikola Tesla");
            Assert.That(result, Is.False);
        }

        [Test]
        public void Evaluate_SinglePredicateWithOneParameter_Valid()
        {
            var predication = new Predication("starts-with(Nik)");
            var result = predication.Evaluate("Nikola Tesla");
            Assert.That(result, Is.True);
        }

        [Test]
        public void Evaluate_IntervalAsParameter_Valid()
        {
            var predication = new Predication("within-interval([0;20[)");
            var result = predication.Evaluate(15);
            Assert.That(result, Is.True);
        }

        [Test]
        public void Evaluate_CultureAsParameter_Valid()
        {
            var predication = new Predication("matches-date(fr-fr)");
            var result = predication.Evaluate("28/12/1978");
            Assert.That(result, Is.True);
        }
    }
}