using Expressif.Library.Text.Casing;
using Expressif.Library.Text.Character;
using Expressif.Library.Text.Concatenation;
using Expressif.Library.Text.Conversion;
using Expressif.Library.Text.Counting;
using Expressif.Library.Text.Encoding;
using Expressif.Library.Text.Filtering;
using Expressif.Library.Text.Masking;
using Expressif.Library.Text.Normalization;
using Expressif.Library.Text.Padding;
using Expressif.Library.Text.Partitioning;
using Expressif.Library.Text.Selection;
using Expressif.Library.Text.Tokenization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Library.Temporal;
using Expressif.Library.Text;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Text.Filtering;

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
