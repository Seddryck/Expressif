using Expressif.Functions;
using Expressif.Functions.Numeric;
using Expressif.Testing.Conformance;
using Expressif.Values;
using Expressif.Values.Casters;

namespace Expressif.Testing.Functions.Numeric;

[TestFixture]
public class PercentChangeFunctionTest
{
    [Conformance]
    public void PercentChange_Valid_Previous(object value, object previous, decimal? expected)
        => Assert.That(
            new PercentChange(() => new Caster().Cast<decimal>(previous)).Evaluate(value),
            Is.EqualTo(expected));

    [TestCase("105 | percent-change(100)", 5)]
    [TestCase("80 | percent-change(100)", -20)]
    [TestCase("100 | percent-change(0)", null)]
    [TestCase("100 | percent-change(\"abc\")", null)]
    public void Instantiate_Expression_Valid(string expression, decimal? expected)
        => Assert.That(
            BindingTestAdapter.ExecutableClosed(expression, new Context()).Evaluate(null),
            Is.EqualTo(expected));
}
