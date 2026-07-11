using Expressif.Functions.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Functions.Text;

[TestFixture]
public class PaddingFunctionsTest
{
    [Conformance]
    public void PadRight_Valid(object value, int length, char character, string expected)
        => Assert.That(new PadRight(() => (length), () => (character))
            .Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void PadLeft_Valid(string value, int length, char character, string expected)
        => Assert.That(new PadLeft(() => (length), () => (character))
            .Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void PadCenter_Valid(string value, int length, char character, string expected)
        => Assert.That(new PadCenter(() => (length), () => (character))
            .Evaluate(value), Is.EqualTo(expected));
}
