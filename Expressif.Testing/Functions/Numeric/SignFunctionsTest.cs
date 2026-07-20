using Expressif.Functions.Numeric;
using Expressif.Testing.Conformance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Functions.Numeric;

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
