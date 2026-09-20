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
using Expressif.Library.Temporal;
using Expressif.Library.Text;
using Expressif.Testing.Conformance;
using NUnit.Framework.Internal;

namespace Expressif.Testing.Text.Selection;

[TestFixture]
public class SubstringFunctionsTest
{
    [TestCase(true, "|", 1, "a|b", 1)]
    [TestCase(false, "|", 1, "c", 1)]
    [TestCase(true, "!", 1, "", 0)]
    [TestCase(false, "!", 1, "", 0)]
    [TestCase(true, "", 1, "", 0)]
    [TestCase(false, "", 1, "a|b|c", 0)]
    public void Substring_Count_EvaluatedOnceWhenFound(bool before, string substring, int count, string expected, int expectedEvaluations)
    {
        var evaluations = 0;
        Func<int> getCount = () => { evaluations++; return count; };
        BaseSubstringFunction function = before
            ? new BeforeSubstring(() => substring, getCount)
            : new AfterSubstring(() => substring, getCount);

        var result = function.Evaluate("a|b|c");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(expected));
            Assert.That(evaluations, Is.EqualTo(expectedEvaluations));
        });
    }

    [Conformance]
    public void BeforeSubstring_Valid(string value, string substring, string expected)
        => Assert.That(new BeforeSubstring(() => substring).Evaluate(value)
            , Is.EqualTo(expected));

    [Conformance]
    public void BeforeSubstring_Valid_Position(string value, string substring, int position, string expected)
        => Assert.That(new BeforeSubstring(() => substring, () => position).Evaluate(value)
            , Is.EqualTo(expected));

    [Conformance]
    public void AfterSubstring_Valid(string value, string substring, string expected)
        => Assert.That(new AfterSubstring(() => substring).Evaluate(value)
            , Is.EqualTo(expected));

    [Conformance]
    public void AfterSubstring_Valid_Position(string value, string substring, int position, string expected)
        => Assert.That(new AfterSubstring(() => substring, () => position).Evaluate(value)
            , Is.EqualTo(expected));
}
