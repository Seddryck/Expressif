using Expressif.Library.Numeric.Arithmetic;
using Expressif.Library.Numeric.Classification;
using Expressif.Library.Numeric.Conversion;
using Expressif.Library.Numeric.Formatting;
using Expressif.Library.Numeric.Rounding;
using Expressif.Functions;
using Expressif.Library.IO;
using Expressif.Library.Numeric;
using Expressif.Testing.Conformance;
using Expressif.Values;
using Expressif.Values.Casters;

namespace Expressif.Testing.Numeric.Arithmetic;

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
            TestExpression.CreateClosed(expression, new Context()).Evaluate(null),
            Is.EqualTo(expected));
}
