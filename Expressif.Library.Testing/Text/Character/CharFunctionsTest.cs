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

namespace Expressif.Testing.Text.Character;

[TestFixture]
public class CharFunctionsTest
{
    [Conformance]
    public void RemoveChars_Valid(string? value, char charToRemove, string expected)
        => Assert.That(new RemoveChars(() => (charToRemove)).Evaluate(value)
            , Is.EqualTo(expected));

    [Conformance]
    public void ReplaceChars_Valid(string? value, char charToReplace, char replacingChar, string expected)
        => Assert.That(new ReplaceChars(() => charToReplace, () => replacingChar).Evaluate(value)
            , Is.EqualTo(expected));

    [Conformance]
    public void FilterChars_Valid_Chars(string? value, char[] filter, string expected)
        => Assert.That(new FilterChars(() => filter).Evaluate(value)
            , Is.EqualTo(expected));

    [Conformance]
    public void FilterChars_Valid_String(string? value, string filter, string expected)
        => Assert.That(new FilterChars(() => filter).Evaluate(value)
            , Is.EqualTo(expected));

    [Conformance]
    public void CollapseWhitespace_Valid(string? value, string expected)
        => Assert.That(new CollapseWhitespace().Evaluate(value)
            , Is.EqualTo(expected));

    [Conformance]
    public void CleanWhitespace_Valid(string? value, string expected)
        => Assert.That(new CleanWhitespace().Evaluate(value)
            , Is.EqualTo(expected));
}
