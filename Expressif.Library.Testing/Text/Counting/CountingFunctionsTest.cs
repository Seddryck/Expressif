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

namespace Expressif.Testing.Text.Counting;

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

    [Conformance]
    public void TokenCountLexical_Valid(object? value, int expected)
        => Assert.That(new TokenCountLexical().Evaluate(value), Is.EqualTo(expected));
}
