using Expressif.Functions.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Functions.Text;

[TestFixture]
public class RetainFunctionsTest
{
    [Conformance]
    public void RetainNumeric_Valid(string value, string expected)
       => Assert.That(new RetainNumeric().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void RetainNumericSymbol_Valid(string value, string expected)
        => Assert.That(new RetainNumericSymbol().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void RetainAlpha_Valid(string value, string expected)
        => Assert.That(new RetainAlpha().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void RetainAlphaNumeric_Valid(string value, string expected)
        => Assert.That(new RetainAlphaNumeric().Evaluate(value), Is.EqualTo(expected));
}
