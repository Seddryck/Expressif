using Expressif.Functions.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Functions.Text;

[TestFixture]
public class CountingFunctionsTest
{
    [Conformance]
    public void Length_Valid(object? value, int? expected)
        => Assert.That(new Length().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void CountDistinctChars_Valid(object? value, int? expected)
        => Assert.That(new CountDistinctChars().Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void CountSubstring_Valid(object? value, string? substring, int? expected)
        => Assert.That(new CountSubstring(() => substring).Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void TokenCount_Valid(object? value, int expected)
        => Assert.That(new TokenCount().Evaluate(value), Is.EqualTo(expected));
}
