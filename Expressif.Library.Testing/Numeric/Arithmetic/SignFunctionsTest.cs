using Expressif.Library.Numeric.Arithmetic;
using Expressif.Library.Numeric.Classification;
using Expressif.Library.Numeric.Conversion;
using Expressif.Library.Numeric.Formatting;
using Expressif.Library.Numeric.Rounding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Library.IO;
using Expressif.Library.Numeric;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Numeric.Arithmetic;

[TestFixture]
public class SignFunctionsTest
{
    [Conformance]
    public void Sign_Valid(object? value, decimal? expected)
        => Assert.That(new Sign().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Absolute_Valid(object? value, decimal? expected)
        => Assert.That(new Absolute().Evaluate(value), Is.EqualTo(expected));
}
